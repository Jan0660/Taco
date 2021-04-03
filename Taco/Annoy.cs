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
            while (true)
            {
                hardwareInfo.RefreshMemoryStatus();
                hardwareInfo.RefreshCPUList();
                ulong h = 1024;
                var cpu = hardwareInfo.CpuList.First();
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
**Physical Ram:** {hardwareInfo.MemoryStatus.AvailablePhysical / h / h}MB free of {hardwareInfo.MemoryStatus.TotalPhysical / h / h}MB
**Cpu:** {cpu.Name}"
                    }
                });
                await Task.Delay(1000);
            }
        }
    }
}