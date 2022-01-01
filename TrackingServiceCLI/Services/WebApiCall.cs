using System;
using System.Collections.Generic;
using System.Net.Http;

namespace TrackingServiceCLI.Services
{
    internal class WebApiCall
    {
        /// <summary>
        /// Async calls to web api
        /// </summary>
        /// <param name="baseAddress">Api base address</param>
        /// <param name="urls">list of urls to make request to</param>
        internal static void CallWebApi(string baseAddress, List<string> urls)
        {
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(baseAddress);
            foreach (var url in urls)
                client.GetAsync(url);
        }
    }
}
