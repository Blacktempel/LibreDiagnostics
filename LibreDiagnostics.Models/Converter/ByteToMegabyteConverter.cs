/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.Core.Converters;
using BlackSharp.Core.Converters.Enums;
using LibreDiagnostics.Models.Interfaces;

namespace LibreDiagnostics.Models.Converter
{
    public sealed class ByteToMegabyteConverter : IValueConverter
    {
        #region IValueConverter

        public double Convert(double value)
        {
            return (double)DataUnitConverter.ToMegaByte((decimal)value, DataUnit.Byte);
        }

        public double ConvertBack(double value)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
