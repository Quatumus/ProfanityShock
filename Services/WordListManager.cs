using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProfanityShock.Data;

namespace ProfanityShock.Services
{
    internal class WordListManager
    {
        public static List<string> words = new List<string>();

        private static bool _hasBeenInitialized = false;
        private static async Task Init()
        {
            if (_hasBeenInitialized)
                return;
            words = await WordListRepository.ListAsync();
            _hasBeenInitialized = true;
        }

        public static List<string> GetList()
        {
            Init().Wait();
            return words;
        }
    }
}
