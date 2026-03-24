/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using LibreDiagnostics.Models.Configuration;
using LibreDiagnostics.Models.Enums;
using LibreDiagnostics.Models.Events;
using LibreDiagnostics.Models.Globals;
using LibreDiagnostics.Models.Hardware.Metrics;
using LibreHardwareMonitor.Hardware;

namespace LibreDiagnostics.Models.Hardware.HardwareMonitor
{
    public class HardwareMonitorRAM : HardwareMonitorBoardItem
    {
        #region Constructor

        public HardwareMonitorRAM(IHardware hardware, IHardware board, HardwareConfig config)
            : base(hardware, config)
        {
            Initialize(board);

            Global.SettingsChanged += OnSettingsChanged;

            //Initial call to apply settings
            OnSettingsChanged(this, new(Global.Settings));
        }

        #endregion

        #region Protected

        protected override void OnRequestHardwareDetails()
        {
            EventDistributor.ShowRAMDetails(Hardware);
        }

        #endregion

        #region Private

        void Initialize(IHardware board)
        {
            var sensorList = new List<MetricRAM>();

            //Clock
            {
                ISensor ramClock = Hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Clock);

                if (ramClock != null)
                {
                    sensorList.Add(new MetricRAM(ramClock, HardwareMetricKey.RAMClock, DataType.MHz));
                }
            }

            //Voltage
            {
                ISensor voltage = null;

                if (board != null)
                {
                    voltage = board.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Voltage && s.Name.Contains("RAM"));
                }

                if (voltage == null)
                {
                    voltage = Hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Voltage);
                }

                if (voltage != null)
                {
                    sensorList.Add(new MetricRAM(voltage, HardwareMetricKey.RAMVoltage, DataType.Voltage));
                }
            }

            //Load
            {
                ISensor loadSensor = Hardware.Sensors.FirstOrDefault(s =>
                    (s.SensorType == SensorType.Load && s.Index == 0) //Total  Memory
                 || (s.SensorType == SensorType.Load && s.Index == 1) //Virtual Memory
                );

                if (loadSensor != null)
                {
                    sensorList.Add(new MetricRAM(loadSensor, HardwareMetricKey.RAMLoad, DataType.Percent));
                }
            }

            //Used
            {
                ISensor usedSensor = Hardware.Sensors.FirstOrDefault(s =>
                    (s.SensorType == SensorType.Data && s.Index == 0) //Total  Memory
                 || (s.SensorType == SensorType.Data && s.Index == 2) //Virtual Memory
                );

                if (usedSensor != null)
                {
                    sensorList.Add(new MetricRAM(usedSensor, HardwareMetricKey.RAMUsed, DataType.Gigabyte));
                }
            }

            //Free
            {
                ISensor freeSensor = Hardware.Sensors.FirstOrDefault(s =>
                    (s.SensorType == SensorType.Data && s.Index == 1) //Total  Memory
                 || (s.SensorType == SensorType.Data && s.Index == 3) //Virtual Memory
                );

                if (freeSensor != null)
                {
                    sensorList.Add(new MetricRAM(freeSensor, HardwareMetricKey.RAMFree, DataType.Gigabyte));
                }
            }

            //Temp
            {
                ISensor tempSensor = Hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature && s.Name.Contains("DIMM", StringComparison.OrdinalIgnoreCase));

                if (tempSensor != null)
                {
                    sensorList.Add(new MetricRAM(tempSensor, HardwareMetricKey.RAMTemp, DataType.Celsius));
                }
            }

            HardwareMetrics.Clear();
            HardwareMetrics.AddRange(sensorList);
        }

        void OnSettingsChanged(object sender, SettingsChangedEventArgs e)
        {
            if (e.NewSettings == null)
            {
                return;
            }

            ShowName = e.NewSettings.GetHardwareConfigOptionValue<bool>(HardwareMonitorType.RAM, HardwareConfigOption.HardwareNames);

            SharedMethods.SetRoundAll     (this, HardwareMonitorType.RAM, e.NewSettings);
            SharedMethods.SetUseFahrenheit(this, HardwareMonitorType.RAM, e.NewSettings);
            SharedMethods.SetTempAlert    (this, HardwareMonitorType.RAM, e.NewSettings);

            //Set Clock
            var clock = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.RAMClock);
            clock?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.RAM, HardwareMetricKey.RAMClock);

            //Set Voltage
            var voltage = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.RAMVoltage);
            voltage?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.RAM, HardwareMetricKey.RAMVoltage);

            //Set Load
            var load = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.RAMLoad);
            load?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.RAM, HardwareMetricKey.RAMLoad);

            //Set Used
            var used = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.RAMUsed);
            used?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.RAM, HardwareMetricKey.RAMUsed);

            //Set Free
            var free = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.RAMFree);
            free?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.RAM, HardwareMetricKey.RAMFree);

            //Set Temp
            var temp = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.RAMTemp);
            temp?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.RAM, HardwareMetricKey.RAMTemp);
        }

        #endregion
    }
}
