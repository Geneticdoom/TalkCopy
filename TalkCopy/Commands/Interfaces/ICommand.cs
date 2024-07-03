namespace TalkCopy.Commands.Interfaces;

internal interface ICommand
{
    string Command { get; }
    string HelpMessage { get; }
    bool ShowInHelp { get;}

    void OnCommand(string command, string args);
}

