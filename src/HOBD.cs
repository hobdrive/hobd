using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using Fleux.Core;

namespace hobd
{
    sealed class HOBD
    {
        public static ConfigData config;
        public static Engine engine;
        public static SensorRegistry Registry;
        public static HOBDTheme theme;
        public static UnitsConverter uConverter;
        public static NumberFormatInfo DefaultNumberFormat;

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
            if (!i18n.TryGetValue(v, out lv))
                lv = v;
            return lv;
        }

        private static void LoadLang(string lang)
        {
            try{
                var sr = new StreamReader(new FileStream( Path.Combine(HOBD.AppPath, lang + ".lang"), FileMode.Open));
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
            }catch(Exception e){
                Logger.error("HOBD", "i18n init", e);
            }
        }
        
        [STAThread]
        private static void Main(string[] args)
        {

            int handle = FindWindow(null, HomePage.Title);
            if (handle != 0)
            {
                Logger.log("INFO", "HOBD", "App bring to foreground", null);
                SetForegroundWindow(handle);
                return;
            }
                
            try{
                Logger.log("INFO", "HOBD", "App start", null);

                try{
                    config = new ConfigData(Path.Combine(HOBD.AppPath, "config.xml"));
                }catch(Exception e){
                    Logger.error("HOBD", "failure loading config.xml, using defaults", e);
                    config = new ConfigData();
                }
                
                Logger.SetLevel(config.LogLevel);

                HOBD.LoadLang("en");
                if (config.Language != "en")
                {
                    HOBD.LoadLang(config.Language);
                }

                HOBD.uConverter = new UnitsConverter(HOBD.config.Units);

                var vehicle = config.GetVehicle(config.Vehicle);

                if (vehicle == null){
                    Logger.error("HOBD", "Bad configuration: can't find vehicle " + config.Vehicle);
                    vehicle = ConfigVehicleData.defaultVehicle;
                }
                
                HOBD.engine = (Engine)Assembly.GetExecutingAssembly().CreateInstance(vehicle.ECUEngine);
                
                IStream stream = null;
                if (config.Port.StartsWith("btspp"))
                    stream = new BluetoothStream();
                else
                    stream = new SerialStream();
                
                engine.Init(stream, config.Port);
                                
                Registry = new SensorRegistry();
                Registry.VehicleParameters = vehicle.Parameters;

                vehicle.Sensors.ForEach((provider) =>
                        {
                            Logger.trace("HOBD", "RegisterProvider: "+ provider);
                            try{
                                Registry.RegisterProvider((SensorProvider)Assembly.GetExecutingAssembly().CreateInstance(provider));
                            }catch(Exception e){
                                Logger.error("HOBD", "bad provider", e);
                            }
                        });
                
                engine.Registry = Registry;
                
                int dpi_value;
                dpi_value = 96 / (Screen.PrimaryScreen.Bounds.Height / 272);
                if (config.DPI != 0)
                    dpi_value = config.DPI;
                FleuxApplication.TargetDesignDpi = dpi_value;

                /* Migration: */
                if (config.Theme == "hobd.HOBDTheme")
                    config.Theme = "themes/default.theme";
                // load theme
                HOBD.theme = HOBDTheme.LoadTheme(Path.Combine(HOBD.AppPath, config.Theme));

                
                FleuxApplication.Run(new HomePage());

                
            }catch(Exception e){
                Logger.error("HOBD", "fatal failure, exiting", e);
                if (engine != null && engine.IsActive()) engine.Deactivate();
            }finally{
                if (engine != null)
                    engine.Deactivate();
                if (Registry != null)
                    Registry.Deactivate();
                config.Save();
                Logger.error("HOBD", "app exit");
            }

        }
        
    }
}
