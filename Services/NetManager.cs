using ProfanityShock.Config;
using ProfanityShock.Data;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using ProfanityShock;

namespace ProfanityShock.Services
{
    public static class NetManager
    {

        static HttpClient client = new HttpClient
        {
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
