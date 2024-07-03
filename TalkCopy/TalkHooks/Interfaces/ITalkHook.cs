using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Component.GUI;
using TalkCopy.Core.Hooking;

namespace TalkCopy.TalkHooks.Interfaces;

internal unsafe interface ITalkHook
{
    string AddonName { get; }
    string LastText { get; }
    void OnPreUpdate(BaseNode baseNode);
    void OnTalk(AddonEvent type, AddonArgs args);
    bool CanCopy();
    void ExtractText(AtkTextNode* textNode);
}
