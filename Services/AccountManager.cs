using ProfanityShock.Config;
using ProfanityShock.Data;
using ProfanityShock.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfanityShock.Services
{
    public static class AccountManager
    {
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

        public static async Task LoadSave()
        {
            if (await AccountRepository.LoadAsync() != null)
            {
                AccountConfig = await AccountRepository.LoadAsync();
                NetManager.ChangeToken(AccountConfig.Token);
            }
        }

        public static async Task SaveConfig()
        {
            await AccountRepository.SaveItemAsync(AccountConfig);
            NetManager.ChangeToken(AccountConfig.Token);
        }
    }
}
