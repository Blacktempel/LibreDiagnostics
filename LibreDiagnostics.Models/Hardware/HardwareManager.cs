/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.Core.Asynchronous;
using BlackSharp.Core.Collections;
using BlackSharp.Core.Converters;
using BlackSharp.Core.Logging;
using BlackSharp.MVVM.ComponentModel;
using LibreDiagnostics.Models.Configuration;
using LibreDiagnostics.Models.Enums;
using LibreDiagnostics.Models.Globals;
using LibreDiagnostics.Models.Hardware.HardwareMonitor;
using LibreDiagnostics.Models.Helper;
using LibreDiagnostics.Models.Logging;
using LibreHardwareMonitor.Hardware;

namespace LibreDiagnostics.Models.Hardware
{
    public class HardwareManager : ViewModelBase, IDisposable
    {
        #region Constructor

        public HardwareManager()
        {
            Global.SettingsChanged += OnSettingsChanged;

            //Initial call to apply settings
            OnSettingsChanged(this, new(Global.Settings));
        }

        ~HardwareManager()
        {
            Dispose();
        }

        #endregion

        #region Fields

        bool _Disposed = false;

        Computer _Computer;
        IHardware _Board;

        object _HardwarePanelsLock = new();

        DateTime _LastLogWrite = DateTime.MinValue;
        TimeSpan _LogInterval = TimeSpan.FromSeconds(30);

        #endregion

        #region Properties

        ObservableCollectionEx<HardwarePanel> _HardwarePanels;
        public ObservableCollectionEx<HardwarePanel> HardwarePanels
        {
            get { return _HardwarePanels; }
            set { SetField(ref _HardwarePanels, value); }
        }

        #endregion

        #region Public

        public void Update()
        {
            //Save log file periodically
            if (DateTime.Now - _LastLogWrite >= _LogInterval)
            {
                LoggerUtilities.SaveLogFile();
            }

            UpdateBoard();

            using (var guard = new LockGuard(_HardwarePanelsLock))
            {
                HardwarePanels.ForEach(hp => hp.Hardware.ForEach(hm => hm.Update()));
            }

            UpdateMemorySize();
        }

        public List<HardwareConfig> GetHardware(HardwareMonitorType type)
        {
            List<HardwareConfig> getHardware(HardwareMonitorType type)
            {
                return GetComputerHardware(HardwareMonitorTypeHelper.GetHardwareTypes(type).ToArray())
                    .Select(h => new HardwareConfig
                    {
                        ID = h.Identifier.ToString(),
                        Name = h.Name,
                        ActualName = h.Name
                    }).ToList();
            }

            switch (type)
            {
                case HardwareMonitorType.CPU:
                case HardwareMonitorType.RAM:
                case HardwareMonitorType.GPU:
                case HardwareMonitorType.Storage:
                case HardwareMonitorType.Network:
                case HardwareMonitorType.PowerMonitor:
                    return getHardware(type);
                case HardwareMonitorType.Fan:
                    var hwComputer = getHardware(type);
                    var hwBoard = GetBoardHardware(HardwareMonitorTypeHelper.GetHardwareTypes(type).ToArray())
                        .Select(h => new HardwareConfig
                        {
                            ID = h.Identifier.ToString(),
                            Name = h.Name,
                            ActualName = h.Name
                        }).ToList();

                    return hwComputer.Concat(hwBoard).ToList();
                default:
                    throw new ArgumentException($"Invalid {nameof(HardwareMonitorType)}.");
            }
        }

        public void Dispose()
        {
            if (!_Disposed)
            {
                _Computer.Close();
            }
        }

        /// <summary>
        /// <inheritdoc cref="Computer.GetReport"/>
        /// </summary>
        /// <returns><inheritdoc cref="Computer.GetReport"/></returns>
        public string GetReport()
        {
            return _Computer?.GetReport();
        }

        #endregion

        #region Private

        void UpdateBoard()
        {
            _Board.Update();

            _Board.SubHardware?.ToList().ForEach(h => h.Update());
        }

        IEnumerable<IHardware> GetComputerHardware(params HardwareType[] types)
        {
            return _Computer.Hardware.Where(h => types.Contains(h.HardwareType));
        }

