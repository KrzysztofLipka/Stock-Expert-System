using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace DataSerializer.ExternalServices
{
    public static class ApiHelper
    {
        public static HttpClient ApiClient { get; set; } 

        public static void InitClient() {
            ApiClient = new HttpClient();
            ApiClient.DefaultRequestHeaders.Accept.Clear();
            ApiClient.DefaultRequestHeaders.Accept
                .Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

    }
}
