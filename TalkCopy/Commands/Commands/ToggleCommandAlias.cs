using TalkCopy.Attributes;

namespace TalkCopy.Commands.Commands;

[Active]
internal class ToggleCommandAlias : ToggleCommand
{
    public override string Command { get; } = "/dialogcopy";
}
