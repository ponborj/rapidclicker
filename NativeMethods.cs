using System.Runtime.InteropServices;

namespace RapidClicker;

internal static class NativeMethods
{
    internal const int WmHotKey = 0x0312;
    internal const uint ModNone = 0x0000;
    internal const uint ModNoRepeat = 0x4000;
    internal const uint KeyF6 = 0x75;
    internal const uint KeyF7 = 0x76;
    private const uint InputMouse = 0;
    private const uint MouseEventFLeftDown = 0x0002;
    private const uint MouseEventFLeftUp = 0x0004;

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool UnregisterHotKey(nint hWnd, int id);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint cInputs, INPUT[] pInputs, int cbSize);

    internal static void LeftClick()
    {
        var inputs = new[]
        {
            new INPUT { Type = InputMouse, Data = new InputUnion { Mouse = new MOUSEINPUT { DwFlags = MouseEventFLeftDown } } },
            new INPUT { Type = InputMouse, Data = new InputUnion { Mouse = new MOUSEINPUT { DwFlags = MouseEventFLeftUp } } }
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        internal uint Type;
        internal InputUnion Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] internal MOUSEINPUT Mouse;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        internal int Dx;
        internal int Dy;
        internal uint MouseData;
        internal uint DwFlags;
        internal uint Time;
        internal nuint DwExtraInfo;
    }
}
