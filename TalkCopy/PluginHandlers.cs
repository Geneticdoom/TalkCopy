using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace TalkCopy.Core.Handlers;

internal class PluginHandlers
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; set; } = null!;
    internal static TalkCopyPlugin Plugin { get; set; } = null!;
    [PluginService] internal static IGameGui GameGui { get; private set; } = null!;
    [PluginService] internal static IAddonLifecycle AddonLifecycle { get; set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;

    internal static void Start(ref IDalamudPluginInterface plugin, TalkCopyPlugin selfPlugin)
    {
        plugin.Create<PluginHandlers>();
        Plugin = selfPlugin;
    }
}
