/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

namespace LibreDiagnostics.Models.Enums
{
    [Serializable]
    public enum HardwareMonitorType : byte
    {
        CPU,
        RAM,
        GPU,
        Storage,
        Network,
        Fan,
        PowerMonitor,
    }
}
