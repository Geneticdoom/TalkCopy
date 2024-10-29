using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using System;
using TalkCopy.Core.Handlers;

namespace TalkCopy.Windows;

internal class WindowHandler : IDisposable
{
    WindowSystem windowSystem;

    public WindowHandler(IDalamudPluginInterface pluginInterface)
    {
        windowSystem = new WindowSystem("Dialog Copy");
        pluginInterface.UiBuilder.Draw += windowSystem.Draw;
    }

    public void Dispose()
    {
        PluginHandlers.PluginInterface.UiBuilder.Draw -= windowSystem.Draw;
        windowSystem.RemoveAllWindows();
    }

    public void AddWindow(TalkWindow talkWindow)
    {
        windowSystem.AddWindow(talkWindow);
    }

    public void RemoveWindow(TalkWindow talkWindow)
    {
        windowSystem.RemoveWindow(talkWindow);
    }

    public void OpenWindow<T>() where T : TalkWindow
    {
        foreach(Window w in windowSystem.Windows)
        {
            if (w is not T) continue;
            w.IsOpen = true;
        }
    }

    public void CloseWindow<T>() where T : TalkWindow
    {
        foreach (Window w in windowSystem.Windows)
        {
            if (w is not T) continue;
            w.IsOpen = false;
        }
    }

    public void ToggleWindow<T>() where T : TalkWindow
    {
        foreach (Window w in windowSystem.Windows)
        {
            if (w is not T) continue;
            w.IsOpen ^= true;
        }
    }
}
