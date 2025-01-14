using ProfanityShock.Config;
using ProfanityShock.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfanityShock.Services
{
    public static class AccountManager
    {
        private static bool _hasBeenInitialized = false;

        static AppConfig AccountConfig = new AppConfig()
        {
            Email = "",
            Password = "",
            Token = "",
            Backend = new Uri("https://api.openshock.app")
        };

        public static AppConfig GetConfig()
        {
            return AccountConfig;
        }

        private static async Task Init()
        {
            if (_hasBeenInitialized)
                return;

            if (await AccountRepository.LoadAsync() != null)
            {
                AccountConfig = await AccountRepository.LoadAsync();
            }

            _hasBeenInitialized = true;
        }

        public static async Task SaveConfig()
        {
            await AccountRepository.SaveItemAsync(AccountConfig);
        }
    }
}
