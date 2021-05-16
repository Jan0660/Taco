using System.Net;
using System.Text.RegularExpressions;
using System.Timers;

namespace Taco
{
    public static class BingReminder
    {
        private static Timer _timer;

        public static void Init()
        {
            _timer = new Timer(15d * 1000 * 1000);
            // _timer = new Timer(10d * 1000);
            _timer.AutoReset = true;
            _timer.Elapsed += (sender, args) =>
            {
                var content = new WebClient().DownloadString("https://www.bing.com/version");
                var matches = new Regex("(?<=<td>build</td><td>)(.+?)(?=</td>)").Matches(content);
                var con = Program.Config;
                if (con.BingCoreClr != matches[1].ToString()
                    | con.BingCoreFx != matches[2].ToString())
                {
                    foreach (var channel in con.BingReminderChannels)
                    {
                        Program.Client.Channels.SendMessageAsync(channel, $@"> # Bing
> **SNRCode:** {con.BingSnrCode} => {matches[0]}
> **CoreCLR:** {con.BingCoreClr} => {matches[1]}
> **CoreFX:** {con.BingCoreFx} => {matches[2]}");
                    }

                    con.BingSnrCode = matches[0].ToString();
                    con.BingCoreClr = matches[1].ToString();
                    con.BingCoreFx = matches[2].ToString();
                    con.Save().Wait();
                }
            };
            _timer.Start();
        }
    }
}