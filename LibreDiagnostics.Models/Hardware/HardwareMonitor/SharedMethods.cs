/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.Core.Converters.Enums;
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

        public static void SetDataRateUnit(IHardwareMonitor monitor, HardwareMonitorType hardwareMonitorType, Settings settings, MetricBase hardwareMetric, DataUnit source)
        {
            var dataRateUnit = settings.GetHardwareConfigOptionValue<DataUnit>(hardwareMonitorType, HardwareConfigOption.DataRateUnit);

            SetDataRateUnit(hardwareMetric, source, dataRateUnit);
        }

        #endregion

        #region Private

        static void SetDataRateUnit(MetricBase metric, DataUnit source, DataUnit target)
        {
            if (!TryGetDataRateUnit(target, out var dataType))
            {
                target = DataUnit.MegaByte;
                dataType = DataType.MegaBytePerSecond;
            }

            if (source != target)
            {
                metric.Converter = new DataRateUnitConverter(source, target);
            }
            else
            {
                metric.Converter = null;
            }

            metric.DataType = dataType;
        }

        static bool TryGetDataRateUnit(DataUnit dataUnit, out DataType dataType)
        {
            switch (dataUnit)
            {
                case DataUnit.Byte:
                    dataType = DataType.BytePerSecond    ; break;
                case DataUnit.KiloByte:
                    dataType = DataType.KiloBytePerSecond; break;
                case DataUnit.MegaByte:
                    dataType = DataType.MegaBytePerSecond; break;
                case DataUnit.GigaByte:
                    dataType = DataType.GigaBytePerSecond; break;
                case DataUnit.TeraByte:
                    dataType = DataType.TeraBytePerSecond; break;

                case DataUnit.Bit:
                    dataType = DataType.BitPerSecond     ; break;
                case DataUnit.KiloBit:
                    dataType = DataType.KiloBitPerSecond ; break;
                case DataUnit.MegaBit:
                    dataType = DataType.MegaBitPerSecond ; break;
                case DataUnit.GigaBit:
                    dataType = DataType.GigaBitPerSecond ; break;
                case DataUnit.TeraBit:
                    dataType = DataType.TeraBitPerSecond ; break;

                default:
                    dataType = DataType.MegaBytePerSecond;
                    return false;
            }

            return true;
        }

        #endregion
    }
}
