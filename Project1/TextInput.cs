/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * XnaTextInput.TextInputHandler - benryves@benryves.com                                     *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * This is quick and very, VERY dirty.                                                       *
 * It uses Win32 message hooks to grab messages (as we don't get a nicely wrapped WndProc).  *
 * I couldn't get WH_KEYBOARD to work (accessing the data via its pointer resulted in access *
 * violation exceptions), nor could I get WH_CALLWNDPROC to work.                            *
 * Maybe someone who actually knows what they're  doing can work something out that's not so *
 * kludgy.                                                                                   *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * This quite obviously relies on a Win32 nastiness, so this is for Windows XNA games only!  *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

#region Using Statements
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms; // This class exposes WinForms-style key events.
#endregion

namespace Start_a_Town_
{

    /// <summary>
    /// A class to provide text input capabilities to an XNA application via Win32 hooks.
    /// </summary>
    public class TextInputHandler : IDisposable
    {

        #region Win32

        /// <summary>
        /// Types of hook that can be installed using the SetWindwsHookEx function.
        /// </summary>
        public enum HookId
        {
            WH_CALLWNDPROC = 4,
            WH_CALLWNDPROCRET = 12,
            WH_CBT = 5,
            WH_DEBUG = 9,
            WH_FOREGROUNDIDLE = 11,
            WH_GETMESSAGE = 3,
            WH_HARDWARE = 8,
            WH_JOURNALPLAYBACK = 1,
            WH_JOURNALRECORD = 0,
            WH_KEYBOARD = 2,
            WH_KEYBOARD_LL = 13,
            WH_MAX = 11,
            WH_MAXHOOK = WH_MAX,
            WH_MIN = -1,
            WH_MINHOOK = WH_MIN,
            WH_MOUSE_LL = 14,
            WH_MSGFILTER = -1,
            WH_SHELL = 10,
            WH_SYSMSGFILTER = 6,
        };

        /// <summary>
        /// Window message types.
        /// </summary>
        /// <remarks>Heavily abridged, naturally.</remarks>
        public enum WindowMessage
        {
            WM_KEYDOWN = 0x100,
            WM_KEYUP = 0x101,
            WM_CHAR = 0x102,
            WM_SYSKEYDOWN = 0x104,
            WM_MOUSEMOVE = 0x0200,
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208,
            WM_MOUSEWHEEL = 0x020A,
            WM_LBUTTONDBLCLK = 0x0203,
            WM_RBUTTONDBLCLK = 0x0206,
        }

        public enum Buttons
        {
            MK_CONTROL = 0x0008,
            MK_LBUTTON = 0x0001,
            MK_MBUTTON = 0x0010,
            MK_RBUTTON = 0x0002,
            MK_SHIFT = 0x0004,
            MK_XBUTTON1 = 0x0020,
            MK_XBUTTON2 = 0x0040
        }

        System.Windows.Forms.MouseButtons TranslateButton(Buttons button)
        {
            switch (button)
            {
                case Buttons.MK_LBUTTON:
                    return System.Windows.Forms.MouseButtons.Left;
                case Buttons.MK_RBUTTON:
                    return System.Windows.Forms.MouseButtons.Right;
                case Buttons.MK_XBUTTON1:
                    return System.Windows.Forms.MouseButtons.XButton1;
                case Buttons.MK_XBUTTON2:
                    return System.Windows.Forms.MouseButtons.XButton2;
                default:
                    return System.Windows.Forms.MouseButtons.None;
            }
        }

        /// <summary>
        /// A delegate used to create a hook callback.
        /// </summary>
        public delegate int GetMsgProc(int nCode, int wParam, ref Message msg);

        /// <summary>
        /// Install an application-defined hook procedure into a hook chain.
        /// </summary>
        /// <param name="idHook">Specifies the type of hook procedure to be installed.</param>
        /// <param name="lpfn">Pointer to the hook procedure.</param>
        /// <param name="hmod">Handle to the DLL containing the hook procedure pointed to by the lpfn parameter.</param>
        /// <param name="dwThreadId">Specifies the identifier of the thread with which the hook procedure is to be associated.</param>
        /// <returns>If the function succeeds, the return value is the handle to the hook procedure. Otherwise returns 0.</returns>
        [DllImport("user32.dll", EntryPoint = "SetWindowsHookExA")]
        public static extern IntPtr SetWindowsHookEx(HookId idHook, GetMsgProc lpfn, IntPtr hmod, int dwThreadId);

