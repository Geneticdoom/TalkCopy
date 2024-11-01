using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Collections.Generic;
using System.Linq;
using TalkCopy.Attributes;
using TalkCopy.Copying;
using TalkCopy.Core.Handlers;
using TalkCopy.TalkHooks.Base;
using static FFXIVClientStructs.FFXIV.Component.GUI.AtkComponentList;

namespace TalkCopy.TalkHooks.Hooks;

[Active]
internal unsafe class ListElementHook
{
    string[] allowed =
    [
        "RetainerList",
        "ContextMenu",
        "AddonContextMenuTitle",
        "SelectIconString",
        "SelectString",
    ];

    public ListElementHook()
    {
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostReceiveEvent, OnPostSetup);
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, OnPostSetup);
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, OnPostSetup);
    }

    ~ListElementHook()
    {
        PluginHandlers.AddonLifecycle.UnregisterListener(AddonEvent.PostReceiveEvent, OnPostSetup);
        PluginHandlers.AddonLifecycle.UnregisterListener(AddonEvent.PostRefresh, OnPostSetup);
        PluginHandlers.AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, OnPostSetup);
    }

    string lastAddon = string.Empty;

    // type 1006 is a ComponentList
    // type 1004 is a ComponentListElement or w/e

    void OnPostSetup(AddonEvent type, AddonArgs args)
    {
        string addonName = args.AddonName;
        if (!allowed.Contains(addonName)) return;

        lastAddon = addonName;

        AtkUnitBase* atkUnitBase = (AtkUnitBase*)args.Addon;
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
            SeString seString = SeString.Parse(listItem.Label);
            if (seString.Payloads.Count != 0)
            {
                strings.Add(seString.TextValue);
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
}
