using System.Linq;
using System.Threading.Tasks;
using Hardware.Info;
using RevoltApi;

namespace RevoltBot
{
    public static class Annoy
    {
        public static async Task Run()
        {
            HardwareInfo hardwareInfo = new HardwareInfo();
            hardwareInfo.RefreshCPUList();
            hardwareInfo.RefreshNetworkAdapterList();
            hardwareInfo.RefreshMemoryList();
            var cpu = hardwareInfo.CpuList.First();
            ulong h = 1024;
            string networks = "";
            foreach (var network in hardwareInfo.NetworkAdapterList)
            {
                networks += $@"> {network.Name} @ {network.IPAddressList.FirstOrDefault()}
";
            }

            string ramSticks = "";
            foreach (var ramStick in hardwareInfo.MemoryList)
            {
                ramSticks +=
                    $@"> {ramStick.Manufacturer} {ramStick.FormFactor} {ramStick.Capacity / h / h}MB @ {ramStick.Speed}Mhz
";
            }

            string drives = "";
            foreach (var drive in hardwareInfo.DriveList)
            {
                drives += $@"> {drive.Model} @ {drive.Name} {drive.Size / h / h / h}GB
";
            }

            while (true)
            {
                if (Program.Config.AnnoyToggle)
                {
                    hardwareInfo.RefreshMemoryStatus();
                    await Program.Client.Self.EditProfileAsync(new UserInfo()
                    {
                        Status = new()
                        {
                            Text = hardwareInfo.MemoryStatus.AvailablePhysical / h / h + "MB free of " +
                                   hardwareInfo.MemoryStatus.TotalPhysical / h / h + "MB"
                        },
                        Profile = new()
                        {
                            Content = $@"# Compute
**Cpu:** {cpu.Name}
**Physical Ram:** {hardwareInfo.MemoryStatus.AvailablePhysical / h / h}MB free of {hardwareInfo.MemoryStatus.TotalPhysical / h / h}MB
### Ram Sticks:
{ramSticks}
### Networks:
{networks}
### Drives:
{drives}"
                        }
                    });
                }

                await Task.Delay(30000);
            }
        }
    }
}