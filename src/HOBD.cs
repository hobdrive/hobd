using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

using Fleux.Core;

namespace hobd
{
    class HOBD
    {
        public static ConfigData config;
        public static Engine engine;
        public static SensorRegistry Registry;
        public static HOBDTheme theme;
        public static UnitsConverter uConverter;
        public static NumberFormatInfo DefaultNumberFormat;

        static bool ReloadUI = true;

        static HOBD()
        {
            DefaultNumberFormat = new NumberFormatInfo();
            DefaultNumberFormat.NumberDecimalSeparator = ".";
            DefaultNumberFormat.PositiveInfinitySymbol = "∞";
        }

        static string appPath;
        public static string AppPath {
            get{
                if (appPath == null){
                    appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                    if (appPath.StartsWith("file:\\")) appPath = appPath.Substring(6);
                    if (appPath.StartsWith("file://")) appPath = appPath.Substring(7);
                    if (appPath.StartsWith("file:")) appPath = appPath.Substring(5);
                }
                return appPath;
            }
        }
#if WINCE
        [DllImport("coredll", EntryPoint="FindWindowW", SetLastError = true)]
		 private static extern int FindWindow(string cls, string name);
        
        [DllImport("coredll", EntryPoint="SetForegroundWindow", SetLastError = true)]
		 private static extern bool SetForegroundWindow(int handle); 
#else
        [DllImport("user32", EntryPoint="FindWindowA", SetLastError = true)]
		 private static extern int FindWindow(string cls, string name); 
        
        [DllImport("user32", EntryPoint="SetForegroundWindow", SetLastError = true)]
		 private static extern bool SetForegroundWindow(int handle); 
#endif

        static Dictionary<string, string> i18n = new Dictionary<string, string>();

        public static string t(string v)
        {
            string lv = null;
            if (v != null && !i18n.TryGetValue(v, out lv))
                lv = v;
            return lv;
        }

        public static string t(string v, string def)
        {
            string lv = def;
            if (v != null && !i18n.TryGetValue(v, out lv))
                lv = def;
            return lv;
        }

        private static void LoadLang(string lang)
        {
            try{
                Directory.GetFiles(Path.Combine(HOBD.AppPath, "lang"), lang + "*.lang").ToList().ForEach( (f) => {
                    var sr = new StreamReader(new FileStream( f, FileMode.Open));
                    while(true)
                    {
                        var line = sr.ReadLine();
                        if (line == null)
                            break;
                        var idx = line.IndexOf("=");
                        if (idx != -1){
                           var key = line.Substring(0, idx).Trim();
                           var val = line.Substring(idx+1).Trim();
                           //Logger.trace("HOBD", key + val);
                           i18n.Remove(key);
                           i18n.Add(key, val);
                        }
                    }
                    sr.Close();
                });
            }catch(Exception e){
                Logger.error("HOBD", "i18n init", e);
            }
        }
        
        public static void ReloadApp()
        {
            ReloadUI = true;
            Application.Exit();
        }

        public static void ReloadLang()
        {
            i18n.Clear();
            HOBD.LoadLang("en");
            if (config.Language != "en")
            {
                HOBD.LoadLang(config.Language);
            }
        }

        public static void ReloadUnits()
        {
            HOBD.uConverter = new UnitsConverter(HOBD.config.Units);
        }

        public static void ReloadTheme()
        {
            /* Migration: */
            if (config.Theme == "hobd.HOBDTheme")
                config.Theme = "themes/default.theme";
            // load theme
            HOBD.theme = HOBDTheme.LoadTheme(Path.Combine(HOBD.AppPath, config.Theme));
        }

        public static void ReloadVehicle()
        {
            var vehicle = config.GetVehicle(config.Vehicle);

            if (vehicle == null){
                Logger.error("HOBD", "Bad configuration: can't find vehicle " + config.Vehicle);
                vehicle = ConfigVehicleData.defaultVehicle;
            }
            
            Registry = new SensorRegistry();
            Registry.VehicleParameters = vehicle.Parameters;

            vehicle.Sensors.ForEach((provider) =>
                    {
                        Logger.trace("HOBD", "RegisterProvider: "+ provider);
                        try{
                            Registry.RegisterProvider(provider);
                        }catch(Exception e){
                            Logger.error("HOBD", "bad provider", e);
                        }
                    });
            
            EngineConnect();
        }

        public static void EngineConnect()
        {
			var veh = config.GetVehicle(config.Vehicle);
			
			if (veh == null) veh = new ConfigVehicleData();
        
            if (HOBD.engine == null)
                HOBD.engine = Engine.CreateInstance(veh.ECUEngine);
            
            IStream stream = null;
            if (config.Port.StartsWith("btspp"))
                stream = new BluetoothStream();
            else if (config.Port.StartsWith("tcp"))
                stream = new TCPStream();
            else
                stream = new SerialStream();
            
            engine.Deactivate();
            engine.Registry = Registry;
            engine.Init(stream, config.Port, "");
        }

        public static bool Init()
        {
            Logger.Init(Path.Combine(HOBD.AppPath, "log.txt"));
            
            try{
            
            int handle = FindWindow(null, HomePage.Title);
            if (handle != 0)
            {
                Logger.log("INFO", "HOBD", "App bring to foreground", null);
                SetForegroundWindow(handle);
                return false;
            }
            }catch(Exception e){}
                
            try{
                Logger.log("INFO", "HOBD", "App start", null);

                try{
                    config = new ConfigData(Path.Combine(HOBD.AppPath, "config.xml"));
                }catch(Exception e){
                    Logger.error("HOBD", "failure loading config.xml, using defaults", e);
                    config = new ConfigData();
                }
                
                Logger.SetLevel(config.LogLevel);

                ReloadLang();

                ReloadUnits();

                ReloadVehicle();
                
                int dpi_value;
                
                var bwidth = Screen.PrimaryScreen.Bounds.Width;
                if (bwidth > 1920) bwidth = 800;
                
                dpi_value = (int) (96f / bwidth * 480f);
                
                Logger.trace ("HOBD", "Bounds.Width: "+bwidth);
                
                if (config.DPI != 0)
                    dpi_value = config.DPI;
                FleuxApplication.TargetDesignDpi = dpi_value;

                ReloadTheme();

            }catch(Exception e){
                Logger.error("HOBD", "fatal failure, exiting", e);
                if (engine != null && engine.IsActive()) engine.Deactivate();
                return false;
            }
            return true;
        }

        public static void Run(string homepage)
        {
            while (ReloadUI)
            {
                try{
                    ReloadUI = false;
                    Thread main = new Thread( () => {
                        try{
                            FleuxPage home = (FleuxPage)Assembly.GetExecutingAssembly().CreateInstance(homepage);
                            FleuxApplication.Run(home);
                            home.Dispose();
                        }catch(Exception e){
                            Logger.error("HOBD", "fatal UI thread failure, exiting", e);
                        }
                    });
                    main.Start();
                    main.Join();
                }catch(Exception e){
                    Logger.error("HOBD", "fatal failure, exiting", e);
                    if (engine != null && engine.IsActive()) engine.Deactivate();
                }
            }

            if (engine != null)
                engine.Deactivate();
            if (Registry != null)
                Registry.Deactivate();
            config.Save();
            Logger.info("HOBD", "app exit");
            
#if !WINCE
            // hanged threads?
            Environment.Exit(0);
#endif
        }
        
    }
}
