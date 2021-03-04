using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using RevoltApi;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Console = Log73.Console;

namespace RevoltBot
{
    public class Config
    {
        public string Prefix;

        public Task Save()
            => File.WriteAllTextAsync("./config.json", JsonConvert.SerializeObject(this));
    }
}