        /// <summary>
        /// Removes a hook procedure installed in a hook chain by the SetWindowsHookEx function. 
        /// </summary>
        /// <param name="hHook">Handle to the hook to be removed. This parameter is a hook handle obtained by a previous call to SetWindowsHookEx.</param>
        /// <returns>If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll")]
        public static extern int UnhookWindowsHookEx(IntPtr hHook);

        /// <summary>
        /// Passes the hook information to the next hook procedure in the current hook chain.
        /// </summary>
        /// <param name="hHook">Ignored.</param>
        /// <param name="ncode">Specifies the hook code passed to the current hook procedure.</param>
        /// <param name="wParam">Specifies the wParam value passed to the current hook procedure.</param>
        /// <param name="lParam">Specifies the lParam value passed to the current hook procedure.</param>
        /// <returns>This value is returned by the next hook procedure in the chain.</returns>
        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(int hHook, int ncode, int wParam, ref Message lParam);

        /// <summary>
        /// Translates virtual-key messages into character messages.
        /// </summary>
        /// <param name="lpMsg">Pointer to an Message structure that contains message information retrieved from the calling thread's message queue.</param>
        /// <returns>If the message is translated (that is, a character message is posted to the thread's message queue), the return value is true.</returns>
        [DllImport("user32.dll")]
        public static extern bool TranslateMessage(ref Message lpMsg);


        /// <summary>
        /// Retrieves the thread identifier of the calling thread.
        /// </summary>
        /// <returns>The thread identifier of the calling thread.</returns>
        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();

        #endregion

        #region Hook management and class construction.

        /// <summary>Handle for the created hook.</summary>
        private readonly IntPtr HookHandle;

        private readonly GetMsgProc ProcessMessagesCallback;

        /// <summary>Create an instance of the TextInputHandler.</summary>
        /// <param name="whnd">Handle of the window you wish to receive messages (and thus keyboard input) from.</param>
        public TextInputHandler(IntPtr whnd)
        {
            // Create the delegate callback:
            this.ProcessMessagesCallback = new GetMsgProc(ProcessMessages);
            // Create the keyboard hook:
            this.HookHandle = SetWindowsHookEx(HookId.WH_GETMESSAGE, this.ProcessMessagesCallback, IntPtr.Zero, GetCurrentThreadId());
        }

        public void Dispose()
        {
            // Remove the hook.
            if (this.HookHandle != IntPtr.Zero) UnhookWindowsHookEx(this.HookHandle);
        }

        #endregion

        #region Message processing

