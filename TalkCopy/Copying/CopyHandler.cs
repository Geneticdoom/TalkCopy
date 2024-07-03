using ImGuiNET;
using System;
using System.Collections.Generic;
using TalkCopy.Core.Handlers;

namespace TalkCopy.Copying;

internal static class CopyHandler
{
    public static List<CopyData> CopyData { get; private set; } = new List<CopyData>();

    public static void CopyTextToClipboard(string Addon, string Text, bool Block)
    {
        if (!PluginHandlers.Plugin.Config.CopyAnyText) return;
        PluginHandlers.PluginLog.Verbose("Addon: " + Addon + " wants to copy the text: " + Text);
        CopyData.Add(new CopyData(DateTime.Now, Addon, Text, Block));
        if (CopyData.Count > 100) CopyData.RemoveAt(0);
        ImGui.SetClipboardText(Text);
    }
}

internal struct CopyData(DateTime timeStamp, string addon, string text, bool blocked)
{
    public readonly DateTime MessageTimestamp = timeStamp;
    public readonly string Addon = addon;
    public readonly string Text = text;
    public readonly bool Blocked = blocked;
}
