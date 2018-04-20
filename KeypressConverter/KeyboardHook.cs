using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using static KeypressConverter.Win32;

namespace KeypressConverter
{
    // Code adapted from: https://blogs.msdn.microsoft.com/toub/2006/05/03/low-level-keyboard-hook-in-c/
    //               and: https://www.codeproject.com/Articles/14485/Low-level-Windows-API-hooks-from-C-to-stop-unwante
    //               and: https://stackoverflow.com/questions/18647053/sendinput-not-equal-to-pressing-key-manually-on-keyboard-in-c
    // Constants found here: https://java-native-access.github.io/jna/4.2.0/constant-values.html#com.sun.jna.platform.win32.WinUser.INPUT.INPUT_KEYBOARD

    public class KeyboardHook
    {
        private static LowLevelKeyboardProc KeyboardHookCallback = HookCallback;
        private static IntPtr hHook = IntPtr.Zero;

        /// <summary>
        /// Installs low level keyboard hook procedure into hook chain
        /// </summary>
        internal static void SetKeyboardHook()
        {
            IntPtr handle = GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);
            hHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookCallback, handle, 0);
        }

        /// <summary>
        /// Removes hook procedure installed in hook chain
        /// </summary>
        internal static void UnsetKeyboardHook()
        {
            UnhookWindowsHookEx(hHook);
        }

        /// <summary>
        /// Callback for low level keyboard event
        /// </summary>
        /// <param name="nCode">Tells the hook procedure how to process the message. Only process if nCode >= 0 and nCode == HC_ACTION</param>
        /// <param name="wParam">Type of keyboard message. Can be WM_KEYDOWN, WM_KEYUP, WM_SYSKEYDOWN, or WM_SYSKEYUP. Only interested in WM_KEYDOWN and WM_KEYUP</param>
        /// <param name="lParam">Pointer to a KBDLLHOOKSTRUCT structure</param>
        /// <returns></returns>
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && nCode == HC_ACTION && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_KEYUP))
            {
                KBDLLHOOKSTRUCT hookStruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

                // If Scan Code is 0, key is completely virtual, i.e. it did not come from actual keyboard. This is what we're interested in processing.
                if (hookStruct.scanCode == 0)
                {
                    foreach (var key in Config.ConvertedKeys) // Check if key pressed is a selected converted key
                    {
                        if ((Keys)hookStruct.vkCode == key.Key)
                        {
                            KeyAction(hookStruct, wParam, key.Delay);
                            return (IntPtr)1; // Suppress original key press
                        }
                    }
                }
            }

            return CallNextHookEx(hHook, nCode, wParam, lParam); // Key press didn't need to be modified, pass on
        }

        /// <summary>
        /// MapVirtualKey does not set extended flag correctly, so determine extended key from this
        /// </summary>
        /// <param name="vKey">Key to check</param>
        /// <returns>True if key is extended key, False if not extended key</returns>
        private static bool isExtendedKey(Keys vKey)
        {
            // Numpad Enter is also an extended key, but doesn't have a virtual key
            switch (vKey)
            {
                case Keys.Insert:
                case Keys.Delete:
                case Keys.Home:
                case Keys.End:
                case Keys.Prior: // Page Up
                case Keys.Next:  // Page Down

                case Keys.Left:
                case Keys.Up:
                case Keys.Right:
                case Keys.Down:

                case Keys.Multiply:
                case Keys.Add:
                case Keys.Divide:

                case Keys.RControlKey:
                case Keys.RMenu:

                case Keys.NumLock:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Create new key event that uses scan code and delay for KeyUp
        /// </summary>
        /// <param name="hookStruct">Key event</param>
        /// <param name="wParam">Key message type</param>
        /// <param name="delay">Delay in milliseconds for KeyUp event</param>
        private static void KeyAction(KBDLLHOOKSTRUCT hookStruct, IntPtr wParam, int delay)
        {
            INPUT[] inputs = new INPUT[1];
            INPUT input = new INPUT();

            uint scanCode = MapVirtualKeyEx(hookStruct.vkCode, (uint)MapType.MAPVK_VK_TO_VSC_EX, IntPtr.Zero);

            // Set up the INPUT structure
            input.Type = 1; // INPUT_KEYBOARD;
            input.Data.Keyboard.Time = hookStruct.time;
            input.Data.Keyboard.Vk = (ushort)hookStruct.vkCode;
            input.Data.Keyboard.ExtraInfo = hookStruct.dwExtraInfo;

            // This let's you do a hardware scan instead of a virtual key press
            input.Data.Keyboard.Flags = 8; // KEYEVENTF_SCANCODE

            if (isExtendedKey((Keys)hookStruct.vkCode))
            {
                input.Data.Keyboard.Flags |= 1; // KEYEVENTF_EXTENDEDKEY, use this for scan codes beginning in E0
            }

            input.Data.Keyboard.Scan = (ushort)scanCode;

            if (wParam == (IntPtr)WM_KEYUP)
            {
                if (delay > 0)
                {
                    Thread.Sleep(delay);
                }
                input.Data.Keyboard.Flags |= 2; // KEYEVENTF_KEYUP;
            }

            //Send the action
            inputs[0] = input;
            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
    }
}
