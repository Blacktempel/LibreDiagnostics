/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.Core.Converters.Enums;
using BlackSharp.Core.Interfaces;
using BlackSharp.Core.Reflection;
using BlackSharp.MVVM.ComponentModel;
using LibreDiagnostics.Language.Resources;
using LibreDiagnostics.Models.Configuration.Json;
using LibreDiagnostics.Models.Enums;
using LibreDiagnostics.Models.Helper;
using Newtonsoft.Json;

namespace LibreDiagnostics.Models.Configuration
{
    /// <summary>
    /// Hardware configuration options. See also <seealso cref="HardwareConfigOption"/>.
    /// </summary>
    public class HardwareConfigOptions : ViewModelBase, ICopyable<HardwareConfigOptions>, ICloneable<HardwareConfigOptions>
    {
        #region Properties

        HardwareConfigOption _Key;
        [JsonProperty]
        public HardwareConfigOption Key
        {
            get { return _Key; }
            set { SetField(ref _Key, value); }
        }

        object _Value;
        [JsonProperty]
        [JsonConverter(typeof(EnumTypeEnumValueJsonConverter))]
        public object Value
        {
            get { return _Value; }
            set { SetField(ref _Value, value); }
        }

        [JsonIgnore]
        public string Name
        {
            get { return GetName(); }
        }

        [JsonIgnore]
        public string Tooltip
        {
            get { return GetToolTip(); }
        }

        [JsonIgnore]
        public HardwareConfigOptionValueKind ValueKind
        {
            get { return HardwareConfigOptionValueKindTranslator.GetValueKind(Key); }
        }

        #endregion

        #region ICloneable

        public HardwareConfigOptions Clone()
        {
            var clone = (HardwareConfigOptions)MemberwiseClone();
            return clone;
        }

        #endregion

        #region ICopyable

        public void Copy(HardwareConfigOptions from)
        {
            ShallowCopy.CopyValueTypeProperties(from, this);
        }

        #endregion

        #region Public

        public string GetName()
        {
            switch (Key)
            {
                case HardwareConfigOption.HardwareNames:
                    return Resources.SettingsShowHardwareNames;
                case HardwareConfigOption.UseFahrenheit:
                    return Resources.SettingsUseFahrenheit;
                case HardwareConfigOption.RoundAll:
                    return Resources.SettingsRoundAllDecimals;
                case HardwareConfigOption.TempAlert:
                    return Resources.SettingsTemperatureAlert;
                case HardwareConfigOption.DataRateUnit:
                    return Resources.SettingsDataRateUnit;

                case HardwareConfigOption.AllCoreClocks:
                    return Resources.SettingsAllCoreClocks;
                case HardwareConfigOption.CoreLoads:
                    return Resources.SettingsCoreLoads;

                case HardwareConfigOption.ThrottleInterval:
                    return Resources.SettingsThrottleInterval;
                case HardwareConfigOption.UsedSpaceAlert:
                    return Resources.SettingsUsedSpaceAlert;

                case HardwareConfigOption.BandwidthInAlert:
                    return Resources.SettingsBandwidthInAlert;
                case HardwareConfigOption.BandwidthOutAlert:
                    return Resources.SettingsBandwidthOutAlert;

                case HardwareConfigOption.ShowInactiveFans:
                    return Resources.SettingsShowInactiveFans;

                default:
                    return "Unknown";
            }
        }

        public string GetToolTip()
        {
            switch (Key)
            {
                case HardwareConfigOption.HardwareNames:
                    return Resources.SettingsShowHardwareNamesTooltip;
                case HardwareConfigOption.UseFahrenheit:
                    return Resources.SettingsUseFahrenheitTooltip;
                case HardwareConfigOption.RoundAll:
                    return Resources.SettingsRoundAllDecimalsTooltip;
                case HardwareConfigOption.TempAlert:
                    return Resources.SettingsTemperatureAlertTooltip;
                case HardwareConfigOption.DataRateUnit:
                    return Resources.SettingsDataRateUnitTooltip;

                case HardwareConfigOption.AllCoreClocks:
                    return Resources.SettingsAllCoreClocksTooltip;
                case HardwareConfigOption.CoreLoads:
                    return Resources.SettingsCoreLoadsTooltip;

                case HardwareConfigOption.ThrottleInterval:
                    return Resources.SettingsThrottleIntervalTooltip;
                case HardwareConfigOption.UsedSpaceAlert:
                    return Resources.SettingsUsedSpaceAlertTooltip;

                case HardwareConfigOption.BandwidthInAlert:
                    return Resources.SettingsBandwidthInAlertTooltip;
                case HardwareConfigOption.BandwidthOutAlert:
                    return Resources.SettingsBandwidthOutAlertTooltip;

                case HardwareConfigOption.ShowInactiveFans:
                    return Resources.SettingsShowInactiveFansTooltip;

                default:
                    return "Unknown";
            }
        }

        #endregion

        #region Public - Mock Data

        public static List<HardwareConfigOptions> GetMock()
        {
            //if (!IsInDesignMode)
            //    return null;

            return new List<HardwareConfigOptions>
            {
                new HardwareConfigOptions
                {
                    Key = HardwareConfigOption.HardwareNames,
                    Value = true,
                },
                new HardwareConfigOptions
                {
                    Key = HardwareConfigOption.RoundAll,
                    Value = true,
                },
            };
        }

        #endregion

        #region Default Values

        public static class Defaults
        {
            #region Properties

            //General / Shared
            public static HardwareConfigOptions HardwareNames     { get { return GetDefaultConfig(HardwareConfigOption.HardwareNames    , true             ); } }
            public static HardwareConfigOptions UseFahrenheit     { get { return GetDefaultConfig(HardwareConfigOption.UseFahrenheit    , false            ); } }
            public static HardwareConfigOptions RoundAll          { get { return GetDefaultConfig(HardwareConfigOption.RoundAll         , false            ); } }
            public static HardwareConfigOptions TempAlert         { get { return GetDefaultConfig(HardwareConfigOption.TempAlert        , 0                ); } }
            public static HardwareConfigOptions DataRateUnit      { get { return GetDefaultConfig(HardwareConfigOption.DataRateUnit     , DataUnit.MegaByte); } }

            //CPU
            public static HardwareConfigOptions AllCoreClocks     { get { return GetDefaultConfig(HardwareConfigOption.AllCoreClocks    , false            ); } }

            //Storage
            public static HardwareConfigOptions ThrottleInterval  { get { return GetDefaultConfig(HardwareConfigOption.ThrottleInterval , 0                ); } }
            public static HardwareConfigOptions UsedSpaceAlert    { get { return GetDefaultConfig(HardwareConfigOption.UsedSpaceAlert   , 0                ); } }

            //Network
            public static HardwareConfigOptions BandwidthInAlert  { get { return GetDefaultConfig(HardwareConfigOption.BandwidthInAlert , 0                ); } }
            public static HardwareConfigOptions BandwidthOutAlert { get { return GetDefaultConfig(HardwareConfigOption.BandwidthOutAlert, 0                ); } }

            //Fans
            public static HardwareConfigOptions ShowInactiveFans  { get { return GetDefaultConfig(HardwareConfigOption.ShowInactiveFans , false            ); } }

            #endregion

            #region Private

            static HardwareConfigOptions GetDefaultConfig(HardwareConfigOption option, object value) =>
                new() { Key = option, Value = value };

            #endregion
        }

        #endregion
    }
}
