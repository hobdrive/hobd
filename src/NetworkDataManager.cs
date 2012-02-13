using System;
using System.Net;
using System.IO;
using System.Collections.Generic;

namespace hobd{

public class NetworkDataManager
{
    string AccessKey;
    
    public NetworkDataManager(string AccessKey)
    {
        this.AccessKey = AccessKey;
    }

    
    public int CreateNew(bool force, string VehicleName, string VehicleType, string vin, string Lang)
    {
        //return RESULT_CONNFAIL;
        if (!force)
        {
            return Result.RESULT_EXISTS;
        }
        this.AccessKey = "";
        return Result.RESULT_OK;
    }

    public int Validate()
    {
        //return RESULT_CONNFAIL;

        //return RESULT_INVALID_KEY;

        return Result.RESULT_OK;
    }

}

}