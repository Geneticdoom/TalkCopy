using FFXIVClientStructs.FFXIV.Component.GUI;
using TalkCopy.Attributes;
using TalkCopy.Core.Handlers;
using TalkCopy.Core.Hooking;
using TalkCopy.TalkHooks.Base;

namespace TalkCopy.TalkHooks.Hooks;

[Active]
internal unsafe class BattleTalkHook : TalkHookBase
{
    public BattleTalkHook() : base("_BattleTalk") { }

    public override void OnPreUpdate(BaseNode baseNode, ushort ID) => ExtractText(baseNode.GetNode<AtkTextNode>(6), ID);

    public override bool CanCopy() => PluginHandlers.Plugin.Config.CopyBattleTalk;
}
