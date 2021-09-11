using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Taco
{
    public class Config
    {
        public string MongoUrl;
        public string BotToken;
        public string DatabaseName;
        public string Presence = "Idle";
        public string Profile = "";
        public string Status = "";
        public List<string> CodeOfConduct = new();
        public string UserId;

        public Task Save()
            => File.WriteAllTextAsync("./config.json", JsonConvert.SerializeObject(this));
    }
}