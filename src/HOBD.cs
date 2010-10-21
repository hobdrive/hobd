using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Fleux.Core;

namespace hobd
{
    sealed class HOBD
    {
        public static ConfigData config;
        public static Engine engine;
        public static SensorRegistry Registry;
        public static HOBDTheme theme = new HOBDTheme();
        
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
        
        [STAThread]
        private static void Main(string[] args)
        {

            try{
                try{
                    config = new ConfigData(Path.Combine(HOBD.AppPath, "config.xml"));
                }catch(Exception e){
                    Logger.error("HOBD", "failure loading config.xml, using defaults", e);
                    config = new ConfigData();
                }
                
                Logger.SetLevel(config.LogLevel);
                
                HOBD.engine = (Engine)Activator.CreateInstance(null, "hobd.OBD2Engine").Unwrap();
                
                IStream stream = null;
                if (config.Port.StartsWith("btspp"))
                    stream = new BluetoothStream();
                else
                    stream = new SerialStream();
                
                engine.Init(stream, config.Port);
                
                Registry = new SensorRegistry();
                Registry.RegisterProvider(new OBD2Sensors());
                Registry.RegisterProvider(new ToyotaSensors());
                engine.Registry = Registry;
                engine.Activate();
                
                //TODO: autoajust from screen size
                int dpi_value;
#if WINCE
                dpi_value = 96;
#else
                dpi_value = 96/2;
#endif
                if (config.DPI != 0)
                    dpi_value = config.DPI;
    
                FleuxApplication.TargetDesignDpi = dpi_value;
                FleuxApplication.Run(new HomePage());
                
                engine.Deactivate();
                config.Save();
            }catch(Exception e){
                Logger.error("HOBD", "fatal failure, exiting", e);
            }

        }
        
    }
}
