﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;




namespace Externals
{
    public class CsvHelper
    {
        public string GetCSV(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string results = sr.ReadToEnd();
            sr.Close();

            return results;
        }

    }
}
