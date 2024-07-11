using FFXIVClientStructs.FFXIV.Component.GUI;
using TalkCopy.Attributes;
using TalkCopy.Core.Handlers;
using TalkCopy.Core.Hooking;
using TalkCopy.TalkHooks.Base;

namespace TalkCopy.TalkHooks.Hooks;

[Active]
internal unsafe class TalkHook : TalkHookBase
{
    public TalkHook() : base("Talk") { }

    public override void OnPreUpdate(BaseNode baseNode, ushort ID) => ExtractText(baseNode.GetNode<AtkTextNode>(3), ID);

    public override bool CanCopy() => PluginHandlers.Plugin.Config.CopyDialogBoxText;
}
