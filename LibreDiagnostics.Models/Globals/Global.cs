/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using DiskInfoToolkit;
using LibreDiagnostics.Language;
using LibreDiagnostics.Models.Configuration;
using LibreDiagnostics.Models.Hardware;
using LibreDiagnostics.Models.Interfaces;
using LibreDiagnostics.Models.Platform;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LibreDiagnostics.Models.Globals
{
    public static class Global
    {
        #region Constructor

        static Global()
        {
            SettingsChanged += OnSettingsChanged;
        }

        #endregion

        #region Properties

        static bool _IsReady;
        public static bool IsReady
        {
            get { return _IsReady; }
            set { SetField(ref _IsReady, value); }
        }

        static Settings _Settings;
        public static Settings Settings
        {
            get { return _Settings; }
            set
            {
                var temp = _Settings;
                SetField(ref _Settings, value);

                if (!ReferenceEquals(temp, _Settings))
                {
                    SettingsChanged?.Invoke(null, new(_Settings));
                }
            }
        }

        static IHardwareMonitorManager _HardwareMonitorManager;
        public static IHardwareMonitorManager HardwareMonitorManager
        {
            get { return _HardwareMonitorManager; }
            set { SetField(ref _HardwareMonitorManager, value); }
        }

        static HardwareManager _HardwareManager;
        public static HardwareManager HardwareManager
        {
            get { return _HardwareManager; }
            set { SetField(ref _HardwareManager, value); }
        }

        static IFontManager _FontManager;
        public static IFontManager FontManager
        {
            get { return _FontManager; }
            set { SetField(ref _FontManager, value); }
        }

        static ITrayIcon _TrayIcon;
        public static ITrayIcon TrayIcon
        {
            get { return _TrayIcon; }
            set { SetField(ref _TrayIcon, value); }
        }

        static ProcessInformation _ProcessInformation = new();
        public static ProcessInformation ProcessInformation
        {
            get { return _ProcessInformation; }
            set { SetField(ref _ProcessInformation, value); }
        }

        #endregion

        #region Public

        public static void CopySettingsFrom(Settings settings)
        {
            var oldSettings = Settings.Clone();
            Settings.Copy(settings);

            SettingsChanged?.Invoke(null, new(oldSettings, Settings));
        }

        #endregion

        #region Private

        static void OnSettingsChanged(object sender, SettingsChangedEventArgs e)
        {
            if (e.NewSettings == null)
            {
                return;
            }

            Culture.SetCurrent(e.NewSettings.Language);
            Storage.ResourceCulture = Culture.GetCurrent();

            //FontManager may have not been initialized yet
            FontManager?.GlobalFontSize   = e.NewSettings.FontSize  ;
            FontManager?.GlobalFontFamily = e.NewSettings.FontFamily;
        }

        #endregion

        #region Events

        public static event EventHandler<SettingsChangedEventArgs> SettingsChanged;

        #endregion

        #region StaticNotifyPropertyChanged

        public static bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            NotifyStaticPropertyChanged(propertyName);
            return true;
        }

        public static void NotifyStaticPropertyChanged(string propertyName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        public static void NotifyAllStaticPropertyChanged()
        {
            NotifyStaticPropertyChanged(string.Empty);
        }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        #endregion
    }
}
