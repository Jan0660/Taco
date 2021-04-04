using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hardware.Info;
using MongoDB.Driver;
using RevoltApi;

namespace RevoltBot
{
    public static class Annoy
    {
        private static readonly HardwareInfo HardwareInfo = new();
        private static string networks = "";
        private static string ramSticks = "";
        private static string drives = "";
        private const ulong h = 1024;
        public static async Task Run()
        {
            HardwareInfo.RefreshCPUList();
            HardwareInfo.RefreshNetworkAdapterList();
            HardwareInfo.RefreshMemoryList();
            foreach (var network in HardwareInfo.NetworkAdapterList)
            {
                networks += $@"> {network.Name} @ {network.IPAddressList.FirstOrDefault()}
";
            }

            foreach (var ramStick in HardwareInfo.MemoryList)
            {
                ramSticks +=
                    $@"> {ramStick.Manufacturer} {ramStick.FormFactor} {ramStick.Capacity / h / h}MB @ {ramStick.Speed}Mhz
";
            }

            foreach (var drive in HardwareInfo.DriveList)
            {
                drives += $@"> {drive.Model} @ {drive.Name} {drive.Size / h / h / h}GB
";
            }

            while (true)
            {
                if (Program.Config.AnnoyToggle)
                {
                    await Update();
                }

                await Task.Delay(Program.Config.UpdateTime);
            }
        }

        public static async Task Update()
        {
            var cpu = HardwareInfo.CpuList.First();
            HardwareInfo.RefreshMemoryStatus();
            string memory;
            if (Environment.OSVersion.Platform != PlatformID.Unix)
            {
                memory = HardwareInfo.MemoryStatus.AvailablePhysical / h / h + "MB free of " +
                         HardwareInfo.MemoryStatus.TotalPhysical / h / h + "MB";
            }
            else
            {
                var process = Process.Start(new ProcessStartInfo()
                {
                    FileName = "free",
                    RedirectStandardOutput = true
                });
                await process!.WaitForExitAsync();
                var output = await process.StandardOutput.ReadToEndAsync();
                var rgx = new Regex("[0-9]+");
                var matches = rgx.Matches(output);
                memory = $"{ulong.Parse(matches[5].Value) / h}MB free of {ulong.Parse(matches[0].Value) / h}MB";
            }
            await Program.Client.Self.EditProfileAsync(new UserInfo()
            {
                Status = new()
                {
                    Text = Program.Config.Status ?? memory,
                    Presence = Program.Config.Presence
                },
                Profile = new()
                {
                    Content = Program.Config.Profile ?? $@"# Compute
**Cpu:** {cpu.Name}
**Physical Ram:** {memory}
### Ram Sticks:
{ramSticks}
### Networks:
{networks}
### Drives:
{drives}"
                }
            });
        }
    }
}