using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Keys;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using Dalamud.Bindings.ImGui;

namespace TalkCopy;

// Most of this is stolen from Dalamud.FindAnything who stole it from QoLBar
public unsafe class Input
{
    static bool IsGameFocused => !Framework.Instance()->WindowInactive;
    static bool IsGameTextInputActive => RaptureAtkModule.Instance()->AtkModule.IsTextInputActive();

    public static bool Disabled => IsGameTextInputActive || !IsGameFocused || ImGui.GetIO().WantCaptureKeyboard;

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetKeyboardState(byte[] lpKeyState);
    static readonly byte[] keyboardState = new byte[256];

    public void Update()
    {
        GetKeyboardState(keyboardState);
    }

    public bool IsDown(VirtualKey key) => (keyboardState[(int)key] & 0x80) != 0;
}
