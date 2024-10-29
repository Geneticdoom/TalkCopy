using TalkCopy.Core.Handlers;
using TalkCopy.TalkHooks.Base;

namespace TalkCopy.TalkHooks.Hooks.Tooltip;

internal abstract class BaseTooltipTextHook : TalkHookBase
{
    protected BaseTooltipTextHook(string addonName) : base(addonName) { }

    public override bool CanCopy() => PluginHandlers.Plugin.Config.CopyTooltips;
}
