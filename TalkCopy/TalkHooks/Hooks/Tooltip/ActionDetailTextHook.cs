using FFXIVClientStructs.FFXIV.Component.GUI;
using TalkCopy.Attributes;
using TalkCopy.Core.Handlers;
using TalkCopy.Core.Hooking;

namespace TalkCopy.TalkHooks.Hooks.Tooltip;

[Active]
internal unsafe class ActionDetailTextHook : BaseTooltipTextHook
{
    public ActionDetailTextHook() : base("ActionDetail") { }

    public override void OnPreUpdate(BaseNode baseNode)
    {
        string? actionTitle = ExtractText(baseNode.GetNode<AtkTextNode>(5));
        string? talentText = ExtractText(baseNode.GetNode<AtkTextNode>(6));

        string? rangeTitle = ExtractText(baseNode.GetNode<AtkTextNode>(8));
        string? rangeValue = ExtractText(baseNode.GetNode<AtkTextNode>(9)); 
        string? radiusTitle = ExtractText(baseNode.GetNode<AtkTextNode>(11));
        string? radiusvalue = ExtractText(baseNode.GetNode<AtkTextNode>(12));

        ComponentNode activationType = baseNode.GetComponentNode(14);
        if (activationType == null) { PluginHandlers.PluginLog.Error("activationType node null!"); return; }

        string? activationTitle = ExtractText(activationType.GetNode<AtkTextNode>(2));
        string? activationvalue = ExtractText(activationType.GetNode<AtkTextNode>(3));

        ComponentNode reactivationType = baseNode.GetComponentNode(15);
        if (reactivationType == null) { PluginHandlers.PluginLog.Error("activationType node null!"); return; }

        string? reactivationTitle = ExtractText(reactivationType.GetNode<AtkTextNode>(2));
        string? reactivationValue = ExtractText(reactivationType.GetNode<AtkTextNode>(3));

        string? mainText = ExtractText(baseNode.GetNode<AtkTextNode>(19));

        string? learnedAtTitle = ExtractText(baseNode.GetNode<AtkTextNode>(22));
        string? learnedAtValue = ExtractText(baseNode.GetNode<AtkTextNode>(26));

        string? affinityTitle = ExtractText(baseNode.GetNode<AtkTextNode>(28));
        string? affinityValue = ExtractText(baseNode.GetNode<AtkTextNode>(29));

        string outcome =
            $"{actionTitle}\n" +
            $"{talentText}   {rangeTitle} {rangeValue}   {radiusTitle} {radiusvalue}\n" +
            $"{activationTitle}     {reactivationTitle}\n" +
            $"{activationvalue}     {reactivationValue}\n" +
            $"---------------------------\n" +
            $"{mainText}\n" +
            $"\n" +
            $"{learnedAtTitle} {learnedAtValue}\n" +
            $"{affinityTitle} {affinityValue}";

        CopyText(outcome);
    }
}
