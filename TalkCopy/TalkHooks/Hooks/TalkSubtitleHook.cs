using FFXIVClientStructs.FFXIV.Component.GUI;
using TalkCopy.Attributes;
using TalkCopy.Core.Handlers;
using TalkCopy.Core.Hooking;
using TalkCopy.TalkHooks.Base;

namespace TalkCopy.TalkHooks.Hooks;

[Active]
internal unsafe class TalkSubtitleHook : TalkHookBase
{
    public TalkSubtitleHook() : base("TalkSubtitle") { }

    public override void OnPreUpdate(BaseNode baseNode, ushort ID) => ExtractText(baseNode.GetNode<AtkTextNode>(2), ID);

    public override bool CanCopy() => PluginHandlers.Plugin.Config.CopySubtitles;
}
