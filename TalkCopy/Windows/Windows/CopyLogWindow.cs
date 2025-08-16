using Dalamud.Bindings.ImGui;
using System.Globalization;
using System.Numerics;
using TalkCopy.Attributes;
using TalkCopy.Copying;
using TalkCopy.Core.Handlers;
using TalkCopy.Windows.Attributes;

namespace TalkCopy.Windows.Windows;

[Active]
[MainWindow]
internal class CopyLogWindow : TalkWindow
{

    public CopyLogWindow() : base("Talk Copy Log")
    {
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new System.Numerics.Vector2(250, 250)
        };
    }

    public override void Draw()
    {
        if (ImGui.Checkbox("24 Hour Time?", ref PluginHandlers.Plugin.Config.hour24))
            PluginHandlers.Plugin.Config.Save();

        ImGui.BeginTable("##CopyLogTable", 5, ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg, ImGui.GetContentRegionAvail());

        for (int i = CopyHandler.CopyData.Count - 1; i >= 0; i--) { 
            CopyData data = CopyHandler.CopyData[i];
            ImGui.TableNextRow();
            if (data.Blocked) ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.ColorConvertFloat4ToU32(new Vector4(0.4f, 0, 0, 1)));
            ImGui.TableSetColumnIndex(0);
            if (PluginHandlers.Plugin.Config.hour24)
            {
                ImGui.Text(data.MessageTimestamp.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
            }
            else {
                ImGui.Text(data.MessageTimestamp.ToString("hh:mm:ss tt", CultureInfo.InvariantCulture));
            }
            ImGui.TableSetColumnIndex(1);
            ImGui.Text(data.Addon);
            ImGui.TableSetColumnIndex(2);
            ImGui.Text(data.Text);
            ImGui.TableSetColumnIndex(3);
            ImGui.Text(data.Blocked.ToString());
            ImGui.TableSetColumnIndex(4);
            if (ImGui.Button($"Copy Again##{i}"))
            {
                if (PluginHandlers.Plugin.Config.UseWebSocket)
                {
                    // Send via WebSocket
                    PluginHandlers.Plugin.WebSocketServer?.SendTextAsync(data.Text);
                }
                else
                {
                    // Use clipboard (original behavior)
                    ImGui.SetClipboardText(data.Text);
                }
            }
        }
        ImGui.EndTable();
    }
}
