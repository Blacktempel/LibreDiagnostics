/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.Core.Collections;
using BlackSharp.Core.Interfaces;
using BlackSharp.Core.Reflection;
using BlackSharp.MVVM.ComponentModel;
using LibreDiagnostics.Language;
using LibreDiagnostics.Models.Enums;
using LibreDiagnostics.Models.Globals;
using LibreDiagnostics.Models.Platform;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LibreDiagnostics.Models.Configuration
{
    public sealed class Settings : ViewModelBase, ICopyable<Settings>, ICloneable<Settings>
    {
        #region Properties

        #region Not visible

        bool _InitialStart = true;
        [JsonProperty]
        public bool InitialStart
        {
            get { return _InitialStart; }
            set { SetField(ref _InitialStart, value); }
        }

        DateTime? _NextUpdateReminder;
        public DateTime? NextUpdateReminder
        {
            get { return _NextUpdateReminder; }
            set { SetField(ref _NextUpdateReminder, value); }
        }

        #endregion

        #region General

        DockingPosition _DockingPosition = DockingPosition.Right;
        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public DockingPosition DockingPosition
        {
            get { return _DockingPosition; }
            set { SetField(ref _DockingPosition, value); }
        }

        ScreenModel _ScreenInfo;
        [JsonProperty]
        public ScreenModel ScreenInfo
        {
            get { return _ScreenInfo; }
            set { SetField(ref _ScreenInfo, value); }
        }

        string _Language = Culture.DEFAULT;
        [JsonProperty]
        public string Language
        {
            get { return _Language; }
            set { SetField(ref _Language, value); }
        }

        TextAlignment _TextAlignment = TextAlignment.Left;
        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public TextAlignment TextAlignment
        {
            get { return _TextAlignment; }
            set { SetField(ref _TextAlignment, value); }
        }

        bool _IsAppBar = true;
        [JsonProperty]
        public bool IsAppBar
        {
            get { return _IsAppBar; }
            set { SetField(ref _IsAppBar, value); }
        }

        bool _AlwaysOnTop = false;
        [JsonProperty]
        public bool AlwaysOnTop
        {
            get { return _AlwaysOnTop; }
            set { SetField(ref _AlwaysOnTop, value); }
        }

        bool _StartWithSystem = false;
        [JsonProperty]
        public bool StartWithSystem
        {
            get { return _StartWithSystem; }
            set { SetField(ref _StartWithSystem, value); }
        }

        bool _AutoUpdate = true;
        [JsonProperty]
        public bool AutoUpdate
        {
            get { return _AutoUpdate; }
            set { SetField(ref _AutoUpdate, value); }
        }

        #endregion

        #region Advanced

        int _AppWidth = 220;
        [JsonProperty]
        public int AppWidth
        {
            get { return _AppWidth; }
            set { SetField(ref _AppWidth, value); }
        }

        double _FontSize = 11;
        [JsonProperty]
        public double FontSize
        {
            get { return _FontSize; }
            set { SetField(ref _FontSize, value); }
        }

        string _FontFamily;
        [JsonProperty]
        public string FontFamily
        {
            get { return _FontFamily; }
            set { SetField(ref _FontFamily, value); }
        }

        int _HorizontalOffset = 0;
        [JsonProperty]
        public int HorizontalOffset
        {
            get { return _HorizontalOffset; }
            set { SetField(ref _HorizontalOffset, value); }
        }

        int _VerticalOffset = 0;
        [JsonProperty]
        public int VerticalOffset
        {
            get { return _VerticalOffset; }
            set { SetField(ref _VerticalOffset, value); }
        }

        int _UpdateInterval = 1000;
        [JsonProperty]
        public int UpdateInterval
        {
            get { return _UpdateInterval; }
            set { SetField(ref _UpdateInterval, value); }
        }

        bool _ShowTrayIcon = true;
        [JsonProperty]
        public bool ShowTrayIcon
        {
            get { return _ShowTrayIcon; }
            set { SetField(ref _ShowTrayIcon, value); }
        }

        bool _IsInAltTab = false;
        [JsonProperty]
        public bool IsInAltTab
        {
            get { return _IsInAltTab; }
            set { SetField(ref _IsInAltTab, value); }
        }

        bool _ClickThrough = false;
        [JsonProperty]
        public bool ClickThrough
        {
            get { return _ClickThrough; }
            set { SetField(ref _ClickThrough, value); }
        }

        bool _ShowMenuBar = true;
        [JsonProperty]
        public bool ShowMenuBar
        {
            get { return _ShowMenuBar; }
            set { SetField(ref _ShowMenuBar, value); }
        }

        bool _ShowClock = false;
        [JsonProperty]
        public bool ShowClock
        {
            get { return _ShowClock; }
            set { SetField(ref _ShowClock, value); }
        }

        bool _UseHardwareAcceleration = true;
        [JsonProperty]
        public bool UseHardwareAcceleration
        {
            get { return _UseHardwareAcceleration; }
            set { SetField(ref _UseHardwareAcceleration, value); }
        }

        #endregion

        #region Customize

        bool _AutoBackgroundColor = true;
        [JsonProperty]
        public bool AutoBackgroundColor
        {
            get { return _AutoBackgroundColor; }
            set { SetField(ref _AutoBackgroundColor, value); }
        }

        string _BackgroundColor = "#00000000";
        [JsonProperty]
        public string BackgroundColor
        {
            get { return _BackgroundColor; }
            set { SetField(ref _BackgroundColor, value); }
        }

        double _BackgroundOpacity = 0.6;
        [JsonProperty]
        public double BackgroundOpacity
        {
            get { return _BackgroundOpacity; }
            set { SetField(ref _BackgroundOpacity, value); }
        }

        bool _AutoFontColor = true;
        [JsonProperty]
        public bool AutoFontColor
        {
            get { return _AutoFontColor; }
            set { SetField(ref _AutoFontColor, value); }
        }

        string _FontColor = "#FFFFFF";
        [JsonProperty]
        public string FontColor
        {
            get { return _FontColor; }
            set { SetField(ref _FontColor, value); }
        }

        string _AlertFontColor = "#FF4136";
        [JsonProperty]
        public string AlertFontColor
        {
            get { return _AlertFontColor; }
            set { SetField(ref _AlertFontColor, value); }
        }

        #endregion

        #region Monitors

        ObservableCollectionEx<HardwareMonitorConfig> _HardwareMonitorConfigs = new();
        [JsonProperty]
        public ObservableCollectionEx<HardwareMonitorConfig> HardwareMonitorConfigs
        {
            get { return _HardwareMonitorConfigs; }
            set { SetField(ref _HardwareMonitorConfigs, value); }
        }

        #endregion

        #region Hotkeys

        HotKey _HotKeyCloseApp = new(HotKeyID.CloseApp);
        [JsonProperty]
        public HotKey HotKeyCloseApp
        {
            get { return _HotKeyCloseApp; }
            set { SetField(ref _HotKeyCloseApp, value); }
        }

        HotKey _HotKeyRestartApp = new(HotKeyID.RestartApp);
        [JsonProperty]
        public HotKey HotKeyRestartApp
        {
            get { return _HotKeyRestartApp; }
            set { SetField(ref _HotKeyRestartApp, value); }
        }

        HotKey _HotKeyToggleAppBar = new(HotKeyID.ToggleAppBar);
        [JsonProperty]
        public HotKey HotKeyToggleAppBar
        {
            get { return _HotKeyToggleAppBar; }
            set { SetField(ref _HotKeyToggleAppBar, value); }
        }

        HotKey _HotKeyOpenSettings = new(HotKeyID.OpenSettings);
        [JsonProperty]
        public HotKey HotKeyOpenSettings
        {
            get { return _HotKeyOpenSettings; }
            set { SetField(ref _HotKeyOpenSettings, value); }
        }

        #endregion

        #endregion

        #region ICloneable

        public Settings Clone()
        {
            var clone = (Settings)MemberwiseClone();

            clone.ScreenInfo = ScreenInfo?.Clone();

            clone.HardwareMonitorConfigs = new(HardwareMonitorConfigs.Select(hm => hm.Clone()));

            return clone;
        }

        #endregion

        #region ICopyable

        public void Copy(Settings from)
        {
            ShallowCopy.CopyValueTypeProperties(from, this);

            if (ScreenInfo == null && from.ScreenInfo != null)
            {
                ScreenInfo = new();
            }

            if (from.ScreenInfo != null && ScreenInfo != null)
            {
                ShallowCopy.CopyValueTypeProperties(from.ScreenInfo, ScreenInfo);
            }

            //Find differences in HardwareMonitorConfigs first
            from.HardwareMonitorConfigs
                .Where(hmc => !HardwareMonitorConfigs
                    .Any(existing => existing.HardwareMonitorType == hmc.HardwareMonitorType))
                .ToList()
                .ForEach(HardwareMonitorConfigs.Add);

            HardwareMonitorConfigs
                .Where(hmc => !from.HardwareMonitorConfigs
                    .Any(existing => existing.HardwareMonitorType == hmc.HardwareMonitorType))
                .ToList()
                .ForEach(hmc => HardwareMonitorConfigs.Remove(hmc));

            //Reorder items if necessary
            if (from.HardwareMonitorConfigs.Count == HardwareMonitorConfigs.Count)
            {
                for (int i = 0; i < from.HardwareMonitorConfigs.Count; ++i)
                {
                    var hw = from.HardwareMonitorConfigs[i];

                    var index = HardwareMonitorConfigs.FindIndex(hc => hc.HardwareMonitorType == hw.HardwareMonitorType);
                    if (index >= 0 && i != index)
                    {
                        HardwareMonitorConfigs.Move(index, i);
                    }
                }
            }

            //Copy list
            for (int i = 0; i < HardwareMonitorConfigs.Count; ++i)
            {
                var existing = HardwareMonitorConfigs[i];
                var source = from.HardwareMonitorConfigs
                                    .First(hmc => hmc.HardwareMonitorType == existing.HardwareMonitorType);
                existing.Copy(source);
            }
        }

        #endregion

        #region Public

        public static Settings Reload()
        {
            if (File.Exists(Paths.SettingsFile))
            {
                using (var reader = File.OpenText(Paths.SettingsFile))
                {
                    return new JsonSerializer().Deserialize(reader, typeof(Settings)) as Settings;
                }
            }

            return new Settings();
        }

        public void Save(bool applyHardwareChanges = true)
        {
            if (applyHardwareChanges)
            {
                for (int i = 0; i < HardwareMonitorConfigs.Count; ++i)
                {
                    var config = HardwareMonitorConfigs[i];
                    for (int j = 0; j < config.HardwareOC.Count; ++j)
                    {
                        var hardware = config.HardwareOC[j];
                        hardware.Order = (byte)j;
                    }

                    config.HardwareConfig = new(config.HardwareOC);

                    for (int j = 0; j < config.HardwareConfig.Count; ++j)
                    {
                        var metric = config.HardwareConfig[j];
                        metric.Order = (byte)j;
                    }

                    config.Order = (byte)i;
                }
            }

            if (!Directory.Exists(Paths.AppDataLocal))
            {
                Directory.CreateDirectory(Paths.AppDataLocal);
            }

            using (var writer = File.CreateText(Paths.SettingsFile))
            {
                new JsonSerializer() { Formatting = Formatting.Indented }.Serialize(writer, this);
            }
        }

        public bool IsMonitorEnabled(HardwareMonitorType hardwareMonitorType)
        {
            var monitor = HardwareMonitorConfigs.Find(hmc => hmc.HardwareMonitorType == hardwareMonitorType);

            return monitor?.Enabled == true;
        }

        public bool IsConfigEnabled(HardwareMonitorType hardwareMonitorType, HardwareMetricKey key)
        {
            var cfg = HardwareMonitorConfigs.Find(hmc => hmc.HardwareMonitorType == hardwareMonitorType);

            if (cfg == null)
            {
                return false;
            }

            var found = cfg.MetricConfig.Find(hc => hc.HardwareMetricKey == key);

            return found != null && found.Enabled;
        }

        public T GetHardwareConfigOptionValue<T>(HardwareMonitorType hardwareMonitorType, HardwareConfigOption hardwareConfigOption)
        {
            var cfg = HardwareMonitorConfigs.Find(hmc => hmc.HardwareMonitorType == hardwareMonitorType);

            if (cfg == null)
            {
                return default;
            }

            var found = cfg.HardwareConfigOptions.Find(hco => hco.Key == hardwareConfigOption);

            if (found != null)
            {
                return (T)ConvertValue(found);
            }
            else
            {
                return default;
            }
        }

        #endregion

        #region Private

        object ConvertValue(HardwareConfigOptions hco)
        {
            if (string.IsNullOrEmpty(hco.Value?.ToString()))
            {
                return GetDefault(hco.ValueKind);
            }

            switch (hco.ValueKind)
            {
                case HardwareConfigOptionValueKind.Int8:
                    return Convert.ToByte(hco.Value);
                case HardwareConfigOptionValueKind.Int16:
                    return Convert.ToInt16(hco.Value);
                case HardwareConfigOptionValueKind.Int32:
                    return Convert.ToInt32(hco.Value);
                case HardwareConfigOptionValueKind.Int64:
                    return Convert.ToInt64(hco.Value);
                case HardwareConfigOptionValueKind.Double:
                    return Convert.ToDouble(hco.Value);
                case HardwareConfigOptionValueKind.Float:
                    return Convert.ToSingle(hco.Value);
                case HardwareConfigOptionValueKind.Decimal:
                    return Convert.ToDecimal(hco.Value);
                case HardwareConfigOptionValueKind.None:
                case HardwareConfigOptionValueKind.Boolean:
                case HardwareConfigOptionValueKind.String:
                default:
                    return hco.Value;
            }
        }

        object GetDefault(HardwareConfigOptionValueKind valueKind)
        {
            switch (valueKind)
            {
                case HardwareConfigOptionValueKind.Int8:
                    return Convert.ToByte(0);
                case HardwareConfigOptionValueKind.Int16:
                    return Convert.ToInt16(0);
                case HardwareConfigOptionValueKind.Int32:
                    return Convert.ToInt32(0);
                case HardwareConfigOptionValueKind.Int64:
                    return Convert.ToInt64(0);
                case HardwareConfigOptionValueKind.Double:
                    return Convert.ToDouble(0);
                case HardwareConfigOptionValueKind.Float:
                    return Convert.ToSingle(0);
                case HardwareConfigOptionValueKind.Decimal:
                    return Convert.ToDecimal(0);
                case HardwareConfigOptionValueKind.None:
                case HardwareConfigOptionValueKind.Boolean:
                    return false;
                case HardwareConfigOptionValueKind.String:
                default:
                    return string.Empty;
            }
        }

        #endregion

        #region Public - Mock Data

        public static Settings GetMock()
        {
            //if (!IsInDesignMode)
            //    return null;

            var settings = new Settings
            {
                HardwareMonitorConfigs = new(HardwareMonitorConfig.GetMock()),
            };

            settings.HardwareMonitorConfigs.ForEach(hmc => hmc.HardwareConfigOptions = new(HardwareConfigOptions.GetMock()));

            return settings;
        }

        #endregion
    }
}