        IEnumerable<IHardware> GetBoardHardware(params HardwareType[] types)
        {
            return _Board.SubHardware.Where(h => types.Contains(h.HardwareType));
        }

        HardwarePanel CreatePanel(HardwareMonitorConfig hardwareMonitorConfig)
        {
            var monType = HardwareHardwareMetricKeyTranslator.GetMonitorType(hardwareMonitorConfig.HardwareMonitorType);

            switch (hardwareMonitorConfig.HardwareMonitorType)
            {
                case HardwareMonitorType.CPU:
                    return new HardwarePanel(hardwareMonitorConfig.HardwareMonitorType, IconData.CPU         , monType, HardwareMonitorLoader.GetHardwareMonitorsCPU(_Computer, _Board, hardwareMonitorConfig));
                case HardwareMonitorType.RAM:                
                    return new HardwarePanel(hardwareMonitorConfig.HardwareMonitorType, IconData.RAM         , monType, HardwareMonitorLoader.GetHardwareMonitorsRAM(_Computer, _Board, hardwareMonitorConfig));
                case HardwareMonitorType.GPU:                
                    return new HardwarePanel(hardwareMonitorConfig.HardwareMonitorType, IconData.GPU         , monType, HardwareMonitorLoader.GetHardwareMonitorsGPU(_Computer, hardwareMonitorConfig));
                case HardwareMonitorType.Storage:
                    return new HardwarePanel(hardwareMonitorConfig.HardwareMonitorType, IconData.Drives      , monType, HardwareMonitorLoader.GetHardwareMonitorsDrive(_Computer, hardwareMonitorConfig));
                case HardwareMonitorType.Network:
                    return new HardwarePanel(hardwareMonitorConfig.HardwareMonitorType, IconData.Network     , monType, HardwareMonitorLoader.GetHardwareMonitorsNetwork(_Computer, hardwareMonitorConfig));
                case HardwareMonitorType.Fan:
                    return new HardwarePanel(hardwareMonitorConfig.HardwareMonitorType, IconData.Fan         , monType, HardwareMonitorLoader.GetHardwareMonitorsFan(_Computer, _Board, hardwareMonitorConfig));
                case HardwareMonitorType.PowerMonitor:
                    return new HardwarePanel(hardwareMonitorConfig.HardwareMonitorType, IconData.PowerMonitor, monType, HardwareMonitorLoader.GetHardwareMonitorsPowerMonitor(_Computer, hardwareMonitorConfig));
                default:
                    throw new ArgumentException($"Invalid {nameof(HardwareMonitorType)}.");
            }
        }

