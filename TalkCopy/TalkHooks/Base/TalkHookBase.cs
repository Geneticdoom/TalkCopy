using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using TalkCopy.Copying;
using TalkCopy.Core.Handlers;
using TalkCopy.Core.Hooking;
using TalkCopy.TalkHooks.Interfaces;

namespace TalkCopy.TalkHooks.Base;

internal unsafe abstract class TalkHookBase : ITalkHook
{
    Dictionary<ushort, string> lastTextDictionary = new Dictionary<ushort, string>();
    public string AddonName { get; init; } = "";

    public TalkHookBase(string addonName) => PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PreUpdate, AddonName = addonName, OnTalk);
    public void OnTalk(AddonEvent type, AddonArgs args)
    {
        AtkUnitBase* atkUnitBase = (AtkUnitBase*)args.Addon;
        if (atkUnitBase == null) return;
        ushort currentID = atkUnitBase->Id;

        if (PluginHandlers.ClientState.IsGPosing) return;
        try
        {
            OnPreUpdate(new BaseNode(atkUnitBase), currentID);
        }
        catch (Exception e)
        {
            PluginHandlers.PluginLog.Error($"Exception thrown in: {AddonName}. Exception: {e}");
        }
    }
    public abstract void OnPreUpdate(BaseNode baseNode, ushort ID);
    public abstract bool CanCopy();

    public void ExtractText(AtkTextNode* textNode, ushort ID)
    {
        if (textNode == null) return;
        if (!textNode->IsVisible()) return;

        string currentText = textNode->NodeText.ToString();

        if (currentText.IsNullOrEmpty()) return;
        if (lastTextDictionary.TryGetValue(ID, out string? value))
        {
            if (!value.IsNullOrEmpty() && value == currentText) return;
            lastTextDictionary[ID] = currentText;
        }
        else
        {
            lastTextDictionary.Add(ID, currentText);
        }

        CopyHandler.CopyTextToClipboard(AddonName, currentText, !CanCopy());
    }
}