        private int ProcessMessages(int nCode, int wParam, ref Message msg)
        {
            //try
            //{
                // Check if we must process this message (and whether it has been retrieved via GetMessage):
                if (nCode == 0 && wParam == 1)
                {

                    // We need character input, so use TranslateMessage to generate WM_CHAR messages.
                    // WARNING this caused a duplicate wm_char message after switching to vs2022 and monogame
                    // commenting it out doesn't seem to break anything, seems like a wm_char message arrives even without it
                    //TranslateMessage(ref msg);

                    // If it's one of the keyboard-related messages, raise an event for it:
                    switch ((WindowMessage)msg.Msg)
                    {
                        case WindowMessage.WM_CHAR:
                            this.OnKeyPress(new KeyPressEventArgs((char)msg.WParam));
                            break;
                        case WindowMessage.WM_SYSKEYDOWN:
                        case WindowMessage.WM_KEYDOWN:
                            this.OnKeyDown(new KeyEventArgs((Keys)msg.WParam));
                            break;
                        case WindowMessage.WM_KEYUP:
                            this.OnKeyUp(new KeyEventArgs((Keys)msg.WParam));
                            break;
                        case WindowMessage.WM_MOUSEMOVE:
                            this.OnMouseMove(new HandledMouseEventArgs(TranslateButton((Buttons)GetLowWord((int)msg.WParam)), 0, GetLowWord((int)msg.LParam), GetHighWord((int)msg.LParam), GetHighWord(msg.WParam.ToInt32()) / 120));//new MouseEventArgs((Keys)msg.WParam));
                            break;
                        case WindowMessage.WM_LBUTTONDOWN:
                           // DateTime.Now.ToConsole();
                            this.OnLButtonDown(new HandledMouseEventArgs(TranslateButton((Buttons)GetLowWord((int)msg.WParam)), 0, GetLowWord((int)msg.LParam), GetHighWord((int)msg.LParam), GetHighWord(msg.WParam.ToInt32()) / 120));
                            break;
                        case WindowMessage.WM_LBUTTONUP:
                            this.OnLMouseUp(new HandledMouseEventArgs(TranslateButton((Buttons)GetLowWord((int)msg.WParam)), 0, GetLowWord((int)msg.LParam), GetHighWord((int)msg.LParam), GetHighWord(msg.WParam.ToInt32()) / 120));
                            break;
                        case WindowMessage.WM_RBUTTONDOWN:
                            this.OnRMouseDown(new HandledMouseEventArgs(TranslateButton((Buttons)GetLowWord((int)msg.WParam)), 0, GetLowWord((int)msg.LParam), GetHighWord((int)msg.LParam), GetHighWord(msg.WParam.ToInt32()) / 120));
                            break;
                        case WindowMessage.WM_RBUTTONUP:
                            this.OnRMouseUp(new HandledMouseEventArgs(TranslateButton((Buttons)GetLowWord((int)msg.WParam)), 0, GetLowWord((int)msg.LParam), GetHighWord((int)msg.LParam), GetHighWord(msg.WParam.ToInt32()) / 120));
                            break;
                        case WindowMessage.WM_MBUTTONDOWN:
                            this.OnMMouseDown(new HandledMouseEventArgs(TranslateButton((Buttons)GetLowWord((int)msg.WParam)), 0, GetLowWord((int)msg.LParam), GetHighWord((int)msg.LParam), GetHighWord(msg.WParam.ToInt32()) / 120));
                            break;
                        case WindowMessage.WM_MBUTTONUP:
                            this.OnMMouseUp(new HandledMouseEventArgs(TranslateButton((Buttons)GetLowWord((int)msg.WParam)), 0, GetLowWord((int)msg.LParam), GetHighWord((int)msg.LParam), GetHighWord(msg.WParam.ToInt32()) / 120));
                            break;
                        case WindowMessage.WM_MOUSEWHEEL:
                        //   int lw = GetLowWord((int)msg.WParam);
                        //  System.Windows.Forms.MouseButtons button = (System.Windows.Forms.MouseButtons)lw;

                        var longlp = (long)msg.LParam;
                        var llp = GetLowWord64(longlp);
                        var hlp = GetHighWord64(longlp);

                        var longwp = (long)msg.WParam;
                        var btnlwp = (Buttons)GetLowWord64(longwp);

                        var hwp = GetHighWord64(longwp);

                        var hmea = new HandledMouseEventArgs(TranslateButton(btnlwp), 0, llp, hlp, hwp / 120);
                        this.OnMouseWheel(hmea);

                        //this.OnMouseWheel(new HandledMouseEventArgs(TranslateButton((Buttons)GetLowWord((int)msg.WParam)), 0, GetLowWord((int)msg.LParam), GetHighWord((int)msg.LParam), GetHighWord(msg.WParam.ToInt32()) / 120));
                        break;

                            // had to make double click fire a left click event because dragdropping on the same slot quickly, resulted in glitch where object disappeared
                        case WindowMessage.WM_LBUTTONDBLCLK:
                            //"dbl".ToConsole();
                            this.OnLButtonDblClk(new HandledMouseEventArgs(TranslateButton((Buttons)GetLowWord((int)msg.WParam)), 0, GetLowWord((int)msg.LParam), GetHighWord((int)msg.LParam), GetHighWord(msg.WParam.ToInt32()) / 120));
                            //this.OnLButtonDown(new HandledMouseEventArgs(TranslateButton((Buttons)GetLowWord((int)msg.WParam)), 0, GetLowWord((int)msg.LParam), GetHighWord((int)msg.LParam), GetHighWord(msg.WParam.ToInt32()) / 120));
                            break;

                        case WindowMessage.WM_RBUTTONDBLCLK:
                            //"dbl".ToConsole();
                            this.OnRMouseDown(new HandledMouseEventArgs(TranslateButton((Buttons)GetLowWord((int)msg.WParam)), 0, GetLowWord((int)msg.LParam), GetHighWord((int)msg.LParam), GetHighWord(msg.WParam.ToInt32()) / 120));
                            break;
                    }

                }

                // Call next hook in chain:
                return CallNextHookEx(0, nCode, wParam, ref msg);
            //}
            //catch (Exception e) { System.Windows.Forms.MessageBox.Show(e.ToString()); return CallNextHookEx(0, nCode, wParam, ref msg); }
        }


