using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Utility;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System.Numerics;
using TalkCopy.Attributes;
using TalkCopy.Copying;
using TalkCopy.Core.Handlers;
using TalkCopy.Core.Hooking;
using TalkCopy.TalkHooks.Base;

namespace TalkCopy.Windows.Windows;

[Active]
internal unsafe class OverlayWindow : TalkWindow
{
    // Known from: https://github.com/goatcorp/Dalamud/blob/a8244a9114949a1b5595d4aaffab0f4f0c34e60a/Dalamud/Interface/Internal/UiDebug.cs
    const int UnitListCount = 18;

    string wantsToCopy = string.Empty;
    string lastWantsToCopy = string.Empty;

    public OverlayWindow() : base("OverlayWindow")
    {
        SizeConstraints = new WindowSizeConstraints()
        {
            MaximumSize = new Vector2(500, 90),
            MinimumSize = new Vector2(500, 90),
        };

        Flags |= ImGuiWindowFlags.NoMove;
        Flags |= ImGuiWindowFlags.NoBackground;
        Flags |= ImGuiWindowFlags.NoInputs;
        Flags |= ImGuiWindowFlags.NoNavFocus;
        Flags |= ImGuiWindowFlags.NoResize;
        Flags |= ImGuiWindowFlags.NoScrollbar;
        Flags |= ImGuiWindowFlags.NoTitleBar;
        Flags |= ImGuiWindowFlags.NoDecoration;
        Flags |= ImGuiWindowFlags.NoFocusOnAppearing;

        DisableWindowSounds = true;

        ForceMainWindow = true;

        IsOpen = true;
    }

    public override void Update()
    {
        if (IsOpen) return;

        IsOpen = true;
    }

    public override void Draw()
    {
        if (TalkCopyPlugin.CurrentMode != PluginMode.TextCopy) return;

        wantsToCopy = string.Empty;

        DrawStage();
        DrawWarning();
        DrawWantsToCopy();

        HandleAutoCopy();
    }

    void HandleAutoCopy()
    {
        if (lastWantsToCopy == wantsToCopy) return;
        lastWantsToCopy = wantsToCopy;

        if (wantsToCopy.IsNullOrEmpty()) return;

        CopyHandler.CopyTextToClipboard("Text Copy Window", wantsToCopy, false);
    }

    void DrawWantsToCopy()
    {
        if (!PluginHandlers.Plugin.Config.ShowTooltip) return;
        if (wantsToCopy.IsNullOrWhitespace()) return;

        ImGui.SetWindowFontScale(1);

        float mouseOffset = 15;

        Vector2 textSize = ImGui.CalcTextSize(wantsToCopy);
        Vector2 halfTextSize = textSize * 0.5f;
        Vector2 mousePos = ImGui.GetMousePos();
        Vector2 startPos = mousePos - new Vector2(-mouseOffset, textSize.Y + mouseOffset);
        Vector2 rectOffset = new Vector2(10, 10);

        ImDrawListPtr foregroundPointer = ImGui.GetForegroundDrawList(ImGuiHelpers.MainViewport);

        foregroundPointer.AddRectFilled(startPos - rectOffset, startPos + textSize + rectOffset, 0xFF000000);
        foregroundPointer.AddText(ImGui.GetFont(), ImGui.GetFontSize(), startPos, 0xFFFFFFFF, wantsToCopy);
    }

    void DrawStage()
    {
        AtkStage* stage = AtkStage.Instance();

        AtkUnitList* unitManagers = &stage->RaptureAtkUnitManager->AtkUnitManager.DepthLayerOneList;

        for (int i = 0; i < UnitListCount; i++)
        {
            DrawBaseNode(&unitManagers[i]);
        }
    }

    void DrawWarning()
    {
        ImGui.SetWindowFontScale(1.3f);

        string LABEL = "                                        [Dialog Copy]\n" +
            "                       [TEXT COPY MODE ACTIVE]\n\n" +
            "This mode shows every visible text element.\n" +
            "Upon hovering over one the text gets copied.\n\n" +
            "Keybinds:\n" +
            $"[{PluginHandlers.Plugin.Config.ComboModifier.GetFancyName()}] [{PluginHandlers.Plugin.Config.ComboModifier2.GetFancyName()}] [{PluginHandlers.Plugin.Config.ComboKey.GetFancyName()}]\n" +
            $"(Can be changed via: '/dialogcopy settings')";

        Vector2 rectOffset = new Vector2(10, 10);
        Vector2 textSize = ImGui.CalcTextSize(LABEL, false);
        Vector2 halfTextSize = textSize * 0.5f;

        Vector2 centre = ImGuiHelpers.MainViewport.GetCenter();
        centre.Y *= 0.1f;
        centre.X -= halfTextSize.X;

        ImDrawListPtr foregroundPointer = ImGui.GetForegroundDrawList(ImGuiHelpers.MainViewport);

        if (PluginHandlers.Plugin.Config.ShowWarningText)
        {
            foregroundPointer.AddRectFilled(centre - rectOffset, centre + textSize + rectOffset, 0xFF000000);
            foregroundPointer.AddText(ImGui.GetFont(), ImGui.GetFontSize(), centre, 0xFF0000FF, LABEL);
        }

        if (PluginHandlers.Plugin.Config.ShowWarningOutline)
        {
            foregroundPointer.AddRect(Vector2.Zero, ImGuiHelpers.MainViewport.WorkSize, 0xFF0000FF, 0, ImDrawFlags.None, 15);
        }
    }

    void DrawBaseNode(AtkUnitList* unitList)
    {
        for (int i = 0; i < unitList->Count; i++)
        {
            AtkUnitBase* unitBase = unitList->Entries[i];

            string? nameString = unitBase->NameString;
            if (nameString.IsNullOrWhitespace())
            {
                continue;
            }

            DrawAtkUnitBase(unitBase);
        }
    }

