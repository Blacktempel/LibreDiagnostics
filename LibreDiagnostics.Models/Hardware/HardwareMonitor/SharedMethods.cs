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
using LibreDiagnostics.Models.Converter;
using LibreDiagnostics.Models.Enums;
using LibreDiagnostics.Models.Hardware.Metrics;
using LibreDiagnostics.Models.Interfaces;

namespace LibreDiagnostics.Models.Hardware.HardwareMonitor
{
    internal sealed class SharedMethods
    {
        #region Public

        public static void SetRoundAll(IHardwareMonitor monitor, HardwareMonitorType hardwareMonitorType, Settings settings)
        {
            bool roundAll = settings.GetHardwareConfigOptionValue<bool>(hardwareMonitorType, HardwareConfigOption.RoundAll);

            monitor.HardwareMetrics.ForEach(hm => (hm as MetricBase)?.Round = roundAll);
        }

        public static void SetUseFahrenheit(IHardwareMonitor monitor, HardwareMonitorType hardwareMonitorType, Settings settings)
        {
            bool useFahrenheit = settings.GetHardwareConfigOptionValue<bool>(hardwareMonitorType, HardwareConfigOption.UseFahrenheit);

            monitor.HardwareMetrics.ForEach(hm =>
            {
                if (hm is MetricBase mb
                 && mb.DataType.AnyOf(DataType.Celsius, DataType.Fahrenheit))
                {
                    mb.Converter = useFahrenheit ? ConverterFactory.GetConverterShared<CelsiusToFahrenheitConverter>() : null;
                    mb.DataType = useFahrenheit ? DataType.Fahrenheit : DataType.Celsius;
                }
            });
        }

        public static void SetTempAlert(IHardwareMonitor monitor, HardwareMonitorType hardwareMonitorType, Settings settings)
        {
            short temperatureAlert = settings.GetHardwareConfigOptionValue<short>(hardwareMonitorType, HardwareConfigOption.TempAlert);

            monitor.HardwareMetrics.ForEach(hm =>
            {
                if (hm is MetricBase mb
                 && mb.DataType.AnyOf(DataType.Celsius, DataType.Fahrenheit))
                {
                    mb.AlertValue = temperatureAlert;
                }
            });
        }

        #endregion
    }
}
