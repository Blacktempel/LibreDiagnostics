/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using LibreDiagnostics.Language.Resources;
using LibreDiagnostics.Models.Enums;

namespace LibreDiagnostics.Models.Helper
{
    public class HardwareHardwareMetricKeyTranslator
    {
        public static string GetMonitorType(HardwareMonitorType type)
        {
            switch (type)
            {
                case HardwareMonitorType.CPU:
                    return Resources.CPU;

                case HardwareMonitorType.RAM:
                    return Resources.RAM;

                case HardwareMonitorType.GPU:
                    return Resources.GPU;

                case HardwareMonitorType.Storage:
                    return Resources.Drives;

                case HardwareMonitorType.Network:
                    return Resources.Network;

                case HardwareMonitorType.Fan:
                    return Resources.Fan;

                case HardwareMonitorType.PowerMonitor:
                    return Resources.PowerMonitor;

                default:
                    throw new ArgumentException($"Invalid {nameof(HardwareMonitorType)}: '{type}'.");
            }
        }

        public static string GetFullName(HardwareMetricKey key)
        {
            switch (key)
            {
                case HardwareMetricKey.CPUClock:
                    return Resources.CPUClock;

                case HardwareMetricKey.CPUTemp:
                    return Resources.CPUTemp;

                case HardwareMetricKey.CPUVoltage:
                    return Resources.CPUVoltage;

                case HardwareMetricKey.CPUFan:
                    return Resources.CPUFan;

                case HardwareMetricKey.CPULoad:
                    return Resources.CPULoad;

                case HardwareMetricKey.CPUCoreLoad:
                    return Resources.CPUCoreLoad;

                case HardwareMetricKey.CPUPower:
                    return Resources.CPUPower;

                case HardwareMetricKey.RAMClock:
                    return Resources.RAMClock;

                case HardwareMetricKey.RAMVoltage:
                    return Resources.RAMVoltage;

                case HardwareMetricKey.RAMLoad:
                    return Resources.RAMLoad;

                case HardwareMetricKey.RAMUsed:
                    return Resources.RAMUsed;

                case HardwareMetricKey.RAMFree:
                    return Resources.RAMFree;

                case HardwareMetricKey.RAMTemp:
                    return Resources.RAMTemp;

                case HardwareMetricKey.GPUCoreClock:
                    return Resources.GPUCoreClock;

                case HardwareMetricKey.GPUVRAMClock:
                    return Resources.GPUVRAMClock;

                case HardwareMetricKey.GPUCoreLoad:
                    return Resources.GPUCoreLoad;

                case HardwareMetricKey.GPUVRAMLoad:
                    return Resources.GPUVRAMLoad;

                case HardwareMetricKey.GPUVoltage:
                    return Resources.GPUVoltage;

                case HardwareMetricKey.GPUTemp:
                    return Resources.GPUTemp;

                case HardwareMetricKey.GPUFan:
                    return Resources.GPUFan;

                case HardwareMetricKey.GPUPower:
                    return Resources.GPUPower;

                case HardwareMetricKey.NetworkIP:
                    return Resources.NetworkIP;

                case HardwareMetricKey.NetworkExtIP:
                    return Resources.NetworkExtIP;

                case HardwareMetricKey.NetworkIn:
                    return Resources.NetworkIn;

                case HardwareMetricKey.NetworkOut:
                    return Resources.NetworkOut;

                case HardwareMetricKey.DriveLoadBar:
                    return Resources.DriveLoadBar;

                case HardwareMetricKey.DriveLoad:
                    return Resources.DriveLoad;

                case HardwareMetricKey.DriveUsed:
                    return Resources.DriveUsed;

                case HardwareMetricKey.DriveFree:
                    return Resources.DriveFree;

                case HardwareMetricKey.DriveRead:
                    return Resources.DriveRead;

                case HardwareMetricKey.DriveWrite:
                    return Resources.DriveWrite;

                case HardwareMetricKey.DriveTemp:
                    return Resources.DriveTemperature;

                case HardwareMetricKey.FanSpeed:
                    return Resources.FanSpeed;

                case HardwareMetricKey.PowerMonitorTemp:
                    return Resources.PowerMonitorTemp;

                case HardwareMetricKey.PowerMonitorPinVoltage:
                    return Resources.PowerMonitorPinVoltage;

                case HardwareMetricKey.PowerMonitorTotalCurrent:
                    return Resources.PowerMonitorTotalCurrent;

                case HardwareMetricKey.PowerMonitorPinCurrent:
                    return Resources.PowerMonitorPinCurrent;

                case HardwareMetricKey.PowerMonitorPower:
                    return Resources.PowerMonitorPower;

                case HardwareMetricKey.PowerMonitorFan:
                    return Resources.PowerMonitorFan;

                default:
                    return "Unknown";
            }
        }

