/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using CommunityToolkit.Mvvm.Input;
using LibreDiagnostics.Models.Enums;

namespace LibreDiagnostics.Models.Interfaces
{
    public interface ITrayIcon
    {
        IRelayCommand      SettingsRequestedCommand  { get; }
        IAsyncRelayCommand ReportRequestedCommand    { get; }
        IRelayCommand      RestartRequestedCommand   { get; }
        IRelayCommand      DonateRequestedCommand    { get; }
        IRelayCommand      GithubRequestedCommand    { get; }
        IAsyncRelayCommand UpdateRequestedCommand    { get; }
        IRelayCommand      CloseRequestedCommand     { get; }

        void ChangeTrayIconIcon(TrayIconID trayIconID);
    }
}
