using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace YuuyaPad.Utils
{
    internal static class SystemMenuHelper
    {
        private const int MF_BYCOMMAND = 0x00000000;
        private const int MF_BYPOSITION = 0x00000400;

        private const int SC_RESTORE = 0xF120;
        private const int SC_SIZE = 0xF000;
        private const int SC_MINIMIZE = 0xF020;
        private const int SC_MAXIMIZE = 0xF030;

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool RemoveMenu(IntPtr hMenu, int uPosition, int uFlags);

        /// <summary>
        /// Dialog-style system menu
        /// </summary>
        public static void KeepMoveAndCloseOnly(Form form)
        {
            if (form == null || form.IsDisposed)
                return;

            IntPtr hMenu = GetSystemMenu(form.Handle, false);

            // Remove unnecessary items from the dialog
            RemoveMenu(hMenu, SC_RESTORE, MF_BYCOMMAND);
            RemoveMenu(hMenu, SC_SIZE, MF_BYCOMMAND);
            RemoveMenu(hMenu, SC_MINIMIZE, MF_BYCOMMAND);
            RemoveMenu(hMenu, SC_MAXIMIZE, MF_BYCOMMAND);

            // Remove the separator line
            RemoveMenu(hMenu, 1, MF_BYPOSITION);
        }
    }
}
