/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using LibreDiagnostics.Models.Configuration;
using LibreDiagnostics.Models.Helper;
using LibreDiagnostics.Models.Interfaces;
using LibreHardwareMonitor.Hardware;

namespace LibreDiagnostics.Models.Hardware.HardwareMonitor
{
    public static class HardwareMonitorLoader
    {
        #region Public

        public static List<HardwareMonitorCPU> GetHardwareMonitorsCPU(Computer computer, IHardware board, HardwareMonitorConfig hardwareMonitorConfig)
        {
            var hardwareTypes = HardwareMonitorTypeHelper.GetHardwareTypes(hardwareMonitorConfig.HardwareMonitorType);

            var hardwareList = computer.Hardware.Where(hw => hardwareTypes.Contains(hw.HardwareType)).ToList();

            return GetHardware<HardwareMonitorCPU>(hardwareList, hardwareMonitorConfig, (hw, hc) => new(hw, board, hc));
        }

        public static List<HardwareMonitorGPU> GetHardwareMonitorsGPU(Computer computer, HardwareMonitorConfig hardwareMonitorConfig)
        {
            var hardwareTypes = HardwareMonitorTypeHelper.GetHardwareTypes(hardwareMonitorConfig.HardwareMonitorType);

            var hardwareList = computer.Hardware.Where(hw => hardwareTypes.Contains(hw.HardwareType)).ToList();

            return GetHardware<HardwareMonitorGPU>(hardwareList, hardwareMonitorConfig, (hw, hc) => new(hw, hc));
        }

        public static List<HardwareMonitorRAM> GetHardwareMonitorsRAM(Computer computer, IHardware board, HardwareMonitorConfig hardwareMonitorConfig)
        {
            var hardwareTypes = HardwareMonitorTypeHelper.GetHardwareTypes(hardwareMonitorConfig.HardwareMonitorType);

            var hardwareList = computer.Hardware.Where(hw => hardwareTypes.Contains(hw.HardwareType)).ToList();

            return GetHardware<HardwareMonitorRAM>(hardwareList, hardwareMonitorConfig, (hw, hc) => new(hw, board, hc));
        }

        public static List<HardwareMonitorDrive> GetHardwareMonitorsDrive(Computer computer, HardwareMonitorConfig hardwareMonitorConfig)
        {
            var hardwareTypes = HardwareMonitorTypeHelper.GetHardwareTypes(hardwareMonitorConfig.HardwareMonitorType);

            var hardwareList = computer.Hardware.Where(hw => hardwareTypes.Contains(hw.HardwareType)).ToList();

            //Leave that commented code

            //Sort by drive letter, if exists
            //var hardwareSorted = hardwareList
            //    .OfType<StorageDevice>()
            //    .OrderBy(sd =>
            //    {
            //        //Get all drive letters for storage device
            //        var letters = sd.Storage.Partitions
            //            .Where(p => p.DriveLetter != null)
            //            .Select(p => p.DriveLetter.Value)
            //            .OrderBy(c => c)
            //            .ToList();
            //
            //        //Return first drive letter or max value if no drive letter exists
            //        return letters.Any() ? letters.First() : char.MaxValue;
            //    }).ToList();

            return GetHardware<HardwareMonitorDrive>(hardwareList, hardwareMonitorConfig, (hw, hc) => new(hw, hc));
        }

        public static List<HardwareMonitorNetwork> GetHardwareMonitorsNetwork(Computer computer, HardwareMonitorConfig hardwareMonitorConfig)
        {
            var hardwareTypes = HardwareMonitorTypeHelper.GetHardwareTypes(hardwareMonitorConfig.HardwareMonitorType);

            var hardwareList = computer.Hardware.Where(hw => hardwareTypes.Contains(hw.HardwareType)).ToList();

            return GetHardware<HardwareMonitorNetwork>(hardwareList, hardwareMonitorConfig, (hw, hc) => new(hw, hc));
        }

        public static List<HardwareMonitorFans> GetHardwareMonitorsFan(Computer computer, IHardware board, HardwareMonitorConfig hardwareMonitorConfig)
        {
            var hardwareTypes = HardwareMonitorTypeHelper.GetHardwareTypes(hardwareMonitorConfig.HardwareMonitorType);

            var hardwareList = computer.Hardware.Where(hw => hardwareTypes.Contains(hw.HardwareType)).ToList();
            hardwareList.AddRange(board.SubHardware.Where(hw => hardwareTypes.Contains(hw.HardwareType)).ToList());

            return GetHardware<HardwareMonitorFans>(hardwareList, hardwareMonitorConfig, (hw, hc) => new(hw, hc));
        }

        #endregion

        #region Private

        static List<THardware> GetHardware<THardware>(
                List<IHardware> rawHardwareList,
                HardwareMonitorConfig hardwareMonitorConfig,
                Func<IHardware, HardwareConfig, THardware> createHardware)
            where THardware : class, IHardwareMonitor
        {
            var hardwareList = new List<THardware>();
            foreach (var hw in rawHardwareList)
            {
                var match = hardwareMonitorConfig.HardwareConfig
                    .FirstOrDefault(hc => hc.ID == hw.Identifier.ToString());

                match ??= new()
                {
                    ID = hw.Identifier.ToString(),
                    Name = hw.Name,
                    ActualName = hw.Name,
                };

                if (match.ActualName != hw.Name)
                {
                    match.Name = match.ActualName = hw.Name;
                }

                hardwareList.Add(createHardware(hw, match));
            }

            return new
            (
                from hw in hardwareList
                orderby hw.Order ascending, hw.Name ascending
                select hw
            );
        }

        #endregion
    }
}
