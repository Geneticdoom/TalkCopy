using FFXIVClientStructs.FFXIV.Component.GUI;
using TalkCopy.Attributes;
using TalkCopy.Core.Handlers;
using TalkCopy.Core.Hooking;
using TalkCopy.TalkHooks.Base;

namespace TalkCopy.TalkHooks.Hooks;

[Active]
internal unsafe class AreaTextHook : TalkHookBase
{
    public AreaTextHook() : base("_AreaText") { }

    public override void OnPreUpdate(BaseNode baseNode, ushort ID)
    {
        ComponentNode componentNode = baseNode.GetComponentNode(2);
        if (componentNode == null) { PluginHandlers.PluginLog.Error("Comonent node null!"); return; }
        ExtractText(componentNode.GetNode<AtkTextNode>(2), ID);
    }

    public override bool CanCopy() => PluginHandlers.Plugin.Config.CopyAreaText;
}
