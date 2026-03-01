/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.Core.Converters.Enums;
using LibreDiagnostics.Models.Configuration;
using LibreDiagnostics.Models.Enums;
using LibreDiagnostics.Models.Globals;
using LibreDiagnostics.Models.Hardware.Metrics;
using LibreDiagnostics.Models.Helper;
using LibreHardwareMonitor.Hardware;

namespace LibreDiagnostics.Models.Hardware.HardwareMonitor
{
    public class HardwareMonitorNetwork : HardwareMonitorBoardItem
    {
        #region Constructor

        public HardwareMonitorNetwork(IHardware hardware, HardwareConfig config)
            : base(hardware, config)
        {
            Initialize();

            Global.SettingsChanged += OnSettingsChanged;

            //Initial call to apply settings
            OnSettingsChanged(this, new(Global.Settings));
        }

        #endregion

        #region Properties

        MetricBase _DownloadSpeedMetric;
        public MetricBase DownloadSpeedMetric
        {
            get { return _DownloadSpeedMetric; }
            set { SetField(ref _DownloadSpeedMetric, value); }
        }

        MetricBase _UploadSpeedMetric;
        public MetricBase UploadSpeedMetric
        {
            get { return _UploadSpeedMetric; }
            set { SetField(ref _UploadSpeedMetric, value); }
        }

        #endregion

        #region Private

        void Initialize()
        {
            var hardwareMetricList = new List<MetricBase>();

            //Download
            {
                var download = Hardware.Sensors.Where(s => s.SensorType == SensorType.Throughput && s.Name.Contains("download", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (download != null)
                {
                    DownloadSpeedMetric = new MetricBoardItem(download, HardwareMetricKey.NetworkIn, DataType.MegaBytePerSecond);
                    hardwareMetricList.Add(DownloadSpeedMetric);
                }
            }

            //Upload
            {
                var upload = Hardware.Sensors.Where(s => s.SensorType == SensorType.Throughput && s.Name.Contains("upload", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (upload != null)
                {
                    UploadSpeedMetric = new MetricBoardItem(upload, HardwareMetricKey.NetworkOut, DataType.MegaBytePerSecond);
                    hardwareMetricList.Add(UploadSpeedMetric);
                }
            }

            //Set IP
            {
                string ipAddress = IPAddressHelper.GetAdapterIPAddress(Name);

                if (!string.IsNullOrEmpty(ipAddress))
                {
                    hardwareMetricList.Add(new MetricIP(ipAddress, HardwareMetricKey.NetworkIP, DataType.IP));
                }
            }

            //Set External IP
            //var externalIP = IPAddressHelper.GetExternalIPAddress();

            //if (!string.IsNullOrEmpty(externalIP))
            //{
            //    hardwareMetricList.Add(new MetricIP(externalIP, HardwareMetricKey.NetworkExtIP, DataType.IP));
            //}

            HardwareMetrics.Clear();
            HardwareMetrics.AddRange(hardwareMetricList);
        }

        void OnSettingsChanged(object sender, SettingsChangedEventArgs e)
        {
            if (e.NewSettings == null)
            {
                return;
            }

            ShowName = e.NewSettings.GetHardwareConfigOptionValue<bool>(HardwareMonitorType.Network, HardwareConfigOption.HardwareNames);

            bool roundAll = e.NewSettings.GetHardwareConfigOptionValue<bool>(HardwareMonitorType.Network, HardwareConfigOption.RoundAll);
            HardwareMetrics.ForEach(hm => (hm as MetricBase)?.Round = roundAll);

            long downloadThreshold = e.NewSettings.GetHardwareConfigOptionValue<long>(HardwareMonitorType.Network, HardwareConfigOption.BandwidthInAlert );
            long uploadThreshold   = e.NewSettings.GetHardwareConfigOptionValue<long>(HardwareMonitorType.Network, HardwareConfigOption.BandwidthOutAlert);

            DownloadSpeedMetric?.AlertValue = downloadThreshold;
            UploadSpeedMetric  ?.AlertValue = uploadThreshold;

            SharedMethods.SetDataRateUnit(this, HardwareMonitorType.Network, e.NewSettings, DownloadSpeedMetric, DataUnit.Byte);
            SharedMethods.SetDataRateUnit(this, HardwareMonitorType.Network, e.NewSettings, UploadSpeedMetric  , DataUnit.Byte);

            //Set download
            var download = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.NetworkIn);
            download?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.Network, HardwareMetricKey.NetworkIn);

            //Set upload
            var upload = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.NetworkOut);
            upload?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.Network, HardwareMetricKey.NetworkOut);

            //Set IP
            var ip = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.NetworkIP);
            ip?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.Network, HardwareMetricKey.NetworkIP);

            //Set External IP
            //var extIp = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.NetworkExtIP);
            //extIp?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.Network, HardwareMetricKey.NetworkExtIP);
        }

        #endregion
    }
}
