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
using LibreDiagnostics.Models.Globals;
using LibreDiagnostics.Models.Hardware.Metrics;
using LibreHardwareMonitor.Hardware;

namespace LibreDiagnostics.Models.Hardware.HardwareMonitor
{
    public class HardwareMonitorPowerMonitor : HardwareMonitorBoardItem
    {
        #region Constructor

        public HardwareMonitorPowerMonitor(IHardware hardware, HardwareConfig config)
            : base(hardware, config)
        {
            Initialize();

            Global.SettingsChanged += OnSettingsChanged;

            //Initial call to apply settings
            OnSettingsChanged(this, new(Global.Settings));
        }

        #endregion

        #region Private

        void Initialize()
        {
            var sensorList = new List<MetricBase>();

            void TryAddSensor(ISensor sensor, HardwareMetricKey key, DataType type)
            {
                if (sensor != null)
                {
                    if (sensor.SensorType == SensorType.Temperature
                     && sensor.Value.GetValueOrDefault() == -100f)
                    {
                        //Skip invalid temperature sensors (usually external sensors that are not connected)
                        return;
                    }

                    sensorList.Add(new MetricPowerMonitor(sensor, key, type));
                }
            }

            //Temperatures
            {
                ISensor temp1 = Hardware.Sensors.Where(s => s.SensorType == SensorType.Temperature && s.Index == 0).FirstOrDefault();
                ISensor temp2 = Hardware.Sensors.Where(s => s.SensorType == SensorType.Temperature && s.Index == 1).FirstOrDefault();
                ISensor temp3 = Hardware.Sensors.Where(s => s.SensorType == SensorType.Temperature && s.Index == 2).FirstOrDefault();
                ISensor temp4 = Hardware.Sensors.Where(s => s.SensorType == SensorType.Temperature && s.Index == 3).FirstOrDefault();

                TryAddSensor(temp1, HardwareMetricKey.PowerMonitorTemp, DataType.Celsius);
                TryAddSensor(temp2, HardwareMetricKey.PowerMonitorTemp, DataType.Celsius);
                TryAddSensor(temp3, HardwareMetricKey.PowerMonitorTemp, DataType.Celsius);
                TryAddSensor(temp4, HardwareMetricKey.PowerMonitorTemp, DataType.Celsius);
            }

            //Pin voltages
            {
                ISensor pinVoltage1 = Hardware.Sensors.Where(s => s.SensorType == SensorType.Voltage && s.Index == 10).FirstOrDefault();
                ISensor pinVoltage2 = Hardware.Sensors.Where(s => s.SensorType == SensorType.Voltage && s.Index == 11).FirstOrDefault();
                ISensor pinVoltage3 = Hardware.Sensors.Where(s => s.SensorType == SensorType.Voltage && s.Index == 12).FirstOrDefault();
                ISensor pinVoltage4 = Hardware.Sensors.Where(s => s.SensorType == SensorType.Voltage && s.Index == 13).FirstOrDefault();
                ISensor pinVoltage5 = Hardware.Sensors.Where(s => s.SensorType == SensorType.Voltage && s.Index == 14).FirstOrDefault();
                ISensor pinVoltage6 = Hardware.Sensors.Where(s => s.SensorType == SensorType.Voltage && s.Index == 15).FirstOrDefault();

                TryAddSensor(pinVoltage1, HardwareMetricKey.PowerMonitorPinVoltage, DataType.Voltage);
                TryAddSensor(pinVoltage2, HardwareMetricKey.PowerMonitorPinVoltage, DataType.Voltage);
                TryAddSensor(pinVoltage3, HardwareMetricKey.PowerMonitorPinVoltage, DataType.Voltage);
                TryAddSensor(pinVoltage4, HardwareMetricKey.PowerMonitorPinVoltage, DataType.Voltage);
                TryAddSensor(pinVoltage5, HardwareMetricKey.PowerMonitorPinVoltage, DataType.Voltage);
                TryAddSensor(pinVoltage6, HardwareMetricKey.PowerMonitorPinVoltage, DataType.Voltage);
            }

            //Pin total current
            {
                ISensor totalCurrent = Hardware.Sensors.Where(s => s.SensorType == SensorType.Current && s.Index == 20).FirstOrDefault();

                TryAddSensor(totalCurrent, HardwareMetricKey.PowerMonitorTotalCurrent, DataType.Ampere);
            }

            //Pin currents
            {
                ISensor pinCurrent1 = Hardware.Sensors.Where(s => s.SensorType == SensorType.Current && s.Index == 21).FirstOrDefault();
                ISensor pinCurrent2 = Hardware.Sensors.Where(s => s.SensorType == SensorType.Current && s.Index == 22).FirstOrDefault();
                ISensor pinCurrent3 = Hardware.Sensors.Where(s => s.SensorType == SensorType.Current && s.Index == 23).FirstOrDefault();
                ISensor pinCurrent4 = Hardware.Sensors.Where(s => s.SensorType == SensorType.Current && s.Index == 24).FirstOrDefault();
                ISensor pinCurrent5 = Hardware.Sensors.Where(s => s.SensorType == SensorType.Current && s.Index == 25).FirstOrDefault();
                ISensor pinCurrent6 = Hardware.Sensors.Where(s => s.SensorType == SensorType.Current && s.Index == 26).FirstOrDefault();

                TryAddSensor(pinCurrent1, HardwareMetricKey.PowerMonitorPinCurrent, DataType.Ampere);
                TryAddSensor(pinCurrent2, HardwareMetricKey.PowerMonitorPinCurrent, DataType.Ampere);
                TryAddSensor(pinCurrent3, HardwareMetricKey.PowerMonitorPinCurrent, DataType.Ampere);
                TryAddSensor(pinCurrent4, HardwareMetricKey.PowerMonitorPinCurrent, DataType.Ampere);
                TryAddSensor(pinCurrent5, HardwareMetricKey.PowerMonitorPinCurrent, DataType.Ampere);
                TryAddSensor(pinCurrent6, HardwareMetricKey.PowerMonitorPinCurrent, DataType.Ampere);
            }

            //Power
            {
                ISensor totalPower = Hardware.Sensors.Where(s => s.SensorType == SensorType.Power && s.Index == 30).FirstOrDefault();

                TryAddSensor(totalPower, HardwareMetricKey.PowerMonitorPower, DataType.Watt);
            }

            //Fan
            {
                ISensor fan = Hardware.Sensors.Where(s => s.SensorType == SensorType.Fan && s.Index == 40).FirstOrDefault();

                TryAddSensor(fan, HardwareMetricKey.PowerMonitorFan, DataType.RPM);
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

            ShowName = e.NewSettings.GetHardwareConfigOptionValue<bool>(HardwareMonitorType.PowerMonitor, HardwareConfigOption.HardwareNames);

            SharedMethods.SetRoundAll     (this, HardwareMonitorType.PowerMonitor, e.NewSettings);
            SharedMethods.SetUseFahrenheit(this, HardwareMonitorType.PowerMonitor, e.NewSettings);
            SharedMethods.SetTempAlert    (this, HardwareMonitorType.PowerMonitor, e.NewSettings);

            //Set PowerMonitorTemp
            var temps = HardwareMetrics.Where(hm => hm.HardwareMetricKey == HardwareMetricKey.PowerMonitorTemp).ToList();
            bool tempsEnabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.PowerMonitor, HardwareMetricKey.PowerMonitorTemp);
            temps.ForEach(hm => hm.Enabled = tempsEnabled);

            //Set PowerMonitorPinVoltage
            var pinVoltages = HardwareMetrics.Where(hm => hm.HardwareMetricKey == HardwareMetricKey.PowerMonitorPinVoltage).ToList();
            bool pinVoltagesEnabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.PowerMonitor, HardwareMetricKey.PowerMonitorPinVoltage);
            pinVoltages.ForEach(hm => hm.Enabled = pinVoltagesEnabled);

            //Set PowerMonitorTotalCurrent
            var totalCurrent = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.PowerMonitorTotalCurrent);
            totalCurrent?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.PowerMonitor, HardwareMetricKey.PowerMonitorTotalCurrent);

            //Set PowerMonitorPinCurrent
            var pinCurrents = HardwareMetrics.Where(hm => hm.HardwareMetricKey == HardwareMetricKey.PowerMonitorPinCurrent).ToList();
            bool pinCurrentsEnabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.PowerMonitor, HardwareMetricKey.PowerMonitorPinCurrent);
            pinCurrents.ForEach(hm => hm.Enabled = pinCurrentsEnabled);

            //Set PowerMonitorTotalCurrent
            var power = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.PowerMonitorPower);
            power?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.PowerMonitor, HardwareMetricKey.PowerMonitorPower);

            //Set PowerMonitorFan
            var fan = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.PowerMonitorFan);
            fan?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.PowerMonitor, HardwareMetricKey.PowerMonitorFan);
        }

        #endregion
    }
}
