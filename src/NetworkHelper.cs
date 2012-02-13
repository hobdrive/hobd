using System;
using System.Net;
using System.IO;
using System.Collections.Generic;

namespace hobd{

public class NetworkHelper
{
    static NetworkHelper(){
        System.Net.ServicePointManager.Expect100Continue = false;
    }

    public static Result SendGet(string url, IDictionary<string, string> param)
    {
        var p = "";
        foreach(var n in param.Keys)
            p += Uri.EscapeUriString(n) + "=" + Uri.EscapeUriString(param[n]);
        return SendGet(url + "?" + p);
    }

    public static Result SendGet(string url)
    {
        try{

        HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
        wr.Method = "GET";
        using(var response = wr.GetResponse() as HttpWebResponse)
        {
            using(var sr = new StreamReader( response.GetResponseStream(), System.Text.Encoding.UTF8))
            {
                return new Result(Result.RESULT_OK, (int)response.StatusCode, sr.ReadToEnd());
            }
        }

        }catch(Exception e){
            return new Result(Result.RESULT_ERR, 0, e.Message);
        }
    }

    public static Result SendPost(string url, string body)
    {
        return SendPost(url, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(body)));
    }

    public static Result SendPost(string url, Stream body)
    {
        try{

        HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
        wr.Method = "POST";
        wr.ContentType = "application/x-www-form-urlencoded";
        using(Stream stream = wr.GetRequestStream())
        {
            int num;
            byte[] buffer = new byte[256];
            while ((num = body.Read(buffer, 0, buffer.Length)) != 0)
            {
                stream.Write(buffer, 0, num);
            }
        }
        using(var response = wr.GetResponse() as HttpWebResponse)
        {
            using(var sr = new StreamReader( response.GetResponseStream(), System.Text.Encoding.UTF8))
            {
                return new Result(Result.RESULT_OK, (int)response.StatusCode, sr.ReadToEnd());
            }
        }

        }catch(Exception e){
            return new Result(Result.RESULT_ERR, 0, e.Message);
        }
    }

}

public class Result
{
    public const int RESULT_OK = 0;
    public const int RESULT_CONNFAIL = 1;
    public const int RESULT_EXISTS = 2;
    public const int RESULT_INVALID_KEY = 3;
    public const int RESULT_ERR = 4;

    public Result(int result, int status, string data)
    {
        ResultCode = result;
        Status = status;
        Data = data;
    }
    public int ResultCode;
    public int Status;
    public string Data;
}

}