        public static string GetLabel(HardwareMetricKey key)
        {
            switch (key)
            {
                case HardwareMetricKey.CPUClock:
                    return Resources.CPUClockLabel;

                case HardwareMetricKey.CPUTemp:
                    return Resources.CPUTempLabel;

                case HardwareMetricKey.CPUVoltage:
                    return Resources.CPUVoltageLabel;

                case HardwareMetricKey.CPUFan:
                    return Resources.CPUFanLabel;

                case HardwareMetricKey.CPULoad:
                    return Resources.CPULoadLabel;

                case HardwareMetricKey.CPUCoreLoad:
                    return Resources.CPUCoreLoadLabel;

                case HardwareMetricKey.CPUPower:
                    return Resources.CPUPowerLabel;

                case HardwareMetricKey.RAMClock:
                    return Resources.RAMClockLabel;

                case HardwareMetricKey.RAMVoltage:
                    return Resources.RAMVoltageLabel;

                case HardwareMetricKey.RAMLoad:
                    return Resources.RAMLoadLabel;

                case HardwareMetricKey.RAMUsed:
                    return Resources.RAMUsedLabel;

                case HardwareMetricKey.RAMFree:
                    return Resources.RAMFreeLabel;

                case HardwareMetricKey.RAMTemp:
                    return Resources.RAMTempLabel;

                case HardwareMetricKey.GPUCoreClock:
                    return Resources.GPUCoreClockLabel;

                case HardwareMetricKey.GPUVRAMClock:
                    return Resources.GPUVRAMClockLabel;

                case HardwareMetricKey.GPUCoreLoad:
                    return Resources.GPUCoreLoadLabel;

                case HardwareMetricKey.GPUVRAMLoad:
                    return Resources.GPUVRAMLoadLabel;

                case HardwareMetricKey.GPUVoltage:
                    return Resources.GPUVoltageLabel;

                case HardwareMetricKey.GPUTemp:
                    return Resources.GPUTempLabel;

                case HardwareMetricKey.GPUFan:
                    return Resources.GPUFanLabel;

                case HardwareMetricKey.GPUPower:
                    return Resources.GPUPowerLabel;

                case HardwareMetricKey.NetworkIP:
                    return Resources.NetworkIPLabel;

                case HardwareMetricKey.NetworkExtIP:
                    return Resources.NetworkExtIPLabel;

                case HardwareMetricKey.NetworkIn:
                    return Resources.NetworkInLabel;

                case HardwareMetricKey.NetworkOut:
                    return Resources.NetworkOutLabel;

                case HardwareMetricKey.DriveLoadBar:
                    return Resources.DriveLoadBarLabel;

                case HardwareMetricKey.DriveLoad:
                    return Resources.DriveLoadLabel;

                case HardwareMetricKey.DriveUsed:
                    return Resources.DriveUsedLabel;

                case HardwareMetricKey.DriveFree:
                    return Resources.DriveFreeLabel;

                case HardwareMetricKey.DriveRead:
                    return Resources.DriveReadLabel;

                case HardwareMetricKey.DriveWrite:
                    return Resources.DriveWriteLabel;

                case HardwareMetricKey.DriveTemp:
                    return Resources.DriveTemperatureLabel;

                case HardwareMetricKey.FanSpeed:
                    return Resources.FanSpeedLabel;

                case HardwareMetricKey.PowerMonitorTemp:
                    return Resources.PowerMonitorTempLabel;

                case HardwareMetricKey.PowerMonitorPinVoltage:
                    return Resources.PowerMonitorPinVoltageLabel;

                case HardwareMetricKey.PowerMonitorTotalCurrent:
                    return Resources.PowerMonitorTotalCurrentLabel;

                case HardwareMetricKey.PowerMonitorPinCurrent:
                    return Resources.PowerMonitorPinCurrentLabel;

                case HardwareMetricKey.PowerMonitorPower:
                    return Resources.PowerMonitorPowerLabel;

                case HardwareMetricKey.PowerMonitorFan:
                    return Resources.PowerMonitorFanLabel;

                default:
                    return "Unknown";
            }
        }

        public static string GetDataTypeAppendix(DataType type)
        {
            switch (type)
            {
                case DataType.Bit:
                    return " b";

                case DataType.Kilobit:
                    return " kb";

                case DataType.Megabit:
                    return " mb";

                case DataType.Gigabit:
                    return " gb";

                case DataType.Byte:
                    return " B";

                case DataType.Kilobyte:
                    return " KB";

                case DataType.Megabyte:
                    return " MB";

                case DataType.Gigabyte:
                    return " GB";

                case DataType.MHz:
                    return " MHz";

                case DataType.GHz:
                    return " GHz";

                case DataType.Voltage:
                    return " V";

                case DataType.Percent:
                    return "%";

                case DataType.RPM:
                    return " RPM";

                case DataType.Celsius:
                    return " °C";

                case DataType.Fahrenheit:
                    return " °F";

                case DataType.IP:
                    return string.Empty;

                case DataType.Watt:
                    return " W";

                case DataType.Ampere:
                    return " A";

                case DataType.BytePerSecond:
                    return " B/s";

                case DataType.KiloBytePerSecond:
                    return " KB/s";

                case DataType.MegaBytePerSecond:
                    return " MB/s";

                case DataType.GigaBytePerSecond:
                    return " GB/s";

                case DataType.TeraBytePerSecond:
                    return " TB/s";

                case DataType.BitPerSecond:
                    return " b/s";

                case DataType.KiloBitPerSecond:
                    return " Kb/s";

                case DataType.MegaBitPerSecond:
                    return " Mb/s";

                case DataType.GigaBitPerSecond:
                    return " Gb/s";

                case DataType.TeraBitPerSecond:
                    return " Tb/s";

                default:
                    throw new ArgumentException($"Invalid {nameof(DataType)}.");
            }
        }
    }
}
