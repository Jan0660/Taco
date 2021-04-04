using System.Linq;
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

                await Task.Delay(30000);
            }
        }

        public static async Task Update()
        {
            var cpu = HardwareInfo.CpuList.First();
            HardwareInfo.RefreshMemoryStatus();
            await Program.Client.Self.EditProfileAsync(new UserInfo()
            {
                Status = new()
                {
                    Text = Program.Config.Status ?? HardwareInfo.MemoryStatus.AvailablePhysical / h / h + "MB free of " +
                           HardwareInfo.MemoryStatus.TotalPhysical / h / h + "MB",
                    Presence = Program.Config.Presence
                },
                Profile = new()
                {
                    Content = Program.Config.Profile ?? $@"# Compute
**Cpu:** {cpu.Name}
**Physical Ram:** {HardwareInfo.MemoryStatus.AvailablePhysical / h / h}MB free of {HardwareInfo.MemoryStatus.TotalPhysical / h / h}MB
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