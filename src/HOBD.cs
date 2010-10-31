using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
        
        public static string Version {
            get{ 
                return "0.1";
            }
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

        [STAThread]
        private static void Main(string[] args)
        {

            int handle = FindWindow(null, HomePage.Title);
            if (handle != 0)
            {
                MessageBox.Show("handle: "+handle);
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
                HOBD.theme = (HOBDTheme)Assembly.GetExecutingAssembly().CreateInstance(config.Theme);
                
                FleuxApplication.Run(new HomePage());
                
                engine.Deactivate();
                config.Save();
            }catch(Exception e){
                Logger.error("HOBD", "fatal failure, exiting", e);
                if (engine != null && engine.IsActive()) engine.Deactivate();
            }

        }
        
    }
}
