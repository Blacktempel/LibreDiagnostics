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
using Avalonia.Data;
using Avalonia.Platform;
using BlackSharp.Core.Extensions;
using BlackSharp.Core.Logging;
using BlackSharp.MVVM.Dialogs.Enums;
using CommunityToolkit.Mvvm.Input;
using LibreDiagnostics.Language.Resources;
using LibreDiagnostics.Models.Configuration;
using LibreDiagnostics.Models.Enums;
using LibreDiagnostics.Models.Globals;
using LibreDiagnostics.Models.Interfaces;
using LibreDiagnostics.MVVM.Utilities;
using LibreDiagnostics.UI.Platform.Windows.Interop;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using OS = BlackSharp.Core.Platform.OperatingSystem;

namespace LibreDiagnostics.UI.Models
{
    internal partial class TrayIconImplementation : ITrayIcon
    {
        #region Constructor

        public TrayIconImplementation(Settings settings)
        {
            var icon = GetTrayIcon();
            if (icon == null)
            {
                return;
            }

            _UpdateMenu = new NativeMenuItem
            {
                Header = Resources.ButtonUpdate,
                Command = UpdateRequestedCommand,
            };

            icon.Clicked += TrayIconClicked;

            //Manually add menu
            icon.Menu = new NativeMenu
            {
                new NativeMenuItem
                {
                    Header = Resources.ButtonSettings,
                    Command = SettingsRequestedCommand,
                },
                new NativeMenuItem
                {
                    Header = Resources.ButtonReport,
                    Command = ReportRequestedCommand,
                },
                new NativeMenuItemSeparator(),
                new NativeMenuItem
                {
                    Header = Resources.ButtonDonate,
                    Command = DonateRequestedCommand,
                },
                new NativeMenuItem
                {
                    Header = Resources.ButtonGithub,
                    Command = GithubRequestedCommand,
                },
                _UpdateMenu,
                new NativeMenuItemSeparator(),
                new NativeMenuItem
                {
                    Header = Resources.ButtonRestart,
                    Command = RestartRequestedCommand,
                },
                new NativeMenuItem
                {
                    Header = Resources.ButtonClose,
                    Command = CloseRequestedCommand,
                },
            };

            //Add binding for visibility
            icon.ClearValue(TrayIcon.IsVisibleProperty);

            var binding = new Binding
            {
                Source = settings,
                Path = nameof(Settings.ShowTrayIcon),
                Mode = BindingMode.TwoWay
            };
            icon.Bind(TrayIcon.IsVisibleProperty, binding);

            //Add tooltip
            var version = Assembly.GetEntryAssembly()?.GetName().Version;
            icon.ToolTipText = $"{Resources.AppName} v{version.ToString(3)}";

            ChangeTrayIconIcon(TrayIconID.Default);
        }

        #endregion

        #region Fields

        const string DefaultTrayIconIcon         = @"avares://LibreDiagnostics.UI/Assets/Icon.ico";
        const string UpdateAvailableTrayIconIcon = @"avares://LibreDiagnostics.UI/Assets/Icon_Update.ico";

        /// <summary>
        /// Time in seconds.
        /// </summary>
        const int UpdateAlreadyUpToDateTimeout = 5;

        NativeMenuItem _UpdateMenu = null;

        DateTime? _LastClickTime;

        #endregion

        #region Public

        public void ChangeTrayIconIcon(TrayIconID trayIconID)
        {
            var icon = GetTrayIcon();
            if (icon == null)
            {
                return;
            }

            switch (trayIconID)
            {
                case TrayIconID.Default:
                    icon.Icon = LoadIcon(DefaultTrayIconIcon);
                    break;
                case TrayIconID.UpdateAvailable:
                    icon.Icon = LoadIcon(UpdateAvailableTrayIconIcon);

                    //Update menu to show an update is available
                    _UpdateMenu.Header = Resources.ButtonUpdateAvailable;
                    break;
            }
        }

        #endregion

        #region Private

        TrayIcon GetTrayIcon()
        {
            return Application.Current?.GetValue(TrayIcon.IconsProperty)?.FirstOrDefault();
        }

