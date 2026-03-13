/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.MVVM.ComponentModel;
using BlackSharp.MVVM.Dialogs.Enums;
using CommunityToolkit.Mvvm.Input;
using LibreDiagnostics.Language;
using LibreDiagnostics.Language.Resources;
using LibreDiagnostics.Models.Configuration;
using LibreDiagnostics.Models.Enums;
using LibreDiagnostics.Models.Globals;
using LibreDiagnostics.Models.Helper;
using LibreDiagnostics.Models.Platform;
using LibreDiagnostics.MVVM.Utilities;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

namespace LibreDiagnostics.MVVM.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        #region Constructor

        protected SettingsViewModel(object o)
        {
            //Design time
            CommonInit();
        }

        public SettingsViewModel()
        {
            //Run time

            CommonInit();

            HardwareMonitorConfig.SyncHardwareConfigsWithDetectedHardware(Settings);
        }

        #endregion

        #region Properties

        #region Constant lists and dynamic selections

        public List<TextValuePair<DockingPosition>> DockingPositionList { get; private set; }

        TextValuePair<DockingPosition> _DockingPositionSelected;
        public TextValuePair<DockingPosition> DockingPositionSelected
        {
            get { return _DockingPositionSelected; }
            set { SetField(ref _DockingPositionSelected, value); Settings.DockingPosition = _DockingPositionSelected.Value; }
        }

        public List<ScreenModel> ScreenList { get; private set; }

        ScreenModel _ScreenSelected;
        public ScreenModel ScreenSelected
        {
            get { return _ScreenSelected; }
            set { SetField(ref _ScreenSelected, value); Settings.ScreenInfo = _ScreenSelected; }
        }

        public List<TextValuePair<TextAlignment>> TextAlignmentList { get; private set; }

        TextValuePair<TextAlignment> _TextAlignmentSelected;
        public TextValuePair<TextAlignment> TextAlignmentSelected
        {
            get { return _TextAlignmentSelected; }
            set { SetField(ref _TextAlignmentSelected, value); Settings.TextAlignment = _TextAlignmentSelected.Value; }
        }

        public List<CultureItem> LanguageList { get; set; }

        CultureItem _LanguageSelected;
        public CultureItem LanguageSelected
        {
            get { return _LanguageSelected; }
            set { SetField(ref _LanguageSelected, value); Settings.Language = _LanguageSelected.Value; }
        }

        public List<string> FontFamilyList { get; set; }

        string _FontFamilySelected;
        public string FontFamilySelected
        {
            get { return _FontFamilySelected; }
            set { SetField(ref _FontFamilySelected, value); Settings.FontFamily = _FontFamilySelected; }
        }

        DateTimeFormat _DateFormatSelected;
        public DateTimeFormat DateFormatSelected
        {
            get { return _DateFormatSelected; }
            set
            {
                SetField(ref _DateFormatSelected, value);
                Settings.DateFormat = _DateFormatSelected.Format;
            }
        }

        List<DateTimeFormat> _DateFormatList;
        public List<DateTimeFormat> DateFormatList
        {
            get { return _DateFormatList; }
            set { SetField(ref _DateFormatList, value); }
        }

        DateTimeFormat _TimeFormatSelected;
        public DateTimeFormat TimeFormatSelected
        {
            get { return _TimeFormatSelected; }
            set
            {
                SetField(ref _TimeFormatSelected, value);
                Settings.TimeFormat = _TimeFormatSelected.Format;
            }
        }

        List<DateTimeFormat> _TimeFormatList;
        public List<DateTimeFormat> TimeFormatList
        {
            get { return _TimeFormatList; }
            set { SetField(ref _TimeFormatList, value); }
        }

        #endregion

        Settings _Settings;
        public Settings Settings
        {
            get { return _Settings; }
            set { SetField(ref _Settings, value); }
        }

        bool _IsChanged;
        public bool IsChanged
        {
            get { return _IsChanged; }
            set { SetField(ref _IsChanged, value); }
        }

        IRelayCommand _CloseCommand;
        public IRelayCommand CloseCommand
        {
            get { return _CloseCommand; }
            set { SetField(ref _CloseCommand, value); }
        }

        #endregion

        #region Private

        void CommonInit()
        {
            //Clone settings to not make any changes directly
            Settings = Global.Settings.Clone();

            Settings.PropertyChanged += OnSettingsPropertyChanged;
            Settings.HardwareMonitorConfigs.CollectionChanged += OnSettingsCollectionChanged;

            InitializeConstantLists();
        }

        void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IsChanged))
            {
                IsChanged = true;
            }

            //Unsubscribe & subscribe after changes again, as this could cause stackoverflow exception
            Settings.PropertyChanged -= OnSettingsPropertyChanged;

            switch (e.PropertyName)
            {
                case nameof(Settings.HorizontalOffset):
                    if (Settings.HorizontalOffset != 0)
                    {
                        Settings.ShowTrayIcon = true;
                    }
                    break;
                case nameof(Settings.VerticalOffset):
                    if (Settings.VerticalOffset != 0)
                    {
                        Settings.ShowTrayIcon = true;
                    }
                    break;
                case nameof(Settings.ShowTrayIcon):
                    if (!Settings.ShowTrayIcon)
                    {
                        Settings.HorizontalOffset = 0;
                        Settings.VerticalOffset = 0;
                        Settings.ClickThrough = false;
                    }
                    break;
                case nameof(Settings.ClickThrough):
                    if (Settings.ClickThrough)
                    {
                        Settings.ShowTrayIcon = true;
                    }
                    break;
                default:
                    break;
            }

            Settings.PropertyChanged += OnSettingsPropertyChanged;
        }

        void OnSettingsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IsChanged = true;

            foreach (var item in Settings.HardwareMonitorConfigs)
            {
                item.PropertyChanged += OnChildPropertyChanged;

                item.HardwareOC.CollectionChanged += OnChildCollectionChanged;

                //item.HardwareConfig       .ForEach(x => x.PropertyChanged += OnChildPropertyChanged);
                item.HardwareOC           .ForEach(x => x.PropertyChanged += OnChildPropertyChanged);
                item.MetricConfig         .ForEach(x => x.PropertyChanged += OnChildPropertyChanged);
                item.HardwareConfigOptions.ForEach(x => x.PropertyChanged += OnChildPropertyChanged);
            }
        }

        void OnChildCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IsChanged = true;
        }

        void OnChildPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IsChanged = true;
        }

        void InitializeConstantLists()
        {
            DockingPositionList = new()
            {
                new TextValuePair<DockingPosition>() { Text = Resources.SettingsDockLeft , Value = DockingPosition.Left  },
                new TextValuePair<DockingPosition>() { Text = Resources.SettingsDockRight, Value = DockingPosition.Right },
            };

            DockingPositionSelected = DockingPositionList.FirstOrDefault(tvp => tvp.Value == Settings.DockingPosition);

            ScreenList = MessageBro.DoGetScreens();

            var screen = ScreenList.FirstOrDefault(tvp => tvp.ScreenID == Settings.ScreenInfo?.ScreenID, ScreenList.FirstOrDefault());

            ScreenSelected = screen;

            TextAlignmentList = new List<TextValuePair<TextAlignment>>
            {
                new TextValuePair<TextAlignment>() { Text = Resources.SettingsTextAlignmentLeft , Value = TextAlignment.Left  },
                new TextValuePair<TextAlignment>() { Text = Resources.SettingsTextAlignmentRight, Value = TextAlignment.Right },
            };

            TextAlignmentSelected = TextAlignmentList.FirstOrDefault(tvp => tvp.Value == Settings.TextAlignment);

            LanguageList = Culture.GetAll();
            LanguageSelected = LanguageList.FirstOrDefault(ci => ci.Value == Settings.Language);

            FontFamilyList = Global.FontManager.GetSystemFontFamilies();
            FontFamilySelected = Settings.FontFamily ?? Global.FontManager.GlobalFontFamily;

            var sampleDateTime = new DateTime(2026, 12, 31, 23, 59, 59);

            DateFormatList = MessageBro.DoGetDateFormats(sampleDateTime);
            DateFormatSelected = DateFormatList.FirstOrDefault(s => s.Format == Settings.DateFormat) ?? DateTimeFormat.GetDefaultDateFormat(sampleDateTime);

            TimeFormatList = MessageBro.DoGetTimeFormats(sampleDateTime);
            TimeFormatSelected = TimeFormatList.FirstOrDefault(s => s.Format == Settings.TimeFormat) ?? DateTimeFormat.GetDefaultTimeFormat(sampleDateTime);
        }

        #endregion

        #region Commands

        [RelayCommand]
        void Save()
        {
            ApplySettings();
            CloseCommand?.Execute(null);
        }

        [RelayCommand]
        void Apply()
        {
            ApplySettings();
        }

        void ApplySettings()
        {
            Settings.Save();

            //Language change requires application restart
            if (!string.Equals(Settings.Language, Global.Settings.Language, StringComparison.Ordinal))
            {
                MessageBro.DoShowMessageTimeout(Resources.LanguageChangedTitle, Resources.LanguageChangedText, DialogButtons.OK, TimeSpan.FromSeconds(5), out _);

                Process.Start(Environment.ProcessPath);
                MessageBro.DoShutdownApplication();
                return;
            }

            //Copy settings to avoid rendering bindings useless (no cloning)
            Global.CopySettingsFrom(Settings);
        }

        #endregion
    }

    public class MockSettingsViewModel : SettingsViewModel
    {
        public MockSettingsViewModel() : base(null) { }
    }
}
