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
    public class HardwareMonitorGPU : HardwareMonitorBoardItem
    {
        #region Constructor

        public HardwareMonitorGPU(IHardware hardware, HardwareConfig config)
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

            //Core Clock
            {
                ISensor coreClock = Hardware.Sensors.Where(s => s.SensorType == SensorType.Clock && s.Name.Contains("Core")).FirstOrDefault();

                if (coreClock != null)
                {
                    sensorList.Add(new MetricGPU(coreClock, HardwareMetricKey.GPUCoreClock, DataType.MHz));
                }
            }

            //VRAM Clock
            {
                ISensor vramClock = Hardware.Sensors.Where(s => s.SensorType == SensorType.Clock && s.Name.Contains("Memory")).FirstOrDefault();

                if (vramClock != null)
                {
                    sensorList.Add(new MetricGPU(vramClock, HardwareMetricKey.GPUVRAMClock, DataType.MHz));
                }
            }

            //Core Load
            {
                ISensor _coreLoad = Hardware.Sensors.Where(s => s.SensorType == SensorType.Load && s.Name.Contains("Core")).FirstOrDefault() ??
                    Hardware.Sensors.Where(s => s.SensorType == SensorType.Load && s.Index == 0).FirstOrDefault();

                //Slightly different sensor for Intel GPUs
                if (Hardware.HardwareType == HardwareType.GpuIntel)
                {
                    _coreLoad = Hardware.Sensors.Where(s => s.SensorType == SensorType.Load && s.Name.Contains("D3D 3D")).FirstOrDefault();
                }

                if (_coreLoad != null)
                {
                    sensorList.Add(new MetricGPU(_coreLoad, HardwareMetricKey.GPUCoreLoad, DataType.Percent));
                }
            }

            //VRAM Load
            {
                ISensor memoryUsed = Hardware.Sensors.Where(s => (s.SensorType == SensorType.Data || s.SensorType == SensorType.SmallData) && s.Name.Contains("Memory Used", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                ISensor memoryTotal = Hardware.Sensors.Where(s => (s.SensorType == SensorType.Data || s.SensorType == SensorType.SmallData) && s.Name.Contains("Memory Total", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                if (memoryUsed != null && memoryTotal != null)
                {
                    sensorList.Add(new MetricGPUVRAMLoad(memoryUsed, memoryTotal, HardwareMetricKey.GPUVRAMLoad));
                }
                else
                {
                    ISensor vramLoad = Hardware.Sensors.Where(s => s.SensorType == SensorType.Load && s.Name.Contains("Memory")).FirstOrDefault() ??
                        Hardware.Sensors.Where(s => s.SensorType == SensorType.Load && s.Index == 1).FirstOrDefault();

                    if (vramLoad != null)
                    {
                        sensorList.Add(new MetricGPU(vramLoad, HardwareMetricKey.GPUVRAMLoad, DataType.Percent));
                    }
                }
            }

            //Voltage
            {
                ISensor voltage = Hardware.Sensors.Where(s => s.SensorType == SensorType.Voltage && s.Index == 0).FirstOrDefault();

                if (voltage != null)
                {
                    sensorList.Add(new MetricGPU(voltage, HardwareMetricKey.GPUVoltage, DataType.Voltage));
                }
            }

            //Temp
            {
                ISensor tempSensor = Hardware.Sensors.Where(s => s.SensorType == SensorType.Temperature && s.Index == 0).FirstOrDefault();

                if (tempSensor != null)
                {
                    sensorList.Add(new MetricGPU(tempSensor, HardwareMetricKey.GPUTemp, DataType.Celsius));
                }
            }

            //Fan
            {
                ISensor fanSensor = Hardware.Sensors.Where(s => s.SensorType == SensorType.Control).OrderBy(s => s.Index).FirstOrDefault();

                if (fanSensor != null)
                {
                    sensorList.Add(new MetricGPU(fanSensor, HardwareMetricKey.GPUFan, DataType.Percent));
                }
            }

            //GPU Power
            {
                var powerSensor = Hardware.Sensors.Where(s => s.SensorType == SensorType.Power && s.Name.Contains("Package", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                //Slightly different sensor for Intel GPUs
                if (powerSensor == null && Hardware.HardwareType == HardwareType.GpuIntel)
                {
                    powerSensor = Hardware.Sensors.Where(s => s.SensorType == SensorType.Power && s.Name.Contains("Power", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                }

                if (powerSensor != null)
                {
                    sensorList.Add(new MetricGPU(powerSensor, HardwareMetricKey.GPUPower, DataType.Watt));
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

            ShowName = e.NewSettings.GetHardwareConfigOptionValue<bool>(HardwareMonitorType.GPU, HardwareConfigOption.HardwareNames);

            SharedMethods.SetRoundAll     (this, HardwareMonitorType.GPU, e.NewSettings);
            SharedMethods.SetUseFahrenheit(this, HardwareMonitorType.GPU, e.NewSettings);
            SharedMethods.SetTempAlert    (this, HardwareMonitorType.GPU, e.NewSettings);

            //Set CoreClock
            var clock = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.GPUCoreClock);
            clock?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.GPU, HardwareMetricKey.GPUCoreClock);

            //Set VRAMClock
            var vramClock = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.GPUVRAMClock);
            vramClock?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.GPU, HardwareMetricKey.GPUVRAMClock);

            //Set CoreLoad
            var coreLoad = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.GPUCoreLoad);
            coreLoad?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.GPU, HardwareMetricKey.GPUCoreLoad);

            //Set VRAMLoad
            var vramLoad = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.GPUVRAMLoad);
            vramLoad?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.GPU, HardwareMetricKey.GPUVRAMLoad);

            //Set Voltage
            var voltage = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.GPUVoltage);
            voltage?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.GPU, HardwareMetricKey.GPUVoltage);

            //Set Temp
            var temp = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.GPUTemp);
            temp?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.GPU, HardwareMetricKey.GPUTemp);

            //Set Fan
            var fan = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.GPUFan);
            fan?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.GPU, HardwareMetricKey.GPUFan);

            //Set Power
            var power = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.GPUPower);
            power?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.GPU, HardwareMetricKey.GPUPower);
        }

        #endregion
    }
}
