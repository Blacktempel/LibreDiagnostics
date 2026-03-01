/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.Core.Collections;
using BlackSharp.Core.Interfaces;
using BlackSharp.Core.Reflection;
using BlackSharp.MVVM.ComponentModel;
using LibreDiagnostics.Models.Enums;
using LibreDiagnostics.Models.Globals;
using LibreDiagnostics.Models.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace LibreDiagnostics.Models.Configuration
{
    [JsonObject(MemberSerialization.OptIn)]
    public class HardwareMonitorConfig : DragDropViewModelBase, ICopyable<HardwareMonitorConfig>
    {
        #region Properties

        public static ObservableCollectionEx<HardwareMonitorConfig> Default
        {
            get { return GetDefault(); }
        }

        HardwareMonitorType _HardwareMonitorType;
        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public HardwareMonitorType HardwareMonitorType
        {
            get { return _HardwareMonitorType; }
            set { SetField(ref _HardwareMonitorType, value); }
        }

        public string Name
        {
            get { return HardwareMonitorTypeHelper.GetDescription(HardwareMonitorType); }
        }

        bool _Enabled;
        [JsonProperty]
        public bool Enabled
        {
            get { return _Enabled; }
            set { SetField(ref _Enabled, value); }
        }

        byte _Order;
        [JsonProperty]
        public byte Order
        {
            get { return _Order; }
            set { SetField(ref _Order, value); }
        }

        ObservableCollectionEx<HardwareConfig> _HardwareConfig;
        [JsonProperty]
        public ObservableCollectionEx<HardwareConfig> HardwareConfig
        {
            get { return _HardwareConfig; }
            set { SetField(ref _HardwareConfig, value); }
        }

        ObservableCollectionEx<HardwareConfig> _HardwareOC;
        public ObservableCollectionEx<HardwareConfig> HardwareOC
        {
            get { return _HardwareOC; }
            set { SetField(ref _HardwareOC, value); }
        }

        ObservableCollectionEx<MetricConfig> _MetricConfig;
        [JsonProperty]
        public ObservableCollectionEx<MetricConfig> MetricConfig
        {
            get { return _MetricConfig; }
            set { UpdateNotifiers(_MetricConfig, value); SetField(ref _MetricConfig, value); }
        }

        ObservableCollectionEx<HardwareConfigOptions> _HardwareConfigOptions;
        [JsonProperty]
        public ObservableCollectionEx<HardwareConfigOptions> HardwareConfigOptions
        {
            get { return _HardwareConfigOptions; }
            set { SetField(ref _HardwareConfigOptions, value); }
        }

        #endregion

        #region ICloneable

        public override HardwareMonitorConfig Clone()
        {
            var clone = (HardwareMonitorConfig)MemberwiseClone();

            clone.HardwareConfig        = new ObservableCollectionEx<HardwareConfig       >(HardwareConfig       .Select(hc => hc.Clone()));
            clone.MetricConfig          = new ObservableCollectionEx<MetricConfig         >(MetricConfig         .Select(mc => mc.Clone()));
            clone.HardwareConfigOptions = new ObservableCollectionEx<HardwareConfigOptions>(HardwareConfigOptions.Select(hc => hc.Clone()));

            if (HardwareOC != null)
            {
                clone.HardwareOC = new ObservableCollectionEx<HardwareConfig>(HardwareOC.Select(hc => hc.Clone()));
            }

            return clone;
        }

        #endregion

        #region ICopyable

        public void Copy(HardwareMonitorConfig from)
        {
            ShallowCopy.CopyValueTypeProperties(from, this);

            //Resolve differences in HardwareConfig
            CopyHardwareConfig(HardwareConfig, from.HardwareConfig);

            if (HardwareOC == null && from.HardwareOC != null)
            {
                HardwareOC = new();
            }

            //Resolve differences in HardwareOC
            CopyHardwareConfig(HardwareOC, from.HardwareOC);

            //Resolve differences in MetricConfig
            CopyMetricConfig(from);

            //Resolve differences in HardwareConfigOptions
            CopyHardwareConfigOptions(from);
        }

        static void CopyHardwareConfig(ObservableCollectionEx<HardwareConfig> hwOriginal, ObservableCollectionEx<HardwareConfig> hwCopyFrom)
        {
            //Resolve differences in HardwareOC
            hwCopyFrom
                .Where(hc => !hwOriginal
                    .Any(existing => existing.ID == hc.ID))
                .ToList()
                .ForEach(hwOriginal.Add);

            hwOriginal
                .Where(hc => !hwCopyFrom
                    .Any(existing => existing.ID == hc.ID))
                .ToList()
                .ForEach(hc => hwOriginal.Remove(hc));

            //Reorder items if necessary
            if (hwCopyFrom.Count == hwOriginal.Count)
            {
                for (int i = 0; i < hwCopyFrom.Count; ++i)
                {
                    var hw = hwCopyFrom[i];

                    var index = hwOriginal.FindIndex(hc => hc.ID == hw.ID);
                    if (index >= 0 && i != index)
                    {
                        hwOriginal.Move(index, i);
                    }
                }
            }

            //Copy list contents
            for (int i = 0; i < hwOriginal.Count; ++i)
            {
                var existing = hwOriginal[i];
                var source = hwCopyFrom
                                    .First(hmc => hmc.ID == existing.ID);
                existing.Copy(source);
            }
        }

        void CopyMetricConfig(HardwareMonitorConfig from)
        {
            //Resolve differences in MetricConfig
            from.MetricConfig
                .Where(hmc => !MetricConfig
                    .Any(existing => existing.HardwareMetricKey == hmc.HardwareMetricKey))
                .ToList()
                .ForEach(MetricConfig.Add);

            MetricConfig
                .Where(hmc => !from.MetricConfig
                    .Any(existing => existing.HardwareMetricKey == hmc.HardwareMetricKey))
                .ToList()
                .ForEach(hmc => MetricConfig.Remove(hmc));

            //Reorder items if necessary
            if (from.MetricConfig.Count == MetricConfig.Count)
            {
                for (int i = 0; i < from.MetricConfig.Count; ++i)
                {
                    var hw = from.MetricConfig[i];

                    var index = MetricConfig.FindIndex(hmc => hmc.HardwareMetricKey == hw.HardwareMetricKey);
                    if (index >= 0 && i != index)
                    {
                        MetricConfig.Move(index, i);
                    }
                }
            }

            //Copy list contents
            for (int i = 0; i < MetricConfig.Count; ++i)
            {
                var existing = MetricConfig[i];
                var source = from.MetricConfig
                                    .First(hmc => hmc.HardwareMetricKey == existing.HardwareMetricKey);
                existing.Copy(source);
            }
        }

        void CopyHardwareConfigOptions(HardwareMonitorConfig from)
        {
            //Resolve differences in HardwareConfigOptions
            from.HardwareConfigOptions
                .Where(hco => !HardwareConfigOptions
                    .Any(existing => existing.Key == hco.Key))
                .ToList()
                .ForEach(HardwareConfigOptions.Add);

            HardwareConfigOptions
                .Where(hco => !from.HardwareConfigOptions
                    .Any(existing => existing.Key == hco.Key))
                .ToList()
                .ForEach(hco => HardwareConfigOptions.Remove(hco));

            //Reorder items if necessary
            if (from.HardwareConfigOptions.Count == HardwareConfigOptions.Count)
            {
                for (int i = 0; i < from.HardwareConfigOptions.Count; ++i)
                {
                    var hw = from.HardwareConfigOptions[i];

                    var index = HardwareConfigOptions.FindIndex(hco => hco.Key == hw.Key);
                    if (index >= 0 && i != index)
                    {
                        HardwareConfigOptions.Move(index, i);
                    }
                }
            }

            //Copy list contents
            for (int i = 0; i < HardwareConfigOptions.Count; ++i)
            {
                var existing = HardwareConfigOptions[i];
                var source = from.HardwareConfigOptions
                                    .First(hco => hco.Key == existing.Key);
                existing.Copy(source);
            }
        }

        #endregion

        #region Public

        public static void NormalizeConfig(ObservableCollectionEx<HardwareMonitorConfig> hardwareMonitorConfigs)
        {
            var defaultConfig = Default;

            //Insert default config for missing hardware monitor types
            foreach (var defaultItem in defaultConfig)
            {
                if (!hardwareMonitorConfigs.Any(hmc => hmc.HardwareMonitorType == defaultItem.HardwareMonitorType))
                {
                    hardwareMonitorConfigs.Add(defaultItem.Clone());
                }
            }

            foreach (var hardwareMonitorConfig in hardwareMonitorConfigs)
            {
                var hmcRecord = hardwareMonitorConfigs.SingleOrDefault(hmc => hmc.HardwareMonitorType == hardwareMonitorConfig.HardwareMonitorType, hardwareMonitorConfig);

                if (hardwareMonitorConfig.HardwareConfig == null)
                {
                    hardwareMonitorConfig.HardwareConfig = hmcRecord.HardwareConfig;
                }

                if (hardwareMonitorConfig.MetricConfig == null)
                {
                    hardwareMonitorConfig.MetricConfig = hmcRecord.MetricConfig;
                }
                else
                {
                    var defaultRecord = defaultConfig.SingleOrDefault(hmc => hmc.HardwareMonitorType == hardwareMonitorConfig.HardwareMonitorType, hardwareMonitorConfig);

                    //Add any new default metrics that are missing in the current config (preserves new keys)
                    foreach (var defaultMetric in defaultRecord.MetricConfig)
                    {
                        if (!hardwareMonitorConfig.MetricConfig.Any(m => m.HardwareMetricKey == defaultMetric.HardwareMetricKey))
                        {
                            hardwareMonitorConfig.MetricConfig.Add(defaultMetric.Clone());
                        }
                    }

                    foreach (var metricConfig in hardwareMonitorConfig.MetricConfig)
                    {
                        var mcRecord = hmcRecord.MetricConfig.SingleOrDefault(m => m.HardwareMetricKey == metricConfig.HardwareMetricKey, metricConfig);

                        if (!ReferenceEquals(metricConfig, mcRecord))
                        {
                            metricConfig.Copy(mcRecord);
                        }
                    }
                }

                if (hardwareMonitorConfig.HardwareConfigOptions == null)
                {
                    hardwareMonitorConfig.HardwareConfigOptions = hmcRecord.HardwareConfigOptions;
                }
                else
                {
                    var defaultRecord = defaultConfig.SingleOrDefault(hmc => hmc.HardwareMonitorType == hardwareMonitorConfig.HardwareMonitorType, hardwareMonitorConfig);

                    //Add any new default options that are missing in the current config (preserves new keys)
                    foreach (var defaultOption in defaultRecord.HardwareConfigOptions)
                    {
                        if (!hardwareMonitorConfig.HardwareConfigOptions.Any(existing => existing.Key == defaultOption.Key))
                        {
                            hardwareMonitorConfig.HardwareConfigOptions.Add(defaultOption.Clone());
                        }
                    }

                    foreach (var hardwareConfigOption in hardwareMonitorConfig.HardwareConfigOptions)
                    {
                        var hcoRecord = hmcRecord.HardwareConfigOptions.SingleOrDefault(hc => hc.Key == hardwareConfigOption.Key, hardwareConfigOption);

                        if (!ReferenceEquals(hardwareConfigOption, hcoRecord))
                        {
                            hardwareConfigOption.Copy(hcoRecord);
                        }
                    }
                }
            }
        }

        public static void SyncHardwareConfigsWithDetectedHardware(Settings settings)
        {
            foreach (var item in settings.HardwareMonitorConfigs)
            {
                var hwList = new List<HardwareConfig>();

                foreach (var hw in Global.HardwareManager.GetHardware(item.HardwareMonitorType))
                {
                    var hwCfg = item.HardwareConfig.FirstOrDefault(hc => hc.ID == hw.ID, hw);

                    hwCfg.ActualName = hw.ActualName;

                    if (string.IsNullOrEmpty(hwCfg.Name))
                    {
                        hwCfg.Name = hw.ActualName;
                    }

                    hwList.Add(hwCfg);
                }

                item.HardwareOC = new
                (
                    from hw in hwList
                    orderby hw.Order ascending, hw.Name ascending
                    select hw
                );
            }
        }

        public static ObservableCollectionEx<HardwareMonitorConfig> GetDefault()
        {
            byte order = 0;

            return new()
            {
                new()
                {
                    HardwareMonitorType = HardwareMonitorType.CPU,
                    Enabled = true,
                    Order = order++,
                    HardwareConfig = new(),
                    MetricConfig = new()
                    {
                        new() { HardwareMetricKey = HardwareMetricKey.CPUClock   , Enabled = true  },
                        new() { HardwareMetricKey = HardwareMetricKey.CPUTemp    , Enabled = true  },
                        new() { HardwareMetricKey = HardwareMetricKey.CPUVoltage , Enabled = true  },
                        new() { HardwareMetricKey = HardwareMetricKey.CPUFan     , Enabled = true  },
                        new() { HardwareMetricKey = HardwareMetricKey.CPULoad    , Enabled = true  },
                        new() { HardwareMetricKey = HardwareMetricKey.CPUCoreLoad, Enabled = false },
                        new() { HardwareMetricKey = HardwareMetricKey.CPUPower   , Enabled = true  },
                    },
                    HardwareConfigOptions = new()
                    {
                        Configuration.HardwareConfigOptions.Defaults.HardwareNames,
                        Configuration.HardwareConfigOptions.Defaults.UseFahrenheit,
                        Configuration.HardwareConfigOptions.Defaults.RoundAll,
                        Configuration.HardwareConfigOptions.Defaults.AllCoreClocks,
                        Configuration.HardwareConfigOptions.Defaults.TempAlert,
                    }
                },
                new()
                {
                    HardwareMonitorType = HardwareMonitorType.RAM,
                    Enabled = true,
                    Order = order++,
                    HardwareConfig = new(),
                    MetricConfig = new()
                    {
                        new() { HardwareMetricKey = HardwareMetricKey.RAMClock  , Enabled = true },
                        new() { HardwareMetricKey = HardwareMetricKey.RAMVoltage, Enabled = true },
                        new() { HardwareMetricKey = HardwareMetricKey.RAMLoad   , Enabled = true },
                        new() { HardwareMetricKey = HardwareMetricKey.RAMUsed   , Enabled = true },
                        new() { HardwareMetricKey = HardwareMetricKey.RAMFree   , Enabled = true },
                        new() { HardwareMetricKey = HardwareMetricKey.RAMTemp   , Enabled = true },
                    },
                    HardwareConfigOptions = new()
                    {
                        Configuration.HardwareConfigOptions.Defaults.HardwareNames,
                        Configuration.HardwareConfigOptions.Defaults.UseFahrenheit,
                        Configuration.HardwareConfigOptions.Defaults.RoundAll,
                        Configuration.HardwareConfigOptions.Defaults.TempAlert,
                    }
                },
                new()
                {
                    HardwareMonitorType = HardwareMonitorType.GPU,
                    Enabled = true,
                    Order = order++,
                    HardwareConfig = new(),
                    MetricConfig = new()
                    {
                        new() { HardwareMetricKey = HardwareMetricKey.GPUCoreClock, Enabled = false },
                        new() { HardwareMetricKey = HardwareMetricKey.GPUVRAMClock, Enabled = false },
                        new() { HardwareMetricKey = HardwareMetricKey.GPUCoreLoad , Enabled = true  },
                        new() { HardwareMetricKey = HardwareMetricKey.GPUVRAMLoad , Enabled = true  },
                        new() { HardwareMetricKey = HardwareMetricKey.GPUVoltage  , Enabled = true  },
                        new() { HardwareMetricKey = HardwareMetricKey.GPUTemp     , Enabled = true  },
                        new() { HardwareMetricKey = HardwareMetricKey.GPUFan      , Enabled = true  },
                        new() { HardwareMetricKey = HardwareMetricKey.GPUPower    , Enabled = true  },
                    },
                    HardwareConfigOptions = new()
                    {
                        Configuration.HardwareConfigOptions.Defaults.HardwareNames,
                        Configuration.HardwareConfigOptions.Defaults.UseFahrenheit,
                        Configuration.HardwareConfigOptions.Defaults.RoundAll,
                        Configuration.HardwareConfigOptions.Defaults.TempAlert,
                    }
                },
                new()
                {
                    HardwareMonitorType = HardwareMonitorType.Storage,
                    Enabled = true,
                    Order = order++,
                    HardwareConfig = new(),
                    MetricConfig = new()
                    {
                        new MetricConfig() { HardwareMetricKey = HardwareMetricKey.DriveLoadBar, Enabled = true  },
                        new MetricConfig() { HardwareMetricKey = HardwareMetricKey.DriveLoad   , Enabled = true  },
                        new MetricConfig() { HardwareMetricKey = HardwareMetricKey.DriveUsed   , Enabled = true  },
                        new MetricConfig() { HardwareMetricKey = HardwareMetricKey.DriveFree   , Enabled = true  },
                        new MetricConfig() { HardwareMetricKey = HardwareMetricKey.DriveRead   , Enabled = false },
                        new MetricConfig() { HardwareMetricKey = HardwareMetricKey.DriveWrite  , Enabled = false },
                        new MetricConfig() { HardwareMetricKey = HardwareMetricKey.DriveTemp   , Enabled = true  },
                    },
                    HardwareConfigOptions = new()
                    {
                        Configuration.HardwareConfigOptions.Defaults.HardwareNames,
                        Configuration.HardwareConfigOptions.Defaults.UseFahrenheit,
                        Configuration.HardwareConfigOptions.Defaults.RoundAll,
                        Configuration.HardwareConfigOptions.Defaults.DataRateUnit,
                        Configuration.HardwareConfigOptions.Defaults.ThrottleInterval,
                        Configuration.HardwareConfigOptions.Defaults.TempAlert,
                        Configuration.HardwareConfigOptions.Defaults.UsedSpaceAlert,
                        Configuration.HardwareConfigOptions.Defaults.ForceDriveWakeup,
                    }
                },
                new()
                {
                    HardwareMonitorType = HardwareMonitorType.Network,
                    Enabled = false,
                    Order = order++,
                    HardwareConfig = new(),
                    MetricConfig = new()
                    {
                        new MetricConfig() { HardwareMetricKey = HardwareMetricKey.NetworkIP   , Enabled = true  },
                        //new MetricConfig() { HardwareMetricKey = HardwareMetricKey.NetworkExtIP, Enabled = false },
                        new MetricConfig() { HardwareMetricKey = HardwareMetricKey.NetworkIn   , Enabled = true  },
                        new MetricConfig() { HardwareMetricKey = HardwareMetricKey.NetworkOut  , Enabled = true  },
                    },
                    HardwareConfigOptions = new()
                    {
                        Configuration.HardwareConfigOptions.Defaults.HardwareNames,
                        Configuration.HardwareConfigOptions.Defaults.RoundAll,
                        Configuration.HardwareConfigOptions.Defaults.DataRateUnit,
                        Configuration.HardwareConfigOptions.Defaults.BandwidthInAlert,
                        Configuration.HardwareConfigOptions.Defaults.BandwidthOutAlert,
                    }
                },
                new()
                {
                    HardwareMonitorType = HardwareMonitorType.Fan,
                    Enabled = true,
                    Order = order++,
                    HardwareConfig = new(),
                    MetricConfig = new()
                    {
                        new() { HardwareMetricKey = HardwareMetricKey.FanSpeed, Enabled = true },
                    },
                    HardwareConfigOptions = new()
                    {
                        Configuration.HardwareConfigOptions.Defaults.HardwareNames,
                        Configuration.HardwareConfigOptions.Defaults.ShowInactiveFans,
                    }
                },
                new()
                {
                    HardwareMonitorType = HardwareMonitorType.PowerMonitor,
                    Enabled = false,
                    Order = order++,
                    HardwareConfig = new(),
                    MetricConfig = new()
                    {
                        new() { HardwareMetricKey = HardwareMetricKey.PowerMonitorTemp        , Enabled = true  },
                        new() { HardwareMetricKey = HardwareMetricKey.PowerMonitorPinVoltage  , Enabled = false },
                        new() { HardwareMetricKey = HardwareMetricKey.PowerMonitorTotalCurrent, Enabled = true  },
                        new() { HardwareMetricKey = HardwareMetricKey.PowerMonitorPinCurrent  , Enabled = false },
                        new() { HardwareMetricKey = HardwareMetricKey.PowerMonitorPower       , Enabled = true  },
                        new() { HardwareMetricKey = HardwareMetricKey.PowerMonitorFan         , Enabled = true  },
                    },
                    HardwareConfigOptions = new()
                    {
                        Configuration.HardwareConfigOptions.Defaults.HardwareNames,
                        Configuration.HardwareConfigOptions.Defaults.UseFahrenheit,
                        Configuration.HardwareConfigOptions.Defaults.RoundAll,
                        Configuration.HardwareConfigOptions.Defaults.TempAlert,
                    }
                },
            };
        }

        #endregion

        #region Private

        /// <summary>
        /// Workaround for notifying that a property inside of MetricConfig has changed, in order to re-trigger converter in UI upon selection changes
        /// </summary>
        /// <param name="oldItems"></param>
        /// <param name="newItems"></param>
        void UpdateNotifiers(ObservableCollectionEx<MetricConfig> oldItems, ObservableCollectionEx<MetricConfig> newItems)
        {
            if (oldItems != null)
            {
                oldItems.ForEach(item => item.PropertyChanged -= OnMetricConfigPropertyChanged);
            }

            if (newItems != null)
            {
                newItems.ForEach(item => item.PropertyChanged += OnMetricConfigPropertyChanged);
            }
        }

        void OnMetricConfigPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(MetricConfig));
        }

        #endregion

        #region Public - Mock Data

        public static List<HardwareMonitorConfig> GetMock()
        {
            //if (!IsInDesignMode)
            //    return null;

            return GetDefault().ToList();
        }

        #endregion
    }
}
