/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using BlackSharp.UI.Avalonia.Extensions;
using LibreDiagnostics.Models.Enums;
using LibreDiagnostics.Models.Globals;
using LibreDiagnostics.UI.Platform.Windows.Interop;
using LibreDiagnostics.UI.Platform.Windows.Interop.Structures;
using System.Runtime.InteropServices;

namespace LibreDiagnostics.UI.Platform.Windows
{
    internal class AppBarTask
    {
        #region Constructor

        public AppBarTask(Window window)
        {
            _Window = window;
        }

        #endregion

        #region Fields

        delegate void ModifyMessageCallback(ref APPBARDATA data);

        const string AppBarMessage = nameof(LibreDiagnostics) + nameof(AppBarMessage);

        int _AppBarMessageID = -1;

        bool _IsAppBarRegistered = false;
        bool _IsResizing = false;

        Window _Window;

        #endregion

        #region Public

        public void EnableAppBar()
        {
            Win32Properties.AddWndProcHookCallback(_Window, WndProc);

            _AppBarMessageID = User32.RegisterWindowMessage(AppBarMessage);

            SendAppBarMessage(Shell32.ABM_NEW);
            _IsAppBarRegistered = true;

            AppBarUpdate();
        }

        public void DisableAppBar()
        {
            if (!_IsAppBarRegistered)
            {
                return;
            }

            SendAppBarMessage(Shell32.ABM_REMOVE);
            _IsAppBarRegistered = false;

            Win32Properties.RemoveWndProcHookCallback(_Window, WndProc);
        }

        #endregion

        #region Private

        void AppBarUpdate()
        {
            var settings = Global.Settings;

            if (_IsResizing)
            {
                return;
            }

            var screen = ScreenManager.GetScreen(_Window.Screens.All, settings.ScreenInfo) ?? _Window.Screens.ScreenFromWindow(_Window);
            var renderScaling = screen.Scaling;
            var workArea = screen.Bounds;

            var desiredWidthPx = Math.Round(_Window.Width * renderScaling);

            var horizontalOffsetPx = (int)Math.Round(settings.HorizontalOffset * renderScaling);
            var verticalOffsetPx   = (int)Math.Round(settings.VerticalOffset   * renderScaling);

            var width = (int)desiredWidthPx;

            PixelRect appBarBounds = default;

            switch (settings.DockingPosition)
            {
                case DockingPosition.Left:
                    appBarBounds = new(workArea.TopLeft.X, workArea.TopLeft.Y, width, workArea.Height);
                    break;
                case DockingPosition.Right:
                    appBarBounds = new(workArea.TopRight.X - width, workArea.TopRight.Y, width, workArea.Height);
                    break;
            }

            appBarBounds = appBarBounds.Offset(horizontalOffsetPx, verticalOffsetPx);

            SendAppBarMessage(Shell32.ABM_QUERYPOS, (ref d) =>
            {
                d.rc = new()
                {
                    Top    = appBarBounds.TopLeft.Y,
                    Bottom = appBarBounds.Bottom,
                    Left   = appBarBounds.TopLeft.X,
                    Right  = appBarBounds.Right,
                };
            }, out var data);

            SendAppBarMessage(Shell32.ABM_SETPOS, ref data);

            _IsResizing = true;

            _Window.Position = new(data.rc.Left, data.rc.Top);
            _Window.Height   = data.rc.Height == 0 ? 0 : data.rc.Height / renderScaling;
            _Window.Width    = data.rc.Width  == 0 ? 0 : data.rc.Width  / renderScaling;

            _IsResizing = false;
        }

        void SendAppBarMessage(int message)
        {
            SendAppBarMessage(message, null, out _);
        }

        void SendAppBarMessage(int message, ModifyMessageCallback callback, out APPBARDATA data)
        {
            data = new()
            {
                cbSize           = Marshal.SizeOf<APPBARDATA>(),
                hWnd             = _Window.TryGetPlatformHandle().Handle,
                uCallbackMessage = _AppBarMessageID,
                uEdge            = (int)Global.Settings.DockingPosition,
            };

            callback?.Invoke(ref data);

            SendAppBarMessage(message, ref data);
        }

        void SendAppBarMessage(int message, ref APPBARDATA data)
        {
            Shell32.SHAppBarMessage(message, ref data);
        }

        IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case Shell32.WM_ACTIVATE:
                    SendAppBarMessage(Shell32.ABM_ACTIVATE);
                    break;
                case Shell32.WM_WINDOWPOSCHANGING:
                    if (!_IsResizing)
                    {
                        var pos = Marshal.PtrToStructure<WINDOWPOS>(lParam);

                        var isSystemMove =
                            (pos.flags & Shell32.SWP_NOSIZE      ) == 0
                         || (pos.flags & Shell32.SWP_FRAMECHANGED) != 0;

                        if (!isSystemMove)
                        {
                            pos.flags |= Shell32.SWP_NOMOVE;

                            Marshal.StructureToPtr(pos, lParam, false);
                        }
                    }
                    break;
                case Shell32.WM_WINDOWPOSCHANGED:
                    SendAppBarMessage(Shell32.ABM_WINDOWPOSCHANGED);
                    break;
                case Shell32.WM_DISPLAYCHANGE:
                case Shell32.WM_SETTINGCHANGE:
                    DelayedAppBarUpdate();
                    break;
                default:
                    if (msg == _AppBarMessageID)
                    {
                        switch (wParam.ToInt32())
                        {
                            case Shell32.ABN_POSCHANGED:
                                DelayedAppBarUpdate();
                                handled = true;
                                break;
                            case Shell32.ABN_FULLSCREENAPP:
                                if (lParam.ToInt32() > 0)
                                {
                                    WindowStyleHandler.SetBottom(_Window, false);
                                }
                                else
                                {
                                    WindowStyleHandler.SetTopMost(_Window, true);
                                }

                                DelayedAppBarUpdate();
                                break;
                        }
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        void DelayedAppBarUpdate()
        {
            DispatcherTimer.RunOnce(AppBarUpdate, TimeSpan.FromMilliseconds(500));
        }

        #endregion
    }
}
