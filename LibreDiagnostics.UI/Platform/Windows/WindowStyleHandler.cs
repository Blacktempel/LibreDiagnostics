/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using Avalonia.Controls;
using Avalonia.Input;
using LibreDiagnostics.UI.Platform.Windows.Interop;
using OS = BlackSharp.Core.Platform.OperatingSystem;

namespace LibreDiagnostics.UI.Platform.Windows
{
    internal static class WindowStyleHandler
    {
        #region Public

        public static void SetClickThrough(Window window, bool set)
        {
            if (!OS.IsWindows())
            {
                return; //TODO
            }

            window.IsHitTestVisible = !set;

            if (window.Content is InputElement ie)
            {
                ie.IsHitTestVisible = !set;
            }

            if (set)
            {
                SetWindowLong(window, User32.WS_EX_TRANSPARENT | User32.WS_EX_LAYERED, null);
            }
            else
            {
                SetWindowLong(window, null, User32.WS_EX_TRANSPARENT | User32.WS_EX_LAYERED);
            }
        }

        public static void SetIsInAltTab(Window window, bool set)
        {
            if (!OS.IsWindows())
            {
                return; //TODO
            }

            if (set)
            {
                SetWindowLong(window, null, User32.WS_EX_TOOLWINDOW);
            }
            else
            {
                SetWindowLong(window, User32.WS_EX_TOOLWINDOW, null);
            }
        }

        public static void SetTopMost(Window window, bool set)
        {
            if (!OS.IsWindows())
            {
                return; //TODO
            }

            if (set)
            {
                SetWindowPos(window, User32.HWND_TOPMOST, set);
            }
            else
            {
                SetWindowPos(window, User32.HWND_NOTOPMOST, set);
            }
        }

        public static void SetBottom(Window window, bool set)
        {
            SetWindowPos(window, User32.HWND_BOTTOM, set);
        }

        #endregion

        #region Private

        static void SetWindowLong(Window window, long? add, long? remove)
        {
            if (!OS.IsWindows())
            {
                return; //TODO
            }

            var handle = window.TryGetPlatformHandle()?.Handle;
            if (handle == null)
            {
                return;
            }

            var hwnd = handle.Value;

            long style;

            if (Environment.Is64BitProcess)
            {
                style = User32.GetWindowLongPtr(hwnd, User32.GWL_EXSTYLE);
            }
            else
            {
                style = User32.GetWindowLong(hwnd, User32.GWL_EXSTYLE);
            }

            if (add.HasValue)
            {
                style |= add.Value;
            }

            if (remove.HasValue)
            {
                style &= ~remove.Value;
            }

            if (Environment.Is64BitProcess)
            {
                style = User32.SetWindowLongPtr(hwnd, User32.GWL_EXSTYLE, style);
            }
            else
            {
                style = User32.SetWindowLong(hwnd, User32.GWL_EXSTYLE, style);
            }
        }

        static void SetWindowPos(Window window, IntPtr hwndAfter, bool set)
        {
            uint flags = User32.SWP_NOSIZE | User32.SWP_NOMOVE;

            if (!set)
            {
                flags |= User32.SWP_NOACTIVATE;
            }

            var handle = window.TryGetPlatformHandle()?.Handle;
            if (handle == null)
            {
                return;
            }

            User32.SetWindowPos
            (
                handle.Value,
                hwndAfter,
                0, 0, 0, 0,
                flags
            );
        }

        #endregion
    }
}
