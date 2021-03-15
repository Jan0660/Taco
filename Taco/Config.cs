using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RevoltBot
{
    public class Config
    {
        public string BingSnrCode;
        public string BingCoreClr;
        public string BingCoreFx;
        public List<string> BingReminderChannels = new();

        public Task Save()
            => File.WriteAllTextAsync("./config.json", JsonConvert.SerializeObject(this));
    }
}