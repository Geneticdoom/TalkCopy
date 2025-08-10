using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGuizmo;
using TalkCopy.Attributes;
using TalkCopy.Core.Handlers;

namespace TalkCopy.Update.Updatables;

[Active]
internal class KeyCheckUpdatable : UpdatableElement
{
    bool hasLetGo = true;
    bool defaultMode = true;

    public override void Update(IFramework framework)
    {
        if (Input.Disabled) return;

        PluginHandlers.Input.Update();

        bool combo1IsNoKey = PluginHandlers.Plugin.Config.ComboModifier == VirtualKey.NO_KEY;
        bool combo2IsNoKey = PluginHandlers.Plugin.Config.ComboModifier2 == VirtualKey.NO_KEY;
        bool comboKey = PluginHandlers.Plugin.Config.ComboKey == VirtualKey.NO_KEY;

        // this means everything is set to no key!
        if (combo1IsNoKey && combo2IsNoKey && comboKey) return;

        bool combo1Down = combo1IsNoKey | PluginHandlers.Input.IsDown(PluginHandlers.Plugin.Config.ComboModifier);
        bool combo2Down = combo2IsNoKey | PluginHandlers.Input.IsDown(PluginHandlers.Plugin.Config.ComboModifier2);
        bool comboKeyDown = comboKey | PluginHandlers.Input.IsDown(PluginHandlers.Plugin.Config.ComboKey);

        if (PluginHandlers.Plugin.Config.PreventPassthrough)
        {
            if (combo1Down && combo2Down)
            {
                UnsetKey(PluginHandlers.Plugin.Config.ComboModifier);
                UnsetKey(PluginHandlers.Plugin.Config.ComboModifier2);
                UnsetKey(PluginHandlers.Plugin.Config.ComboKey);
            }
        }

        bool toggleMode = PluginHandlers.Plugin.Config.ToggleMode;

        if (!combo1Down || !combo2Down || !comboKeyDown)
        {
            hasLetGo = true;
        }
        else
        {
            if (toggleMode && hasLetGo)
            {
                defaultMode = !defaultMode;
            }
            hasLetGo = false;
        }

        if (!toggleMode)
        {
            defaultMode = hasLetGo;
        }

        if (PluginHandlers.ClientState.IsPvPExcludingDen) defaultMode = true;

        TalkCopyPlugin.CurrentMode = defaultMode ? PluginMode.Default : PluginMode.TextCopy;
    }

    void UnsetKey(VirtualKey key)
    {
        if ((int)key <= 0 || (int)key >= 240)
            return;

        PluginHandlers.KeyState[key] = false;
    }

}
