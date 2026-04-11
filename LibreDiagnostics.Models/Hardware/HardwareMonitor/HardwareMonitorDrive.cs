/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

#pragma warning disable CA1416 // Platform compatibility warning

using BlackSharp.Core.Collections;
using BlackSharp.Core.Converters.Enums;
using BlackSharp.Core.Extensions;
using LibreDiagnostics.Models.Configuration;
using LibreDiagnostics.Models.Enums;
using LibreDiagnostics.Models.Events;
using LibreDiagnostics.Models.Globals;
using LibreDiagnostics.Models.Hardware.Metrics;
using LibreDiagnostics.Models.Interfaces;
using LibreHardwareMonitor.Hardware;

using StorageDeviceDIT = DiskInfoToolkit.StorageDevice;
using StorageDeviceLHM = LibreHardwareMonitor.Hardware.Storage.StorageDevice;

namespace LibreDiagnostics.Models.Hardware.HardwareMonitor
{
    public class HardwareMonitorDrive : HardwareMonitorBoardItem
    {
        #region Constructor

        public HardwareMonitorDrive(IHardware hardware, HardwareConfig config)
            : base(hardware, config)
        {
            var sd = Hardware as StorageDeviceLHM;
            _Storage = sd.Storage;

            Initialize();

            Global.SettingsChanged += OnSettingsChanged;

            //Initial call to apply settings
            OnSettingsChanged(this, new(Global.Settings));
        }

        #endregion

        #region Fields

        readonly StorageDeviceDIT _Storage;

        ISensor _FreeSize;
        ISensor _TotalSize;

        #endregion

        #region Properties

        bool ShouldLoadBarBeEnabled => LoadMetric != null;

        MetricBase _LoadMetric;
        public MetricBase LoadMetric
        {
            get { return _LoadMetric; }
            set { SetField(ref _LoadMetric, value); }
        }

        MetricBase _UsedMetric;
        public MetricBase UsedMetric
        {
            get { return _UsedMetric; }
            set { SetField(ref _UsedMetric, value); }
        }

        DriveLayout _DriveLayout;
        public DriveLayout DriveLayout
        {
            get { return _DriveLayout; }
            set { SetField(ref _DriveLayout, value); }
        }

        bool _IsDevicePowerOn;
        public bool IsDevicePowerOn
        {
            get { return _IsDevicePowerOn; }
            set { SetField(ref _IsDevicePowerOn, value); }
        }

        #endregion

        #region Public

        public override void Update()
        {
            base.Update();

            if (Hardware is StorageDeviceLHM sd)
            {
                IsDevicePowerOn = _Storage.IsDevicePowerOn.GetValueOrDefault();

                var driveLettersForStorageDevice = _Storage.Partitions
                    .Where(p => p.DriveLetter is not null)
                    .OrderBy(p => p.DriveLetter)
                    .Select(p => $"{p.DriveLetter.Value}:")
                    .ToList();

                if (driveLettersForStorageDevice.Count > 0)
                {
                    Name = StringExtensions.Join(", ", driveLettersForStorageDevice);
                }
                else
                {
                    Name = Hardware.Name;
                }

                //Re-check for toggled sensors (Free Space & Used Space)
                TrySetFreeSizeSensor(HardwareMetrics);
                TrySetUsageSensor(HardwareMetrics);

                if (UsedMetric != null && _FreeSize != null)
                {
                    var used = _TotalSize.Value.GetValueOrDefault() - _FreeSize.Value.GetValueOrDefault();

                    //UsedMetric.Update((double)DataStorageSizeConverter.ByteToGigabyte((ulong)usedBytes));
                    UsedMetric.Update(used);
                }
            }
        }

        #endregion

        #region Protected

        protected override void OnRequestHardwareDetails()
        {
            EventDistributor.ShowDriveDetails(Hardware as StorageDeviceLHM);
        }

        #endregion

        #region Private

        void Initialize()
        {
            var hardwareMetricList = new ObservableCollectionEx<IHardwareMetric>();

            //Total Size (for calculation)
            {
                _TotalSize = Hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Data && s.Index == 32);
            }

            //Drive Load
            {
                TrySetUsageSensor(hardwareMetricList);
            }

            //Drive Used
            {
                //Moved into toggle method for Free Size sensor (dependent)
            }

            //Drive Free
            {
                TrySetFreeSizeSensor(hardwareMetricList);
            }

            //Drive Read
            {
                var readActivity = Hardware.Sensors.Where(s => s.SensorType == SensorType.Load && s.Index == 51).FirstOrDefault();
                if (readActivity != null)
                {
                    hardwareMetricList.Add(new MetricBoardItem(readActivity, HardwareMetricKey.DriveRead, DataType.MegaBytePerSecond));
                }
            }

            //Drive Write
            {
                var writeActivity = Hardware.Sensors.Where(s => s.SensorType == SensorType.Load && s.Index == 52).FirstOrDefault();
                if (writeActivity != null)
                {
                    hardwareMetricList.Add(new MetricBoardItem(writeActivity, HardwareMetricKey.DriveWrite, DataType.MegaBytePerSecond));
                }
            }

            //Drive Temp
            {
                ISensor tempSensor = Hardware.Sensors.Where(s => s.SensorType == SensorType.Temperature).FirstOrDefault();

                if (tempSensor != null)
                {
                    hardwareMetricList.Add(new MetricBoardItem(tempSensor, HardwareMetricKey.DriveTemp, DataType.Celsius));
                }
            }

            HardwareMetrics.Clear();
            HardwareMetrics.AddRange(hardwareMetricList);
        }

