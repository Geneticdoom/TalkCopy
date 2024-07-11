using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Component.GUI;
using TalkCopy.Core.Hooking;

namespace TalkCopy.TalkHooks.Interfaces;

internal unsafe interface ITalkHook
{
    string AddonName { get; }
    void OnPreUpdate(BaseNode baseNode, ushort ID);
    void OnTalk(AddonEvent type, AddonArgs args);
    bool CanCopy();
    void ExtractText(AtkTextNode* textNode, ushort ID);
}
