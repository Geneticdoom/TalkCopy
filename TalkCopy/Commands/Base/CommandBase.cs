using TalkCopy.Commands.Interfaces;
using TalkCopy.Core.Handlers;
using Dalamud.Game.Command;

namespace TalkCopy.Commands.Base;

internal abstract class CommandBase : ICommand
{
    public abstract string Command { get; }
    public abstract string HelpMessage { get; }
    public abstract bool ShowInHelp { get; }

    public abstract void OnCommand(string command, string args);

    public CommandBase()
    {
        PluginHandlers.CommandManager.AddHandler(Command, new CommandInfo(OnCommand)
        {
            HelpMessage = HelpMessage,
            ShowInHelp = ShowInHelp
        });
    }
}