        void TrySetFreeSizeSensor(ObservableCollectionEx<IHardwareMetric> hardwareMetricList)
        {
            //Try find sensor
            var freeSizeSensor = Hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Data && s.Index == 31);

            //Toggled off
            if (_FreeSize != null && freeSizeSensor == null)
            {
                hardwareMetricList.Remove(mb => mb is MetricBoardItem mbi && mbi.HasSensor(_FreeSize));
                _FreeSize = null;

                hardwareMetricList.Remove(UsedMetric);
                UsedMetric = null;
            }

            //Toggled on OR initial setup
            if (_FreeSize == null && freeSizeSensor != null)
            {
                _FreeSize = freeSizeSensor;
                hardwareMetricList.Add(new MetricBoardItem(_FreeSize, HardwareMetricKey.DriveFree, DataType.Gigabyte) /*{ Converter = ConverterFactory.GetConverterShared<ByteToGigabyteConverter>() }*/);

                UsedMetric = new MetricBase(HardwareMetricKey.DriveUsed, DataType.Gigabyte);
                hardwareMetricList.Add(UsedMetric);
            }
        }

        void TrySetUsageSensor(ObservableCollectionEx<IHardwareMetric> hardwareMetricList)
        {
            //Try find sensor
            var driveLoadSensor = Hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Index == 30);

            //Toggled off
            if (LoadMetric != null && driveLoadSensor == null)
            {
                hardwareMetricList.Remove(LoadMetric);
                LoadMetric = null;

                //Remove Load Bar
                DriveLayout = DriveLayout.NoLoadBar;
            }

            //Toggled on OR initial setup
            if (LoadMetric == null && driveLoadSensor != null)
            {
                LoadMetric = new MetricBoardItem(driveLoadSensor, HardwareMetricKey.DriveLoad, DataType.Percent);
                hardwareMetricList.Add(LoadMetric);

                //Add Load Bar
                DriveLayout = DriveLayout.LoadBarStacked;
            }
        }

        void OnSettingsChanged(object sender, SettingsChangedEventArgs e)
        {
            if (e.NewSettings == null)
            {
                return;
            }

            ShowName = e.NewSettings.GetHardwareConfigOptionValue<bool>(HardwareMonitorType.Storage, HardwareConfigOption.HardwareNames);

            SharedMethods.SetRoundAll     (this, HardwareMonitorType.Storage, e.NewSettings);
            SharedMethods.SetUseFahrenheit(this, HardwareMonitorType.Storage, e.NewSettings);
            SharedMethods.SetTempAlert    (this, HardwareMonitorType.Storage, e.NewSettings);

            SharedMethods.SetDataRateUnit(this, HardwareMonitorType.Storage, e.NewSettings, HardwareMetrics.FirstOrDefault(m => m.HardwareMetricKey == HardwareMetricKey.DriveRead ) as MetricBase, DataUnit.Byte);
            SharedMethods.SetDataRateUnit(this, HardwareMonitorType.Storage, e.NewSettings, HardwareMetrics.FirstOrDefault(m => m.HardwareMetricKey == HardwareMetricKey.DriveWrite) as MetricBase, DataUnit.Byte);

            long throttleInterval = e.NewSettings.GetHardwareConfigOptionValue<long>(HardwareMonitorType.Storage, HardwareConfigOption.ThrottleInterval);
            if (throttleInterval > 0)
            {
                //Enable
                StorageDeviceLHM.ThrottleInterval = TimeSpan.FromMilliseconds(throttleInterval);
            }
            else
            {
                //Disable
                StorageDeviceLHM.ThrottleInterval = TimeSpan.FromMilliseconds(0);
            }

            short usedSpaceAlert = e.NewSettings.GetHardwareConfigOptionValue<short>(HardwareMonitorType.Storage, HardwareConfigOption.UsedSpaceAlert);

            HardwareMetrics.ForEach(hm =>
            {
                if (hm is MetricBase mb
                 && mb.HardwareMetricKey == HardwareMetricKey.DriveLoad)
                {
                    mb.AlertValue = usedSpaceAlert;
                }
            });

            bool forceDriveWakeup = e.NewSettings.GetHardwareConfigOptionValue<bool>(HardwareMonitorType.Storage, HardwareConfigOption.ForceDriveWakeup);
            (Hardware as StorageDeviceLHM).ForceWakeup = forceDriveWakeup;

            //Set Load Bar layout
            bool loadBarEnabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.Storage, HardwareMetricKey.DriveLoadBar);
            if (loadBarEnabled && ShouldLoadBarBeEnabled)
            {
                DriveLayout = DriveLayout.LoadBarStacked;
            }
            else
            {
                DriveLayout = DriveLayout.NoLoadBar;
            }

            //Set DriveLoad
            var load = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.DriveLoad);
            load?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.Storage, HardwareMetricKey.DriveLoad);

            //Set DriveUsed
            var used = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.DriveUsed);
            used?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.Storage, HardwareMetricKey.DriveUsed);

            //Set DriveFree
            var free = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.DriveFree);
            free?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.Storage, HardwareMetricKey.DriveFree);

            //Set DriveRead
            var read = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.DriveRead);
            read?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.Storage, HardwareMetricKey.DriveRead);

            //Set DriveWrite
            var write = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.DriveWrite);
            write?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.Storage, HardwareMetricKey.DriveWrite);

            //Set DriveTemp
            var temps = HardwareMetrics.Where(hm => hm.HardwareMetricKey == HardwareMetricKey.DriveTemp).ToList();
            bool tempsEnabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.Storage, HardwareMetricKey.DriveTemp);
            temps.ForEach(t => t.Enabled = tempsEnabled);
        }

        #endregion
    }
}

#pragma warning restore CA1416 // Platform compatibility warning
