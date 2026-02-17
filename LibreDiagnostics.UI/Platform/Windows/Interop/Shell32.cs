/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using LibreDiagnostics.UI.Platform.Windows.Interop.Structures;
using System.Runtime.InteropServices;

namespace LibreDiagnostics.UI.Platform.Windows.Interop
{
    internal static class Shell32
    {
        const string DLL_NAME = "shell32.dll";

        public const int ABM_NEW              =  0;
        public const int ABM_REMOVE           =  1;
        public const int ABM_QUERYPOS         =  2;
        public const int ABM_SETPOS           =  3;
        public const int ABM_GETSTATE         =  4;
        public const int ABM_GETTASKBARPOS    =  5;
        public const int ABM_ACTIVATE         =  6;
        public const int ABM_GETAUTOHIDEBAR   =  7;
        public const int ABM_SETAUTOHIDEBAR   =  8;
        public const int ABM_WINDOWPOSCHANGED =  9;
        public const int ABM_SETSTATE         = 10;

        public const int ABN_STATECHANGE   = 0;
        public const int ABN_POSCHANGED    = 1;
        public const int ABN_FULLSCREENAPP = 2;
        public const int ABN_WINDOWARRANGE = 3;

        public const int SWP_NOSIZE       = 0x0001;
        public const int SWP_NOMOVE       = 0x0002;
        public const int SWP_FRAMECHANGED = 0x0020;

        public const int WM_ACTIVATE          = 0x0006;
        public const int WM_WINDOWPOSCHANGING = 0x0046;
        public const int WM_WINDOWPOSCHANGED  = 0x0047;
        public const int WM_DISPLAYCHANGE     = 0x007E;
        public const int WM_SETTINGCHANGE     = 0x001A;

        [DllImport(DLL_NAME)]
        public static extern UIntPtr SHAppBarMessage(int dwMessage, ref APPBARDATA pData);
    }
}
