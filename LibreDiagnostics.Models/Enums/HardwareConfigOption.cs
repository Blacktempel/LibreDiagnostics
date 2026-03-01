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
    public enum HardwareConfigOption : byte
    {
        //General / Shared
        HardwareNames     =  0,
        UseFahrenheit         ,
        RoundAll              ,
        TempAlert             ,
        DataRateUnit          ,

        //CPU
        AllCoreClocks     = 10,
        CoreLoads             ,

        //Storage
        ThrottleInterval  = 20,
        UsedSpaceAlert        ,
        ForceDriveWakeup      ,

        //Network
        BandwidthInAlert  = 30,
        BandwidthOutAlert     ,

        //Fans
        ShowInactiveFans  = 40,
    }
}
