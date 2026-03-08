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
using Avalonia.Interactivity;
using Avalonia.Platform;
using BlackSharp.Core.Logging;
using BlackSharp.UI.Avalonia.Media;
using BlackSharp.UI.Avalonia.Platform.Interfaces;
using BlackSharp.UI.Avalonia.Platform.Windows;
using CommunityToolkit.Mvvm.Input;
using LibreDiagnostics.Models.Configuration;
using LibreDiagnostics.Models.Enums;
using LibreDiagnostics.Models.Globals;
using LibreDiagnostics.MVVM.Utilities;
using LibreDiagnostics.MVVM.ViewModels;
using LibreDiagnostics.UI.Platform;
using LibreDiagnostics.UI.Platform.Windows;
using System.Windows.Input;

using OS = BlackSharp.Core.Platform.OperatingSystem;

namespace LibreDiagnostics.UI.Windows
{
    public partial class MainWindow : FlatWindow
    {
        #region Constructor

        public MainWindow()
        {
            if (!Design.IsDesignMode)
            {
                DataContext = new MainWindowViewModel();
            }

            InitializeComponent();

            if (!Design.IsDesignMode)
            {
                Loaded += OnWindowLoaded;

                //LayoutUpdated += OnLayoutUpdated;

                Global.SettingsChanged += ApplySettings;

                Closing += OnWindowClosing;
            }

            Application.Current.PlatformSettings.ColorValuesChanged += (s, e) =>
            {
                SetAutoColors(Global.Settings);
            };
        }

        #endregion

        #region Fields

        AppBarTask _AppBarTask;
        IHotKeyManager<HotKeyID> _HotKeyManager;

        #endregion

        #region Properties

        protected override Type StyleKeyOverride => typeof(FlatWindow);

        #endregion

        #region Private

        void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            //Wait for main window to be loaded before opening settings
            if (Global.Settings.InitialStart)
            {
                Global.Settings.InitialStart = false;

                MessageBro.DoOpenSettings();
            }

            if (OS.IsWindows() && !Design.IsDesignMode)
            {
                _HotKeyManager = new Win32HotKeyManager<HotKeyID>();
                _HotKeyManager.EnableHotKeyHandling(this);
            }

            ApplySettings(this, new(Global.Settings));
        }

        void OnWindowClosing(object sender, WindowClosingEventArgs e)
        {
            if (_AppBarTask != null)
            {
                _HotKeyManager.ClearHotKeys();
                _HotKeyManager.DisableHotKeyHandling(this);

                _AppBarTask.DisableAppBar();
            }
        }

        //void OnLayoutUpdated(object sender, EventArgs e)
        //{
        //    //Only update if not AppBar (AppBar handles its own positioning)
        //    if (_AppBarTask == null)
        //    {
        //        SetWindowPosition();
        //    }
        //}

        void SetWindowPosition()
        {
            var settings = Global.Settings;
            var screens = Screens.All;

            Screen active = null;

            //No screens
            if (screens.Count == 0)
            {
                Logger.Instance.Add(LogLevel.Warn, "No screens were detected.", DateTime.Now);
                return;
            }

            var defaultScreen = screens.FirstOrDefault();

            //Screen invalid
            if (settings.ScreenInfo == null)
            {
                active = defaultScreen;
            }
            else
            {
                active = screens.FirstOrDefault(s => ScreenManager.GetScreenID(s, settings.ScreenInfo.Strategy) == settings.ScreenInfo.ScreenID, defaultScreen);
            }

            var bounds = active.WorkingArea;
            var scaling = RenderScaling;

            double screenX      = bounds.X      == 0 ? 0 : bounds.X      / scaling;
            double screenY      = bounds.Y      == 0 ? 0 : bounds.Y      / scaling;
            double screenWidth  = bounds.Width  == 0 ? 0 : bounds.Width  / scaling;
            double screenHeight = bounds.Height == 0 ? 0 : bounds.Height / scaling;

            var horizontalOffset = settings.HorizontalOffset == 0 ? 0 : settings.HorizontalOffset / scaling;
            var verticalOffset   = settings.VerticalOffset   == 0 ? 0 : settings.VerticalOffset   / scaling;

            double x = screenX;
            double y = screenY + verticalOffset;

            var width = settings.AppWidth == 0 ? 0 : settings.AppWidth / scaling;

            switch (settings.DockingPosition)
            {
                case DockingPosition.Left:
                    x += horizontalOffset;
                    break;
                case DockingPosition.Right:
                    x += screenWidth - width - horizontalOffset;
                    break;
            }

            Width = width;
            MaxWidth = Width;
            MaxHeight = screenHeight;

            double desiredHeight = 0;

            if (Content is Control ctrl)
            {
                desiredHeight = ctrl.Bounds.Height;

                if (desiredHeight <= 0)
                {
                    desiredHeight = ctrl.DesiredSize.Height;
                }
            }

            double height = Math.Min(desiredHeight, MaxHeight);
            Height = height;

            Position = new((int)(x * scaling), (int)(y * scaling));
        }