        #endregion

        #region Events

        public event KeyEventHandler KeyUp;
        protected virtual void OnKeyUp(KeyEventArgs e)
        {
            Controller.Input.UpdateKeyStates();
            if (this.KeyUp != null) this.KeyUp(this, e);
        }

        public event KeyEventHandler KeyDown;
        protected virtual void OnKeyDown(KeyEventArgs e)
        {
            Controller.Input.UpdateKeyStates();
            if (this.KeyDown != null) this.KeyDown(this, e);
        }

        public event KeyPressEventHandler KeyPress;
        protected virtual void OnKeyPress(KeyPressEventArgs e)
        {
            if (this.KeyPress != null) this.KeyPress(this, e);
        }

        public event EventHandler<System.Windows.Forms.HandledMouseEventArgs> MouseMove;
        protected virtual void OnMouseMove(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.MouseMove != null) this.MouseMove(this, e);
        }

        public event EventHandler<System.Windows.Forms.HandledMouseEventArgs> LButtonDown;
        protected virtual void OnLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.LButtonDown != null) this.LButtonDown(this, e);
        }

        public event EventHandler<System.Windows.Forms.HandledMouseEventArgs> LMouseUp;
        protected virtual void OnLMouseUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.LMouseUp != null) this.LMouseUp(this, e);
        }

        public event EventHandler<HandledMouseEventArgs> RMouseDown;
        protected virtual void OnRMouseDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.RMouseDown != null) this.RMouseDown(this, e);
        }

        public event EventHandler<HandledMouseEventArgs> RMouseUp;
        protected virtual void OnRMouseUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.RMouseUp != null) this.RMouseUp(this, e);
        }

        public event EventHandler<HandledMouseEventArgs> MMouseUp;
        protected virtual void OnMMouseUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.MMouseUp != null) this.MMouseUp(this, e);
        } 
        public event EventHandler<HandledMouseEventArgs> MMouseDown;
        protected virtual void OnMMouseDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.MMouseDown != null) this.MMouseDown(this, e);
        }

        public event EventHandler<HandledMouseEventArgs> MouseWheel;
        protected virtual void OnMouseWheel(HandledMouseEventArgs e)
        {
            if (this.MouseWheel != null) this.MouseWheel(this, e);
        }

        public event EventHandler<HandledMouseEventArgs> LButtonDblClk;
        private void OnLButtonDblClk(HandledMouseEventArgs e)
        {
            if (this.LButtonDblClk != null) this.LButtonDblClk(this, e);
        }
        #endregion

        public static int GetHighWord(int intValue)
        {
            //return (intValue & (0xFFFF << 16));
            return intValue >> 16;
        }

        public static int GetLowWord(int intValue)
        {
            return (intValue & 0x0000FFFF);
        }
        public static int GetHighWord64(long intValue)
        {
            //return (intValue & (0xFFFF << 16));
            var val = intValue >> 256;
            return (int)(val);
        }
        public static int GetLowWord64(long intValue)
        {
            return (int)(intValue & 0x0000FFFF);
        }
    }
}
