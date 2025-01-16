using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfanityShock.Services
{
    public class LiveViewInterface
    {
        public interface ILiveViewInterface
        {
            void SetText(string sentence, int confidence);
        }
    }
}
