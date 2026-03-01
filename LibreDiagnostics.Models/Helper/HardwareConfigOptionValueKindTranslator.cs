/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using LibreDiagnostics.Models.Enums;

namespace LibreDiagnostics.Models.Helper
{
    public class HardwareConfigOptionValueKindTranslator
    {
        public static HardwareConfigOptionValueKind GetValueKind(HardwareConfigOption hardwareConfigOption)
        {
            switch (hardwareConfigOption)
            {
                case HardwareConfigOption.HardwareNames:
                case HardwareConfigOption.UseFahrenheit:
                case HardwareConfigOption.RoundAll:
                case HardwareConfigOption.AllCoreClocks:
                case HardwareConfigOption.CoreLoads:
                case HardwareConfigOption.ForceDriveWakeup:
                case HardwareConfigOption.ShowInactiveFans:
                    return HardwareConfigOptionValueKind.Boolean;
                case HardwareConfigOption.TempAlert:
                case HardwareConfigOption.UsedSpaceAlert:
                    return HardwareConfigOptionValueKind.Int16;
                case HardwareConfigOption.ThrottleInterval:
                case HardwareConfigOption.BandwidthInAlert:
                case HardwareConfigOption.BandwidthOutAlert:
                    return HardwareConfigOptionValueKind.Int64;
                case HardwareConfigOption.DataRateUnit:
                    return HardwareConfigOptionValueKind.DataUnit;
                default:
                    return HardwareConfigOptionValueKind.None;
            }
        }
    }
}
