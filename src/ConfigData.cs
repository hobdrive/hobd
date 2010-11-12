using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;

namespace hobd
{

public class ConfigVehicleData
{
    public static ConfigVehicleData defaultVehicle = new ConfigVehicleData();
    
    public string File{get; set;}
    
    public string Name{get; set;}
    public string ECUEngine{get; set;}
    
    public List<string> Sensors{ get; set;}
    
    Dictionary<string, string> parameters = new Dictionary<string, string>();
    public IDictionary<string, string> Parameters{ get{ return parameters;}}
    
    public ConfigVehicleData()
    {
        Name = "Default Vehicle";
        ECUEngine = "hobd.OBD2Engine";
        Sensors = new List<string>();
    }
}
    
public class ConfigData
{
    
    public string Port {get; set;}
    public string LogLevel  {get; set;}
    
    List<string> vehicle_files = new List<string>();
    List<ConfigVehicleData> vehicles = new List<ConfigVehicleData>();
    Dictionary<string, ConfigVehicleData> vehicles_map = new Dictionary<string, ConfigVehicleData>();
    public IEnumerable<ConfigVehicleData> Vehicles {get{ return vehicles;}}
    
    public string Vehicle {get; set;}
    public string Token {get; set;}
    public int DPI {get; private set;}
    public string Language {get; set;}
    public string Units {get; set;}
    public string Theme {get; set;}
    public string Layout {get; set;}
    public bool HighPerformance {get; set;}
    
    string file;
    
    void init()
    {
        this.Port = "COM1";
        this.LogLevel = "ERROR";
        this.Vehicle = "OBD-II compatible, 1.6l";

        this.file = Path.Combine(HOBD.AppPath, "config.xml");
        
        DPI = 0;
        Language = "en";
        Units = "metric";
        Theme = "themes/default.theme";
        Layout = "default-landscape.layout";
    }

    public ConfigData()
    {
        init();
        // default data init
        var v_file = "default.vehicles";
        this.vehicle_files.Add(v_file);
        try{
            ReadVehicles(v_file);
        }catch(Exception e){
            Logger.error("ConfigData", "fault reading vehicle from " + v_file, e);
        }
    }
    
    public ConfigData(string file)
    {
        init();
        
        XmlReaderSettings xrs = new XmlReaderSettings();
        xrs.IgnoreWhitespace = true;
        xrs.IgnoreComments = true;

        this.file = file;
        XmlReader reader = XmlReader.Create(file, xrs);
        
        reader.ReadStartElement("hobd");

        while(true){
            if (reader.NodeType != XmlNodeType.Element){
                if (!reader.Read())
                    break;
                continue;
            }
            switch (reader.Name) {
                case "log-level":
                    this.LogLevel = reader.ReadElementContentAsString();    
                    break;
                case "port":
                    this.Port = reader.ReadElementContentAsString();
                    break;
                case "vehicles":
                    var v_file = reader.ReadElementContentAsString();
                    this.vehicle_files.Add(v_file);
                    try{
                        ReadVehicles(v_file);
                    }catch(Exception e){
                        Logger.error("ConfigData", "fault reading vehicle from " + v_file, e);
                    }
                    break;
                case "vehicle":
                    this.Vehicle = reader.ReadElementContentAsString();
                    break;
                    
                case "drivehub-token":
                    this.Token = reader.ReadElementContentAsString();
                    break;                    
                case "dpi":
                    this.DPI = reader.ReadElementContentAsInt();
                    break;
                case "language":
                    this.Language = reader.ReadElementContentAsString();
                    break;
                case "units":
                    this.Units = reader.ReadElementContentAsString();
                    break;
                case "theme":
                    this.Theme = reader.ReadElementContentAsString();
                    break;
                case "layout":
                    this.Layout = reader.ReadElementContentAsString();
                    break;
                default:
                    reader.Read();
                    break;
            }
            
        }
        reader.Close();
        
    }
    
    void ReadVehicles(string vfile)
    {
        XmlReaderSettings xrs = new XmlReaderSettings();
        xrs.IgnoreWhitespace = true;
        xrs.IgnoreComments = true;

        XmlReader reader = XmlReader.Create(Path.Combine(Path.GetDirectoryName(file), vfile), xrs);
        
        reader.ReadStartElement("vehicles");

        while( reader.IsStartElement("vehicle") ){
            ConfigVehicleData v = new ConfigVehicleData();
            v.Name = reader.GetAttribute("name");
            
            reader.ReadStartElement();
            if ( reader.IsStartElement("obd") ){
                reader.ReadStartElement();
                v.ECUEngine = reader.ReadElementString("engine");
                while(reader.NodeType == XmlNodeType.Element)
                {
                    v.Sensors.Add(reader.ReadElementString("sensors"));
                }
                reader.ReadEndElement();
            }
            while (reader.NodeType != XmlNodeType.EndElement){
                if (reader.NodeType == XmlNodeType.Element){
                    var name = reader.Name;
                    var val = reader.ReadElementString();
                    v.Parameters.Add(name, val);
                }
            }
            reader.ReadEndElement();
            
            this.vehicles.Add(v);
            this.vehicles_map.Add(v.Name, v);
        }
        reader.Close();
    }
    
    public void Save()
    {
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        
        XmlWriter f = XmlWriter.Create(file, settings);
        
        f.WriteStartDocument();
        f.WriteStartElement("hobd");

            f.WriteElementString("log-level", this.LogLevel);

            f.WriteElementString("port", this.Port);

            vehicle_files.ForEach( (v) => f.WriteElementString("vehicles", v) );

            f.WriteElementString("vehicle", this.Vehicle);
            f.WriteElementString("drivehub-token", this.Token);

            if (this.DPI != 0)
                f.WriteElementString("dpi", this.DPI.ToString());
            f.WriteElementString("language", this.Language);
            f.WriteElementString("units", this.Units);
            f.WriteElementString("theme", this.Theme);
            f.WriteElementString("layout", this.Layout);

        f.WriteEndElement();
        f.WriteEndDocument();
        f.Close();

    }
    
    
    public ConfigVehicleData GetVehicle(string name)
    {
        ConfigVehicleData vehicle = null;
        vehicles_map.TryGetValue(name, out vehicle);
        return vehicle;
    }
    
}

}