        void ApplySettings(object sender, SettingsChangedEventArgs e)
        {
            var settings = Global.Settings;

            SetWindowPosition();

            WindowStyleHandler.SetClickThrough(this, settings.ClickThrough);
            WindowStyleHandler.SetIsInAltTab  (this, settings.IsInAltTab  );
            //WindowStyleHandler.SetTopMost     (this, settings.AlwaysOnTop );
            Topmost = settings.AlwaysOnTop;

            ProcessAppBarRelevantChanges(e);

            SetAutoColors(settings);

            //Only update if not AppBar (AppBar handles its own positioning)
            if (_AppBarTask == null)
            {
                SetWindowPosition();
            }

            UpdateHotKeys(settings);
        }

        void ProcessAppBarRelevantChanges(SettingsChangedEventArgs e)
        {
            if (OS.IsLinux())
            {
                return; //AppBar is not supported on Linux, so ignore this setting
            }

            if (e.OldSettings != null)
            {
                //Check if any AppBar relevant settings have changed
                if (e.OldSettings.IsAppBar              != e.NewSettings.IsAppBar
                 || e.OldSettings.DockingPosition       != e.NewSettings.DockingPosition
                 || 
                    (
                        e.OldSettings.ScreenInfo?.Strategy != e.NewSettings.ScreenInfo.Strategy
                     || e.OldSettings.ScreenInfo?.ScreenID != e.NewSettings.ScreenInfo.ScreenID
                    )
                 || e.OldSettings.AppWidth              != e.NewSettings.AppWidth
                 || e.OldSettings.HorizontalOffset      != e.NewSettings.HorizontalOffset
                 || e.OldSettings.VerticalOffset        != e.NewSettings.VerticalOffset
                 || e.OldSettings.VerticalContentMargin != e.NewSettings.VerticalContentMargin
                    )
                {
                    //Disable AppBar
                    if (_AppBarTask != null)
                    {
                        _AppBarTask.DisableAppBar();
                        _AppBarTask = null;
                    }

                    //Update position
                    SetWindowPosition();

                    //Enable AppBar, if setting says so
                    if (e.NewSettings.IsAppBar)
                    {
                        _AppBarTask = new AppBarTask(this);
                        _AppBarTask.EnableAppBar();
                    }
                }
            }
            else //First time applying settings
            {
                if (e.NewSettings.IsAppBar)
                {
                    _AppBarTask = new AppBarTask(this);
                    _AppBarTask.EnableAppBar();
                }
                else
                {
                    if (_AppBarTask != null)
                    {
                        _AppBarTask.DisableAppBar();
                        _AppBarTask = null;
                    }
                }
            }
        }

        void SetAutoColors(Settings settings)
        {
            if (settings.AutoFontColor)
            {
                var b = SystemColors.GetSystemForeground();
                if (b != null)
                {
                    settings.FontColor = b.Color.ToString();
                }
            }

            if (settings.AutoBackgroundColor)
            {
                var b = SystemColors.GetSystemBackground();
                if (b != null)
                {
                    settings.BackgroundColor = b.Color.ToString();
                }
            }
        }

        void UpdateHotKeys(Settings settings)
        {
            if (_HotKeyManager == null)
            {
                return;
            }

            //Close app
            UpdateHotKey(settings.HotKeyCloseApp, Global.TrayIcon.CloseRequestedCommand);

            //Restart app
            UpdateHotKey(settings.HotKeyRestartApp, Global.TrayIcon.RestartRequestedCommand);

            //Toggle AppBar
            UpdateHotKey(settings.HotKeyToggleAppBar, new RelayCommand(() =>
            {
                var old = Global.Settings.Clone();

                Global.Settings.IsAppBar = !Global.Settings.IsAppBar;
                Global.Settings.Save();

                ProcessAppBarRelevantChanges(new(old, Global.Settings));
            }));

            //Open settings
            UpdateHotKey(settings.HotKeyOpenSettings, new RelayCommand(MessageBro.DoOpenSettings));
        }

        void UpdateHotKey(HotKey hotKey, ICommand command)
        {
            if (hotKey.IsValid)
            {
                _HotKeyManager.RegisterHotKey(hotKey, command);
            }
            else
            {
                _HotKeyManager.UnregisterHotKey(hotKey);
            }
        }

        #endregion
    }
}
