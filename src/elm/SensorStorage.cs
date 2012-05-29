namespace hobd{

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;


/**
 * Sensor storange is responsible for tracking IPersistentSensor sensors
 * storing and restoring their values
 */
public class SensorStorage
{
    SensorRegistry Registry;
    string FilePath;

    public SensorStorage(string filePath)
    {
        this.FilePath = filePath;
    }

    public void Attach(SensorRegistry registry)
    {
        if (this.Registry != null)
            throw new Exception("Can't attach: Invalid State");
        this.Registry = registry;

        try{
            var reader = new StreamReader(new FileStream( FilePath, FileMode.Open));

            while(true){
                var line = reader.ReadLine();
                if (line == null)
                    break;
                int vali = line.IndexOf(" ");
                string sname = line.Substring(0, vali);
                string value = line.Substring(vali+1);
                // backward compat patch between 0.5 -> 0.6
                if (sname == "DistanceRun" || sname == "FuelConsumed" || sname == "TripTime" || sname == "IdleTime")
                {
                    sname = "Common."+sname;
                }

                var sensor = Registry.Sensor(sname);
                if (sensor != null && sensor is IPersistentSensor){
                    try{
                        ((IPersistentSensor)sensor).RestoreState(value);
                        Registry.TriggerListeners(sensor);
                    }catch(Exception e){
                        Logger.error("SensorStorage", "fail on RestoreState: "+sname, e);
                    }
                }else{
                    Logger.error("SensorStorage", "bad sensor: "+sname);
                }
            }
            reader.Close();
        }catch(Exception e){
            Logger.error("SensorStorage", "error reading", e);
        }
    }

    public void StoreState()
    {
        var sensors = Registry.EnumerateSensors().Where(s => s is IPersistentSensor).ToList();

        try{
            var writer = new StreamWriter(new FileStream( FilePath, FileMode.Create));
            
            sensors.ForEach(s => {
                writer.WriteLine(s.ID + " " + ((IPersistentSensor)s).StoreState());
            });
            writer.Close();
        }catch(Exception e){
            Logger.error("SensorStorage", "error writing", e);
        }
    }

    public void Detach()
    {
        this.Registry = null;
    }

}

}