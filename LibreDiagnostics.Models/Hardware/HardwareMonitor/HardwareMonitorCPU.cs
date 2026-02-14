/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.Core.Extensions;
using LibreDiagnostics.Models.Configuration;
using LibreDiagnostics.Models.Enums;
using LibreDiagnostics.Models.Globals;
using LibreDiagnostics.Models.Hardware.Metrics;
using LibreHardwareMonitor.Hardware;
using System.Text.RegularExpressions;

namespace LibreDiagnostics.Models.Hardware.HardwareMonitor
{
    public class HardwareMonitorCPU : HardwareMonitorBoardItem
    {
        #region Constructor

        public HardwareMonitorCPU(IHardware hardware, IHardware board, HardwareConfig config)
            : base(hardware, config)
        {
            Initialize(board);

            Global.SettingsChanged += OnSettingsChanged;

            //Initial call to apply settings
            OnSettingsChanged(this, new(Global.Settings));
        }

        #endregion

        #region Fields

        readonly Regex _CPURegex = new Regex(@"^.*(CPU|Core).*#(\d+)$");

        #endregion

        #region Private

        void Initialize(IHardware board)
        {
            var sensorList = new List<MetricCPU>();

            //CPU Clocks
            {
                var coreClocks = Hardware.Sensors
                    .Where(s => s.SensorType == SensorType.Clock && _CPURegex.Match(s.Name)?.Success == true)
                    .OrderBy(s => s.Index)
                    .ToList();

                if (coreClocks.Count > 0)
                {
                    coreClocks.ForEach(s => sensorList.Add(new MetricCPU(s, HardwareMetricKey.CPUClock, DataType.MHz)));
                }
            }

            //CPU Voltages
            {
                ISensor voltage = null;

                if (board != null)
                {
                    voltage = board.Sensors.Where(s => s.SensorType == SensorType.Voltage && s.Name.Contains("CPU", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                }

                if (voltage == null)
                {
                    voltage = Hardware.Sensors.Where(s => s.SensorType == SensorType.Voltage).FirstOrDefault();
                }

                if (voltage != null)
                {
                    sensorList.Add(new MetricCPU(voltage, HardwareMetricKey.CPUVoltage, DataType.Voltage));
                }
            }

            //CPU Temps
            {
                ISensor tempSensor = null;

                tempSensor = Hardware.Sensors.Where(s => s.SensorType == SensorType.Temperature && s.Name.Contains("CCDs Max (Tdie)", StringComparison.OrdinalIgnoreCase)).FirstOrDefault(); // Check for AMD core chiplet dies (CCDs)

                if (board != null && tempSensor == null)
                {
                    tempSensor = board.Sensors.Where(s => s.SensorType == SensorType.Temperature && s.Name.Contains("CPU", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                }

                if (tempSensor == null)
                {
                    tempSensor =
                        Hardware.Sensors.Where(s => s.SensorType == SensorType.Temperature && (s.Name == "CPU Package" || s.Name.Contains("Tdie", StringComparison.OrdinalIgnoreCase))).FirstOrDefault() ??
                        Hardware.Sensors.Where(s => s.SensorType == SensorType.Temperature).FirstOrDefault();
                }

                if (tempSensor != null)
                {
                    sensorList.Add(new MetricCPU(tempSensor, HardwareMetricKey.CPUTemp, DataType.Celsius));
                }
            }

            //CPU Fans
            {
                ISensor fanSensor = null;

                if (board != null)
                {
                    fanSensor = board.Sensors.Where(s => s.SensorType == SensorType.Fan && s.Name.Contains("CPU", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                    if (fanSensor == null && board.SubHardware?.Length > 0)
                    {
                        var getSensor = (IHardware hw) =>
                            hw.Sensors?.Where(s => s.SensorType == SensorType.Fan && s.Name.Contains("CPU", StringComparison.OrdinalIgnoreCase))
                            ?? Enumerable.Empty<ISensor>();

                        var fanSensorHardware = board.SubHardware.FirstOrDefault(h => getSensor(h).Any());
                        if (fanSensorHardware != null)
                        {
                            SubHardware.Add(fanSensorHardware);

                            fanSensor = getSensor(fanSensorHardware).FirstOrDefault();
                        }
                    }
                }

                if (fanSensor == null)
                {
                    fanSensor = Hardware.Sensors.Where(s => s.SensorType == SensorType.Fan).FirstOrDefault();
                }

                if (fanSensor != null)
                {
                    sensorList.Add(new MetricCPU(fanSensor, HardwareMetricKey.CPUFan, DataType.RPM));
                }
            }

            //CPU Loads & CPU Core Loads
            {
                var loadSensors = Hardware.Sensors.Where(s => s.SensorType == SensorType.Load).ToList();

                if (loadSensors.Count > 0)
                {
                    //CPU Loads
                    {
                        ISensor totalCPU = loadSensors.Where(s => s.Index == 0).FirstOrDefault();

                        if (totalCPU != null)
                        {
                            sensorList.Add(new MetricCPU(totalCPU, HardwareMetricKey.CPULoad, DataType.Percent));
                        }
                    }

                    //CPU Core Loads
                    {
                        var coreLoadSensors = loadSensors
                            .Where(s => s.Index > 0 && _CPURegex.Match(s.Name)?.Success == true)
                            .OrderBy(s => s.Index)
                            .ToList();

                        coreLoadSensors.ForEach(s => sensorList.Add(new MetricCPU(s, HardwareMetricKey.CPUCoreLoad, DataType.Percent)));
                    }
                }
            }

            //CPU Power
            {
                var powerSensor = Hardware.Sensors.Where(s => s.SensorType == SensorType.Power && s.Name.Contains("Package", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                if (powerSensor != null)
                {
                    sensorList.Add(new MetricCPU(powerSensor, HardwareMetricKey.CPUPower, DataType.Watt));
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

            ShowName = e.NewSettings.GetHardwareConfigOptionValue<bool>(HardwareMonitorType.CPU, HardwareConfigOption.HardwareNames);

            SharedMethods.SetRoundAll     (this, HardwareMonitorType.CPU, e.NewSettings);
            SharedMethods.SetUseFahrenheit(this, HardwareMonitorType.CPU, e.NewSettings);
            SharedMethods.SetTempAlert    (this, HardwareMonitorType.CPU, e.NewSettings);

            var clocks = HardwareMetrics.Where(hm => hm.HardwareMetricKey == HardwareMetricKey.CPUClock).ToList();

            //Enable CPUClock
            if (e.NewSettings.IsConfigEnabled(HardwareMonitorType.CPU, HardwareMetricKey.CPUClock))
            {
                if (clocks.Count > 0)
                {
                    //Enable all
                    if (e.NewSettings.GetHardwareConfigOptionValue<bool>(HardwareMonitorType.CPU, HardwareConfigOption.AllCoreClocks))
                    {
                        clocks.ForEach(hm => hm.Enabled = true);
                    }
                    else //Disable all except first
                    {
                        bool first = true;
                        clocks.ForEach(hm =>
                        {
                            if (first)
                            {
                                hm.Enabled = true;
                                first = false;
                            }
                            else
                            {
                                hm.Enabled = false;
                            }
                        });
                    }
                }
            }
            else //Disable CPUClock
            {
                clocks.ForEach(hm => hm.Enabled = false);
            }

            //Set CPUVoltage
            var voltages = HardwareMetrics.Where(hm => hm.HardwareMetricKey == HardwareMetricKey.CPUVoltage).ToList();
            bool voltagesEnabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.CPU, HardwareMetricKey.CPUVoltage);
            voltages.ForEach(hm => hm.Enabled = voltagesEnabled);

            //Set CPUTemp
            var temps = HardwareMetrics.Where(hm => hm.HardwareMetricKey == HardwareMetricKey.CPUTemp).ToList();
            bool tempsEnabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.CPU, HardwareMetricKey.CPUTemp);
            temps.ForEach(hm => hm.Enabled = tempsEnabled);

            //Set CPUFan
            var fans = HardwareMetrics.Where(hm => hm.HardwareMetricKey == HardwareMetricKey.CPUFan).ToList();
            bool fansEnabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.CPU, HardwareMetricKey.CPUFan);
            fans.ForEach(hm => hm.Enabled = fansEnabled);

            //Set CPULoad
            var loads = HardwareMetrics.Where(hm => hm.HardwareMetricKey == HardwareMetricKey.CPULoad).ToList();
            bool loadsEnabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.CPU, HardwareMetricKey.CPULoad);
            loads.ForEach(hm => hm.Enabled = loadsEnabled);

            if (loadsEnabled)
            {
                //Set CPUCoreLoad
                var coreLoads = HardwareMetrics.Where(hm => hm.HardwareMetricKey == HardwareMetricKey.CPUCoreLoad).ToList();
                bool coreLoadsEnabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.CPU, HardwareMetricKey.CPUCoreLoad);
                coreLoads.ForEach(hm => hm.Enabled = coreLoadsEnabled);
            }

            //Set CPUPower
            var power = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.CPUPower);
            power?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.CPU, HardwareMetricKey.CPUPower);
        }

        #endregion
    }
}
