/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2026 Florian K.
*
*/

using BlackSharp.Core.Converters;
using BlackSharp.Core.Converters.Enums;
using LibreDiagnostics.Models.Interfaces;

namespace LibreDiagnostics.Models.Converter
{
    public sealed class DataRateUnitConverter : IValueConverter
    {
        #region Constructor

        public DataRateUnitConverter(DataUnit sourceUnit, DataUnit targetUnit)
        {
            SourceUnit = sourceUnit;
            TargetUnit = targetUnit;
        }

        #endregion

        #region Properties

        public DataUnit SourceUnit { get; set; }

        public DataUnit TargetUnit { get; set; }

        #endregion

        #region IValueConverter

        public double Convert(double value)
        {
            return (double)DataUnitConverter.Convert((decimal)value, SourceUnit, TargetUnit);
        }

        public double ConvertBack(double value)
        {
            return (double)DataUnitConverter.Convert((decimal)value, TargetUnit, SourceUnit);
        }

        #endregion
    }
}
