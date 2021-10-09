using Revolt.Commands.Builders;
using Revolt.Commands.Info;

namespace Revolt.Commands
{
    internal interface IModuleBase
    {
        void SetContext(ICommandContext context);

        void BeforeExecute(CommandInfo command);
        
        void AfterExecute(CommandInfo command);

        void OnModuleBuilding(CommandService commandService, ModuleBuilder builder);
    }
}
