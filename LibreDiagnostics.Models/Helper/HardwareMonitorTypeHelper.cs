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
using LibreHardwareMonitor.Hardware;

namespace LibreDiagnostics.Models.Helper
{
    public class HardwareMonitorTypeHelper
    {
        #region Public

        public static IEnumerable<HardwareType> GetHardwareTypes(HardwareMonitorType type)
        {
            switch (type)
            {
                case HardwareMonitorType.CPU:
                    yield return HardwareType.Cpu;
                    break;
                case HardwareMonitorType.RAM:
                    yield return HardwareType.Memory;
                    break;
                case HardwareMonitorType.GPU:
                    yield return HardwareType.GpuNvidia;
                    yield return HardwareType.GpuAmd;
                    yield return HardwareType.GpuIntel;
                    break;
                case HardwareMonitorType.Storage:
                    yield return HardwareType.Storage;
                    break;
                case HardwareMonitorType.Network:
                    yield return HardwareType.Network;
                    break;
                case HardwareMonitorType.Fan:
                    yield return HardwareType.SuperIO;
                    yield return HardwareType.Cooler;
                    break;
                default:
                    throw new ArgumentException($"Invalid {nameof(HardwareMonitorType)}: '{type}'.");
            }
        }

        public static string GetDescription(HardwareMonitorType type)
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

                default:
                    throw new ArgumentException($"Invalid {nameof(HardwareMonitorType)}: '{type}'.");
            }
        }

        #endregion
    }
}
