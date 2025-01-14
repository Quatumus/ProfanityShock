﻿using ProfanityShock.Config;
using ProfanityShock.Data;


namespace ProfanityShock.Services
{
    public static class NetManager
    {

        static HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("https://api.openshock.com/"),
            DefaultRequestHeaders =
            {
                { "accept", "application/json" },
                // { "User-Agent", GetUserAgent() },
                { "OpenShockToken", "" }
            }
        };

        



        public static HttpClient GetClient()
        {
            return client;
        }

        public static void ChangeToken(string token)
        {
            client.DefaultRequestHeaders.Remove("OpenShockToken");
            client.DefaultRequestHeaders.Add("OpenShockToken", token);
        }

    }
}
