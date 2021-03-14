using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RevoltBot
{
    public class Config
    {
        public string Prefix;

        public Task Save()
            => File.WriteAllTextAsync("./config.json", JsonConvert.SerializeObject(this));
    }
}