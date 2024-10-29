using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Utility;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System.Numerics;
using TalkCopy.Attributes;
using TalkCopy.Copying;
using TalkCopy.Core.Handlers;
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

        DrawResNode(unitBase->RootNode, true);

        for (int i = 0; i < unitBase->UldManager.NodeListCount; i++)
        {
            AtkResNode* baseNode = unitBase->UldManager.NodeList[i];
            if (baseNode == null) continue;

            DrawResNode(baseNode, true);
        }
    }

    void DrawResNode(AtkResNode* node, bool printSiblings = false)
    {
        if (node == null) return;

        if (node->Type == NodeType.Text)
        {
            DrawOutline(node);
        }

        AtkResNode* prevNode = node;
        while ((prevNode = prevNode->PrevSiblingNode) != null)
        {
            if (printSiblings) DrawResNode(prevNode);
        }

        AtkResNode* nextNode = node;
        while ((nextNode = nextNode->NextSiblingNode) != null)
        {
            if (printSiblings) DrawResNode(nextNode);
        }

        if ((int)node->Type < 1000) 
        { 
            DrawResNode(node->ChildNode, true); 
        }
        else
        {
            AtkComponentNode* compNode = (AtkComponentNode*)node;
            AtkUldManager componentInfo = compNode->Component->UldManager;
            DrawResNode(componentInfo.RootNode, true);
        }
    }

    private Vector2 GetNodePosition(AtkResNode* node)
    {
        var pos = new Vector2(node->X, node->Y);
        pos -= new Vector2(node->OriginX * (node->ScaleX - 1), node->OriginY * (node->ScaleY - 1));
        var par = node->ParentNode;
        while (par != null)
        {
            pos *= new Vector2(par->ScaleX, par->ScaleY);
            pos += new Vector2(par->X, par->Y);
            pos -= new Vector2(par->OriginX * (par->ScaleX - 1), par->OriginY * (par->ScaleY - 1));
            par = par->ParentNode;
        }

        return pos;
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

        Vector2 position = GetNodePosition(node);
        Vector2 scale = GetNodeScale(node);
        Vector2 size = new Vector2(node->Width, node->Height) * scale;

        bool nodeVisible = GetNodeVisible(node);
        if (!nodeVisible) return;

        position += ImGuiHelpers.MainViewport.Pos;

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

        ImGui.GetForegroundDrawList(ImGuiHelpers.MainViewport).AddRect(position, max, isHovered ? 0xFF00FF00 : 0xFF999999, 5, ImDrawFlags.RoundCornersAll, 2);
    }
}
