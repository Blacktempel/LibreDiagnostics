/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using BlackSharp.MVVM.Dialogs;
using BlackSharp.MVVM.Dialogs.Enums;
using BlackSharp.UI.Avalonia.Extensions;
using BlackSharp.UI.Avalonia.Windows.Dialogs;
using BlackSharp.UI.Avalonia.Windows.Dialogs.Enums;
using LibreDiagnostics.Language.Resources;
using LibreDiagnostics.Models.Events;
using LibreDiagnostics.Models.Helper;
using LibreDiagnostics.MVVM.Utilities;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Storage;

namespace LibreDiagnostics.UI.Utilities
{
    internal static class NavigationResolver
    {
        #region Constructor

        static NavigationResolver()
        {
            MessageBro.ShowMessage += ShowMessage;
            MessageBro.ShowMessageTimeout += ShowMessageTimeout;
            MessageBro.OpenSettings += OpenSettings;
            MessageBro.ShutdownApplication += ShutdownApplication;
            MessageBro.SaveFile += SaveFile;
            MessageBro.GetScreens += GetScreens;
            MessageBro.CheckForUpdate += CheckForUpdate;

            EventDistributor.ShowDriveDetailsEvent += ShowDriveDetails;
            EventDistributor.ShowRAMDetailsEvent += ShowRAMDetails;
        }

        #endregion

        #region Events

        static DialogButtonType ShowMessage(string title, string message, DialogButtons buttons)
        {
            var msgBox = new MessageBox(DialogType.Information, DialogSize.Medium);
            msgBox.ShowDialog(title, message, buttons);
            return msgBox.Result;
        }

        static DialogButtonType ShowMessageTimeout(string title, string message, DialogButtons buttons, IList<DialogButton> dialogButtons, TimeSpan? timeout, out bool timeouted)
        {
            var msgBox = new TimeoutMessageBox(timeout, DialogType.Information, DialogSize.Medium);
            msgBox.ShowDialog(title, message, buttons, dialogButtons);

            timeouted = msgBox.Timeouted;
            return msgBox.Result;
        }

        static void OpenSettings()
        {
            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime wnd
             && wnd.MainWindow != null)
            {
                new Windows.SettingsWindow().ShowDialog(wnd.MainWindow);
            }
            else
            {
                new Windows.SettingsWindow().Show();
            }
        }

        static void ShutdownApplication()
        {
            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime wnd
             && wnd.MainWindow != null)
            {
                wnd.Shutdown();
            }
            else
            {
                Console.WriteLine("Warning: doing a hard shutdown of application.");
                Environment.Exit(0);
            }
        }

        static async Task<string> SaveFile()
        {
            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime wnd
             && wnd.MainWindow != null)
            {
                var options = new FilePickerSaveOptions()
                {
                    Title = Resources.SaveFileTitle,
                    ShowOverwritePrompt = true,
                };

                var result = await wnd.MainWindow.StorageProvider.SaveFilePickerAsync(options);

                return result?.Path?.LocalPath;
            }

            return null;
        }

        static List<TextValuePair<string>> GetScreens()
        {
            var list = new List<TextValuePair<string>>();

            if (Design.IsDesignMode)
            {
                list.Add(new() { Text = "Display 1 (#0)", Value = "ABC123" });
                list.Add(new() { Text = "Display 2 (#1)", Value = "ABC456" });

                return list;
            }

            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime wnd
             && wnd.MainWindow != null)
            {
                var screens = wnd.MainWindow.Screens.All.ToList();

                for (int i = 0; i < screens.Count; ++i)
                {
                    list.Add(new()
                    {
                        Text = $"{screens[i].DisplayName} (#{i})" ?? $"#{i}",
                        Value = screens[i].GetSerialNumber()
                    });
                }
            }

            return list;
        }

        static void CheckForUpdate()
        {
            Client.CheckForUpdates();
        }

        static void ShowDriveDetails(IHardware hardware)
        {
            if (hardware is StorageDevice sd)
            {
                if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime wnd
                 && wnd.MainWindow != null)
                {
                    new Windows.StorageWindow(sd).ShowDialog(wnd.MainWindow);
                }
                else
                {
                    new Windows.StorageWindow(sd).Show();
                }
            }
        }

        static void ShowRAMDetails(IHardware hardware)
        {
            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime wnd
             && wnd.MainWindow != null)
            {
                new Windows.RAMWindow(hardware).ShowDialog(wnd.MainWindow);
            }
            else
            {
                new Windows.RAMWindow(hardware).Show();
            }
        }

        #endregion
    }
}
