using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Text.ReadOnly;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TalkCopy.Copying;
using TalkCopy.Core.Handlers;
using TalkCopy.Core.Hooking;
using TalkCopy.TalkHooks.Interfaces;

namespace TalkCopy.TalkHooks.Base;

internal unsafe abstract class TalkHookBase : ITalkHook
{
    Dictionary<ushort, string> lastTextDictionary = new Dictionary<ushort, string>();
    public string AddonName { get; init; } = "";

    ushort ID = 0;

    public TalkHookBase(string addonName) => PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PreUpdate, AddonName = addonName, OnTalk);
    public void OnTalk(AddonEvent type, AddonArgs args)
    {
        if (TalkCopyPlugin.CurrentMode != PluginMode.Default) return;

        AtkUnitBase* atkUnitBase = (AtkUnitBase*)args.Addon;
        if (atkUnitBase == null) return;

        ID = atkUnitBase->Id;
        if (PluginHandlers.ClientState.IsGPosing) return;

        try
        {
            OnPreUpdate(new BaseNode(atkUnitBase));
        }
        catch (Exception e)
        {
            PluginHandlers.PluginLog.Error($"Exception thrown in: {AddonName}. Exception: {e}");
        }
    }
    public abstract void OnPreUpdate(BaseNode baseNode);
    public abstract bool CanCopy();

    public string? ExtractText(AtkTextNode* textNode)
    {
        if (textNode == null) return null;
        if (!textNode->IsVisible()) return null;

        return GetDecodedText(textNode);
    }

    public static string GetDecodedText(AtkTextNode* textNode)
    {
        string currentText = string.Empty;

        try
        {
            currentText = new ReadOnlySeStringSpan(textNode->NodeText.StringPtr).ToString();
        }
        catch
        {
            currentText = textNode->NodeText.ToString();
        }

        if (PluginHandlers.Plugin.Config.ParseOutBrackets) 
        {
            currentText = Regex.Replace(currentText, "<.*?>", "");
        }

        return currentText ?? string.Empty;
    }

    public void CopyText(string? text)
    {
        if (text.IsNullOrWhitespace()) return;

        if (text.IsNullOrEmpty()) return;
        if (lastTextDictionary.TryGetValue(ID, out string? value))
        {
            if (!value.IsNullOrEmpty() && value == text) return;
            lastTextDictionary[ID] = text;
        }
        else
        {
            lastTextDictionary.Add(ID, text);
        }

        CopyHandler.CopyTextToClipboard(AddonName, text, !CanCopy());
    }
}
