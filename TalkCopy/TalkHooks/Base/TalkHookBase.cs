using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using TalkCopy.Copying;
using TalkCopy.Core.Handlers;
using TalkCopy.Core.Hooking;
using TalkCopy.TalkHooks.Interfaces;

namespace TalkCopy.TalkHooks.Base;

internal unsafe abstract class TalkHookBase : ITalkHook
{
    public string LastText { get; private set; } = "";
    public string AddonName { get; init; } = "";

    public TalkHookBase(string addonName) => PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PreUpdate, AddonName = addonName, OnTalk);
    public void OnTalk(AddonEvent type, AddonArgs args)
    {
        try
        {
            OnPreUpdate(new BaseNode((AtkUnitBase*)args.Addon));
        }
        catch (Exception e)
        {
            PluginHandlers.PluginLog.Error($"Exception thrown in: {AddonName}. Exception: {e}");
        }
    }
    public abstract void OnPreUpdate(BaseNode baseNode);
    public abstract bool CanCopy();

    public void ExtractText(AtkTextNode* textNode)
    {
        if (textNode == null) return;
        if (!textNode->IsVisible()) return;

        string currentText = textNode->NodeText.ToString();

        if (currentText == null || currentText == string.Empty) return;
        if (LastText == currentText) return;

        LastText = currentText;
        CopyHandler.CopyTextToClipboard(AddonName, LastText, !CanCopy());
    }
}
