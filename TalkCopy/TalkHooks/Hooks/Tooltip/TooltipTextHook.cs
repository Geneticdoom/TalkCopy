using FFXIVClientStructs.FFXIV.Component.GUI;
using TalkCopy.Attributes;
using TalkCopy.Core.Hooking;

namespace TalkCopy.TalkHooks.Hooks.Tooltip;

[Active]
internal unsafe class TooltipTextHook : BaseTooltipTextHook
{
    public TooltipTextHook() : base("Tooltip") { }

    public override void OnPreUpdate(BaseNode baseNode) => CopyText(ExtractText(baseNode.GetNode<AtkTextNode>(2)));
}
