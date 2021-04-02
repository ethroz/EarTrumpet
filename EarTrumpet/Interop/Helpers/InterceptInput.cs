using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Keys = System.Windows.Forms.Keys;

namespace EarTrumpet.Interop.Helpers
{
    class InterceptInput
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_MOUSEHWHEEL = 0x020E;
        private const int WM_MBUTTONDOWN = 0x0207;
        public const int VK_VOLUME_MUTE = 0xAD;
        public const int VK_VOLUME_DOWN = 0xAE;
        public const int VK_VOLUME_UP = 0xAF;
        private static readonly LowLevelHookProc _kbproc = KbHookCallback;
        private static readonly LowLevelHookProc _mproc = MHookCallback;

        public static event Action<int> MouseWheelEvent;
        public static event Func<int, int, bool> IsMouseInsideIcon;
        private static bool[] modifiers = new bool[8];
        private static IntPtr _kbHookID = IntPtr.Zero;
        private static IntPtr _mHookID = IntPtr.Zero;

        public static void SetKeyboardHook()
        {
            _kbHookID = SetKeyboardHook(_kbproc);
        }

        public static void SetMouseHook()
        {
            _mHookID = SetMouseHook(_mproc);
        }

        public static void UnHookKeyboard()
        {
            UnhookWindowsHookEx(_kbHookID);
        }

        public static void UnHookMouse()
        {
            UnhookWindowsHookEx(_mHookID);
        }

        private static IntPtr SetKeyboardHook(LowLevelHookProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr SetMouseHook(LowLevelHookProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr KbHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                Keys k = (Keys)Marshal.ReadInt32(lParam);
                if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
                {
                    switch (k)
                    {
                        case Keys.VolumeDown:
                            App.ChangeVolume(-1);
                            return (IntPtr)(-1);
                        case Keys.VolumeUp:
                            App.ChangeVolume(1);
                            return (IntPtr)(-1);
                        case Keys.VolumeMute:
                            App.MuteVolume();
                            return (IntPtr)(-1);
                        case Keys.LControlKey:
                            modifiers[0] = true;
                            break;
                        case Keys.RControlKey:
                            modifiers[1] = true;
                            break;
                        case Keys.LShiftKey:
                            modifiers[2] = true;
                            break;
                        case Keys.RShiftKey:
                            modifiers[3] = true;
                            break;
                        case Keys.LMenu:
                            modifiers[4] = true;
                            break;
                        case Keys.RMenu:
                            modifiers[5] = true;
                            break;
                        case Keys.LWin:
                            modifiers[6] = true;
                            break;
                        case Keys.RWin:
                            modifiers[7] = true;
                            break;
                    }
                }
                else if (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP)
                {
                    switch (k)
                    {
                        case Keys.LControlKey:
                            modifiers[0] = false;
                            break;
                        case Keys.RControlKey:
                            modifiers[1] = false;
                            break;
                        case Keys.LShiftKey:
                            modifiers[2] = false;
                            break;
                        case Keys.RShiftKey:
                            modifiers[3] = false;
                            break;
                        case Keys.LMenu:
                            modifiers[4] = false;
                            break;
                        case Keys.RMenu:
                            modifiers[5] = false;
                            break;
                        case Keys.LWin:
                            modifiers[6] = false;
                            break;
                        case Keys.RWin:
                            modifiers[7] = false;
                            break;
                    }
                }
                App.MasterModifier = App.AppModifier = true;
                for (int i = 0; i < 8; i++)
                {
                    if (App.Settings.VolumeShiftHotkey.indices[i])
                        App.MasterModifier &= modifiers[i];
                    else
                        App.MasterModifier &= !modifiers[i];

                    if (App.Settings.AppVolumeShiftHotkey.indices[i])
                        App.AppModifier &= modifiers[i];
                    else
                        App.AppModifier &= !modifiers[i];
                }
            }
            return CallNextHookEx(_kbHookID, nCode, wParam, lParam);
        }

        private static IntPtr MHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)WM_MOUSEWHEEL)
                {
                    MSLLHOOKSTRUCT m = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                    App.DisableMaster = IsMouseInsideIcon(m.pt.x, m.pt.y);
                    MouseWheelEvent.Invoke((m.mouseData << 8 >> 24) / 120);
                    if (App.MasterModifier || App.AppModifier)
                        return (IntPtr)(-1);
                }
            }
            return CallNextHookEx(_mHookID, nCode, wParam, lParam);
        }

        private static string AddZeros(string s)
        {
            string output = "";
            for (int i = 0; i < 32 - s.Length; i++)
                output += "0";
            output += s;
            return output;
        }

        private delegate IntPtr LowLevelHookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelHookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
