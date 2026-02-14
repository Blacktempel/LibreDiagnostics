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
    public enum HardwareMetricKey : byte
    {
        CPUClock    = 0,
        CPUTemp        ,
        CPUVoltage     ,
        CPUFan         ,
        CPULoad        ,
        CPUCoreLoad    ,
        CPUPower       ,

        RAMClock   = 10,
        RAMVoltage     ,
        RAMLoad        ,
        RAMUsed        ,
        RAMFree        ,
        RAMTemp        ,

        GPUCoreClock = 20,
        GPUVRAMClock     ,
        GPUCoreLoad      ,
        GPUVRAMLoad      ,
        GPUVoltage       ,
        GPUTemp          ,
        GPUFan           ,
        GPUPower         ,

        NetworkIP    = 30,
        NetworkExtIP     ,
        NetworkIn        ,
        NetworkOut       ,

        DriveLoadBar = 40,
        DriveLoad        ,
        DriveUsed        ,
        DriveFree        ,
        DriveRead        ,
        DriveWrite       ,
        DriveTemp        ,

        FanSpeed     = 50,

        PowerMonitorTemp         = 60,
        PowerMonitorPinVoltage       ,
        PowerMonitorTotalCurrent     ,
        PowerMonitorPinCurrent       ,
        PowerMonitorPower            ,
        PowerMonitorFan              ,
    }
}