        void OnSettingsChanged(object sender, SettingsChangedEventArgs e)
        {
            //Already running - compare if anything has changed and apply differences
            if (e.NewSettings != null && _Computer != null)
            {
                //Adjust order of panels if necessary
                using (var guard = new LockGuard(_HardwarePanelsLock))
                {
                    for (int i = 0; i < e.NewSettings.HardwareMonitorConfigs.Count; ++i)
                    {
                        var hmc = e.NewSettings.HardwareMonitorConfigs[i];

                        var index = HardwarePanels.FindIndex(hp => hp.HardwareMonitorType == hmc.HardwareMonitorType);

                        //In config might be more than we show
                        if (i > index)
                        {
                            continue;
                        }

                        if (index >= 0 && i != index)
                        {
                            HardwarePanels.Move(index, i);
                        }
                    }
                }

                foreach (var cfg in e.NewSettings.HardwareMonitorConfigs)
                {
                    var addOrRemoveConfig = new Action<HardwareMonitorConfig>(cgf =>
                    {
                        using (var guard = new LockGuard(_HardwarePanelsLock))
                        {
                            //Create new panel to use new sensors
                            if (cfg.Enabled)
                            {
                                var order = cfg.Order > HardwarePanels.Count ? (byte)HardwarePanels.Count : cfg.Order;

                                HardwarePanels.TryInsert(order, CreatePanel(cfg));
                            }
                            else //Remove panel
                            {
                                HardwarePanels.Remove(hp => hp.HardwareMonitorType == cfg.HardwareMonitorType);
                            }
                        }
                    });

                    switch (cfg.HardwareMonitorType)
                    {
                        case HardwareMonitorType.CPU:
                            if (_Computer.IsCpuEnabled != cfg.Enabled)
                            {
                                _Computer.IsCpuEnabled = cfg.Enabled;
                                addOrRemoveConfig(cfg);
                            }
                            break;
                        case HardwareMonitorType.RAM:
                            if (_Computer.IsMemoryEnabled != cfg.Enabled)
                            {
                                _Computer.IsMemoryEnabled = cfg.Enabled;
                                addOrRemoveConfig(cfg);
                            }
                            break;
                        case HardwareMonitorType.GPU:
                            if (_Computer.IsGpuEnabled != cfg.Enabled)
                            {
                                _Computer.IsGpuEnabled = cfg.Enabled;
                                addOrRemoveConfig(cfg);
                            }
                            break;
                        case HardwareMonitorType.Storage:
                            if (_Computer.IsStorageEnabled != cfg.Enabled)
                            {
                                _Computer.HardwareAdded   -= OnStorageAdded;
                                _Computer.HardwareRemoved -= OnStorageRemoved;

                                _Computer.IsStorageEnabled = cfg.Enabled;

                                _Computer.HardwareAdded   += OnStorageAdded;
                                _Computer.HardwareRemoved += OnStorageRemoved;

                                addOrRemoveConfig(cfg);
                            }
                            break;
                        case HardwareMonitorType.Network:
                            if (_Computer.IsNetworkEnabled != cfg.Enabled)
                            {
                                _Computer.IsNetworkEnabled = cfg.Enabled;
                                addOrRemoveConfig(cfg);
                            }
                            break;
                        case HardwareMonitorType.Fan:
                            var cfgOldSettings = e.OldSettings.HardwareMonitorConfigs
                                .FirstOrDefault(hmc => hmc.HardwareMonitorType == HardwareMonitorType.Fan);

                            //Changed ?
                            if (cfgOldSettings.Enabled != cfg.Enabled)
                            {
                                addOrRemoveConfig(cfg);
                            }
                            break;
                        case HardwareMonitorType.PowerMonitor:
                            if (_Computer.IsPowerMonitorEnabled != cfg.Enabled)
                            {
                                _Computer.IsPowerMonitorEnabled = cfg.Enabled;
                                addOrRemoveConfig(cfg);
                            }
                            break;
                        default:
                            break;
                    }

                    ApplyHardwareConfigChanges(cfg);
                }
            }
            else //Fresh start
            {
                if (e.NewSettings == null)
                {
                    throw new ArgumentNullException(nameof(e.NewSettings));
                }

                _Computer = new Computer()
                {
                    IsMotherboardEnabled  = true,
                    IsCpuEnabled          = e.NewSettings.IsMonitorEnabled(HardwareMonitorType.CPU         ),
                    IsControllerEnabled   = e.NewSettings.IsMonitorEnabled(HardwareMonitorType.Fan         ),
                    IsGpuEnabled          = e.NewSettings.IsMonitorEnabled(HardwareMonitorType.GPU         ),
                    IsStorageEnabled      = e.NewSettings.IsMonitorEnabled(HardwareMonitorType.Storage     ),
                    IsMemoryEnabled       = e.NewSettings.IsMonitorEnabled(HardwareMonitorType.RAM         ),
                    IsNetworkEnabled      = e.NewSettings.IsMonitorEnabled(HardwareMonitorType.Network     ),
                    IsPowerMonitorEnabled = e.NewSettings.IsMonitorEnabled(HardwareMonitorType.PowerMonitor),
                };

                _Computer.Open();

                _Computer.HardwareAdded   += OnStorageAdded;
                _Computer.HardwareRemoved += OnStorageRemoved;

                _Board = GetComputerHardware(HardwareType.Motherboard).FirstOrDefault();

                UpdateBoard();

                var panels = e.NewSettings.HardwareMonitorConfigs
                                .Where(hmc => hmc.Enabled)
                                .OrderBy(hmc => hmc.Order)
                                .Select(CreatePanel)
                                .ToList();

                using (var guard = new LockGuard(_HardwarePanelsLock))
                {
                    HardwarePanels = new ObservableCollectionEx<HardwarePanel>(panels);
                }

                e.NewSettings.HardwareMonitorConfigs.ForEach(ApplyHardwareConfigChanges);
            }
        }