    void DrawAtkUnitBase(AtkUnitBase* unitBase)
    {
        if (unitBase == null) return;
        if (!unitBase->IsVisible) return;
        if (unitBase->RootNode == null) return;
        if (unitBase->UldManager.NodeListCount <= 0) return;

        DrawResNode(unitBase->RootNode);

        for (int i = 0; i < unitBase->UldManager.NodeListCount; i++)
        {
            AtkResNode* baseNode = unitBase->UldManager.NodeList[i];
            if (baseNode == null) continue;

            DrawResNode(baseNode);
        }
    }

    void DrawResNode(AtkResNode* node)
    {
        PluginHandlers.Utils.FindNodeOfType((int)NodeType.Text, (addon) => DrawOutline((AtkResNode*)addon),false, node, true);
    }

    Vector2 GetNodeScale(AtkResNode* node)
    {
        if (node == null) return new Vector2(1, 1);

        Vector2 scale = new Vector2(node->ScaleX, node->ScaleY);
        while (node->ParentNode != null)
        {
            node = node->ParentNode;
            scale *= new Vector2(node->ScaleX, node->ScaleY);
        }

        return scale;
    }

    bool GetNodeVisible(AtkResNode* node)
    {
        if (node == null) return false;
        while (node != null)
        {
            if (!node->NodeFlags.HasFlag(NodeFlags.Visible)) return false;
            node = node->ParentNode;
        }

        return true;
    }

    void DrawOutline(AtkResNode* node)
    {
        AtkTextNode* textNode = node->GetAsAtkTextNode();
        if (textNode->NodeText.Length == 0) return;

        bool nodeVisible = GetNodeVisible(node);
        if (!nodeVisible) return;

        ushort textWidth, textHeight;
        textNode->GetTextDrawSize(&textWidth, &textHeight);

        float oversizedBase = 5;
        Vector2 oversizedScale = new Vector2(oversizedBase, oversizedBase);

        Vector2 scale = GetNodeScale(node);
        Vector2 rawSize = new Vector2(textWidth, textHeight);
        Vector2 size = rawSize * scale + oversizedScale;
        Vector2 position = GetTextNodeScreenPosition(textNode, rawSize) - (oversizedScale * 0.5f);

        Vector2 mousepos = ImGui.GetMousePos();

        Vector2 min = position;
        Vector2 max = position + size;

        bool isHovered = mousepos.X > min.X &&
                         mousepos.X < max.X &&
                         mousepos.Y > min.Y &&
                         mousepos.Y < max.Y;

        if (Input.Disabled)
        {
            isHovered = false;
        }

        if (isHovered)
        {
            wantsToCopy = TalkHookBase.GetDecodedText(textNode);
        }

        ImGui.GetForegroundDrawList(ImGuiHelpers.MainViewport).AddRect(position, max, isHovered ? 0xFF00FF00 : 0xFFFFFF00, 1, ImDrawFlags.RoundCornersAll, 1);
    }

    Vector2 GetTextNodeScreenPosition(AtkTextNode* textNode, Vector2 rawTextSize)
    {
        Vector2 finalPos;

        Vector2 nodeScreenPos = new Vector2(textNode->ScreenX, textNode->ScreenY);
        Vector2 nodeScale = GetNodeScale(&textNode->AtkResNode);
        Vector2 nodeSize = new Vector2(textNode->Width * nodeScale.X, textNode->Height * nodeScale.Y);
        Vector2 nodeSizeHalf = nodeSize * 0.5f;
        Vector2 nodeCentrePos = nodeScreenPos + nodeSizeHalf;
        Vector2 scaledTextSize = rawTextSize * nodeScale;

        finalPos = nodeCentrePos;

        AlignmentType aType = textNode->AlignmentType;

        int aTypeInt = (int)aType;
        if (aTypeInt < 0 || aTypeInt > (int)AlignmentType.BottomRight) return finalPos;

        switch (aType)
        {
            case AlignmentType.TopLeft:     finalPos -= nodeSizeHalf;                                                                                                           break;
            case AlignmentType.Top:         finalPos -= new Vector2(scaledTextSize.X * 0.5f, 0)                         - new Vector2(0, -nodeSizeHalf.Y);                      break;
            case AlignmentType.TopRight:    finalPos -= new Vector2(scaledTextSize.X, 0)                                - new Vector2(nodeSizeHalf.X, -nodeSizeHalf.Y);         break;
            case AlignmentType.Left:        finalPos -= new Vector2(0, scaledTextSize.Y * 0.5f)                         - new Vector2(-nodeSizeHalf.X, 0);                      break;
            case AlignmentType.Center:      finalPos -= new Vector2(scaledTextSize.X * 0.5f, scaledTextSize.Y * 0.5f);                                                          break;
            case AlignmentType.Right:       finalPos -= new Vector2(scaledTextSize.X, scaledTextSize.Y * 0.5f)          - new Vector2(nodeSizeHalf.X, 0);                       break;
            case AlignmentType.BottomLeft:  finalPos -= new Vector2(0, scaledTextSize.Y)                                - new Vector2(-nodeSizeHalf.X, nodeSizeHalf.Y);         break;
            case AlignmentType.Bottom:      finalPos -= new Vector2(scaledTextSize.X * 0.5f, scaledTextSize.Y)          - new Vector2(0, nodeSizeHalf.Y);                       break;
            case AlignmentType.BottomRight: finalPos -= new Vector2(scaledTextSize.X, scaledTextSize.Y)                 - nodeSizeHalf;                                         break;
        }

        return finalPos;
    }
}
