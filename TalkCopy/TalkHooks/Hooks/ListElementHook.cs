using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Text.ReadOnly;
using TalkCopy.Core.Handlers;
using TalkCopy.Core.Hooking;
using TalkCopy.TalkHooks.Base;
using TalkCopy.Attributes;
using static FFXIVClientStructs.FFXIV.Component.GUI.AtkComponentList;
using System.Collections.Generic;

namespace TalkCopy.TalkHooks.Hooks;

[Active]
internal unsafe class ListElementHook : TalkHookBase
{
    string[] allowed =
    [
        "RetainerList",
        "ContextMenu",
        "AddonContextMenuTitle",
        "SelectIconString",
        "SelectString",
    ];

    public ListElementHook() : base("ListElement")
    {
        foreach (string addon in allowed)
        {
            PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostReceiveEvent, addon, OnPostSetup);
            PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, addon, OnPostSetup);
            PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, addon, OnPostSetup);
        }
    }

    ~ListElementHook()
    {
        PluginHandlers.AddonLifecycle.UnregisterListener(OnPostSetup);
    }

    string lastAddon = string.Empty;

    void OnPostSetup(AddonEvent type, AddonArgs args)
    {
        lastAddon = args.AddonName;

        AtkUnitBase* atkUnitBase = (AtkUnitBase*)args.Addon.Address;
        if (atkUnitBase == null) return;

        PluginHandlers.Utils.FindNodeOfType(1000 + (int)ComponentType.List, (addon) => OnFindList((AtkResNode*)addon), false, atkUnitBase->RootNode, true);
    }

    List<string> strings = new List<string>();
    string lastString = string.Empty;

    void OnFindList(AtkResNode* addon)
    {
        if (addon == null) return;

        AtkComponentList* list = addon->GetAsAtkComponentList();
        if (list == null) return;

        strings.Clear();

        int listLength = list->ListLength;
        for (int i = 0; i < listLength; i++)
        {
            ListItem listItem = list->ItemRendererList[i];
            var seString = new ReadOnlySeStringSpan((byte*)listItem.Label).ExtractText();
            if (!string.IsNullOrWhiteSpace(seString))
            {
                strings.Add(seString);
            }
            else
            {
                strings.Add(TalkHookBase.GetDecodedText(listItem.AtkComponentListItemRenderer->ButtonTextNode));
            }
        }

        string newString = string.Empty;

        foreach(string seString in strings)
        {
            if (seString.IsNullOrWhitespace()) continue;
            newString += seString + "\n";
        }

        if (newString != lastString)
        {
            lastString = newString;
            if (lastString.IsNullOrWhitespace()) return;

            CopyHandler.CopyTextToClipboard(lastAddon, lastString, !PluginHandlers.Plugin.Config.TryCopyLists);
        }
    }

    public override void OnPreUpdate(BaseNode baseNode)
    {
        // This method is required by the base class but not used for list elements
        // List elements are handled by the OnPostSetup method instead
    }

    public override bool CanCopy()
    {
        return PluginHandlers.Plugin.Config.TryCopyLists;
    }
}
