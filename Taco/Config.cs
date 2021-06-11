using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Taco
{
    public class Config
    {
        public string BingSnrCode;
        public string BingCoreClr;
        public string BingCoreFx;
        public List<string> BingReminderChannels = new();
        public string MongoUrl;
        public string DatabaseName;
        public bool AnnoyToggle;
        public string Presence = "Idle";
        public string Profile = "Cry about it nerd.";
        public string Status = "Amogus";
        public int UpdateTime = 20000;
        public List<string> CodeOfConduct = new();

        public Task Save()
            => File.WriteAllTextAsync("./config.json", JsonConvert.SerializeObject(this));
    }
}