        WindowIcon LoadIcon(string resourceUri)
        {
            var uri = new Uri(resourceUri);
            using var stream = AssetLoader.Open(uri);
            return new WindowIcon(stream);
        }

        void TrayIconClicked(object sender, EventArgs e)
        {
            var now = DateTime.Now;

            if (_LastClickTime == null)
            {
                _LastClickTime = now;
                return;
            }

            var delta = (now - _LastClickTime).Value;

            //Check if time between clicks is too long
            if (delta.TotalMilliseconds > GetDoubleClickTime())
            {
                _LastClickTime = now;
                return;
            }
            else //Double click
            {
                _LastClickTime = null;

                if (SettingsRequestedCommand.CanExecute(null))
                {
                    SettingsRequestedCommand.Execute(null);
                }
            }
        }

        void OpenInBrowser(string url)
        {
            if (OS.IsWindows())
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (OS.IsLinux())
            {
                Process.Start("xdg-open", url);
            }
        }

        uint GetDoubleClickTime()
        {
            if (OS.IsWindows())
            {
                return User32.GetDoubleClickTime();
            }
            else
            {
                //Default value for other OS
                return 500;
            }
        }

        #endregion

        #region Commands

        [RelayCommand()]
        void SettingsRequested()
        {
            if (Global.IsReady)
            {
                MessageBro.DoOpenSettings();
            }
        }

        [RelayCommand]
        async Task ReportRequested()
        {
            var filePath = await MessageBro.DoSaveFile();

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            var reportLHM = Global.HardwareManager?.GetReport();

            if (string.IsNullOrEmpty(reportLHM))
            {
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine(reportLHM);
            sb.AppendLine();
            sb.AppendLine($"{nameof(LibreDiagnostics)} Report:");

            var screens = MessageBro.DoGetScreens();

            sb.AppendLine($"Number of Screens: {screens.Count}");
            foreach (var screen in screens)
            {
                sb.AppendLine($"  {nameof(screen.Name)}: {screen.Name}");
                sb.AppendLine($"    {nameof(screen.Strategy)}: {screen.Strategy}");
                sb.AppendLine($"    {nameof(screen.ScreenID)}: {screen.ScreenID}");
            }

            sb.AppendLine();

            File.WriteAllText(filePath, sb.ToString());
        }

        [RelayCommand]
        void RestartRequested()
        {
            Process.Start(Environment.ProcessPath);
            MessageBro.DoShutdownApplication();
        }

        [RelayCommand]
        void DonateRequested()
        {
            OpenInBrowser(@"https://github.com/sponsors/Blacktempel");
        }

        [RelayCommand]
        void GithubRequested()
        {
            OpenInBrowser(@"https://github.com/Blacktempel/LibreDiagnostics");
        }

        [RelayCommand]
        async Task UpdateRequested()
        {
            var result = DialogButtonType.Invalid;

            do
            {
                try
                {
                    var updateCheckResult = await Client.CheckUpdateAvailable(false);

                    //Require confirmation
                    await Client.TryUpdate(updateCheckResult, true);

                    //Already up to date
                    if (updateCheckResult != null && !updateCheckResult.IsUpdateAvailable)
                    {
                        MessageBro.DoShowMessageTimeout(
                            Resources.UpdateAlreadyUpToDateTitle,
                            Resources.UpdateAlreadyUpToDateMessage,
                            DialogButtons.OK,
                            TimeSpan.FromSeconds(UpdateAlreadyUpToDateTimeout),
                            out _);
                    }
                }
                catch (Exception e)
                {
                    var fullExceptionString = e.FullExceptionString();

                    Logger.Instance.Add(LogLevel.Error, fullExceptionString, DateTime.Now);

                    //Format message to include exception string
                    var message = string.Format(Resources.UpdateFailedMessage.Replace(@"\n", Environment.NewLine), fullExceptionString);

                    result = MessageBro.DoShowMessage(Resources.UpdateFailedTitle, message, DialogButtons.RetryCancel);
                }
            }
            while (result == DialogButtonType.Retry);
        }

        [RelayCommand]
        void CloseRequested()
        {
            MessageBro.DoShutdownApplication();
        }

        #endregion
    }
}