        void ApplyHardwareConfigChanges(HardwareMonitorConfig cfg)
        {
            if (HardwarePanels == null || cfg?.HardwareOC == null)
            {
                return;
            }

            using (var guard = new LockGuard(_HardwarePanelsLock))
            {
                var panel = HardwarePanels.FirstOrDefault(hp => hp.HardwareMonitorType == cfg.HardwareMonitorType);
                if (panel == null)
                {
                    return;
                }

                if (panel.Hardware.Count == cfg.HardwareOC?.Count)
                {
                    //Update order of Hardware according to config
                    for (int i = 0; i < cfg.HardwareOC.Count; ++i)
                    {
                        var config = cfg.HardwareOC[i];

                        //Get current index of Hardware according to config
                        var index = panel.Hardware.FindIndex(hm => hm.ID == config.ID);
                        if (index >= 0 && i != index)
                        {
                            //Position in config has changed - move Hardware to new position
                            panel.Hardware.Move(index, i);
                        }
                    }

                    //Update names of Hardware, if changed
                    foreach (var hw in cfg.HardwareOC)
                    {
                        var found = panel.Hardware.FirstOrDefault(hm => hm.ID == hw.ID);
                        if (found != null && found.Name != hw.Name)
                        {
                            found.Name = hw.Name;
                        }
                    }

                    //Update order of Hardware
                    for (byte i = 0; i < panel.Hardware.Count; ++i)
                    {
                        panel.Hardware[i].Order = i;
                    }
                }

                foreach (var monitor in panel.Hardware)
                {
                    var matching = cfg.HardwareOC?.FirstOrDefault(hc => hc.ID == monitor.ID);
                    if (matching != null)
                    {
                        monitor.Enabled = matching.Enabled;
                    }
                }
            }
        }

        void OnStorageAdded(IHardware hardware)
        {
            if (hardware.HardwareType != HardwareType.Storage)
            {
                return;
            }

            Logger.Instance.Add(LogLevel.Trace, $"{nameof(OnStorageAdded)}: '{hardware.Name}'", DateTime.Now);

            OnStoragesChanged();
        }

        void OnStorageRemoved(IHardware hardware)
        {
            if (hardware.HardwareType != HardwareType.Storage)
            {
                return;
            }

            Logger.Instance.Add(LogLevel.Trace, $"{nameof(OnStorageRemoved)}: '{hardware.Name}'", DateTime.Now);

            OnStoragesChanged();
        }

        void OnStoragesChanged()
        {
            var cfg = Global.Settings.HardwareMonitorConfigs.Find(hmc => hmc.HardwareMonitorType == HardwareMonitorType.Storage);

            if (cfg != null && HardwarePanels != null)
            {
                using (var guard = new LockGuard(_HardwarePanelsLock))
                {
                    //Create temporary list to avoid multiple notifications and possible UI duplication of a panel
                    //Notify events are not required for temp list
                    var tempList = new ObservableCollectionEx<HardwarePanel>(HardwarePanels) { AreNotifyEventsEnabled = false };

                    //Remove old panel
                    tempList.Remove(hp => hp.HardwareMonitorType == cfg.HardwareMonitorType);

                    //Add new panel with updated hardware list
                    tempList.TryInsert(cfg.Order, CreatePanel(cfg));

                    //Assign updated list back to property
                    HardwarePanels = new(tempList);
                }
            }
        }

        void UpdateMemorySize()
        {
#if DEBUG
            var memory = Environment.WorkingSet;
            
            var memoryInMB = memory == 0 ? 0 : DataStorageSizeConverter.ByteToMegabyte((ulong)memory);

            Global.ProcessInformation.TotalMemorySize = memoryInMB;
#endif
        }

        #endregion
    }
}
