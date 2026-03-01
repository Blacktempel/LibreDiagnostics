/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2026 Florian K.
*
*/

using BlackSharp.Core.Converters.Enums;

namespace LibreDiagnostics.Models.Mappings
{
    public static class DataUnitMapping
    {
        #region Properties

        public static IReadOnlyList<Tuple<DataUnit, string> > Mapping { get; } = new List<Tuple<DataUnit, string> >
        {
            new(DataUnit.Byte      , "Byte"       ),
            new(DataUnit.KiloByte  , "KiloByte"   ),
            new(DataUnit.MegaByte  , "MegaByte"   ),
            new(DataUnit.GigaByte  , "GigaByte"   ),
            new(DataUnit.TeraByte  , "TeraByte"   ),
            //new(DataUnit.PetaByte  , "PetaByte"   ),
            //new(DataUnit.ExaByte   , "ExaByte"    ),
            //new(DataUnit.ZettaByte , "ZettaByte"  ),
            //new(DataUnit.YottaByte , "YottaByte"  ),
            //new(DataUnit.RonnaByte , "RonnaByte"  ),
            //new(DataUnit.QuettaByte, "QuettaByte" ),

            //new(DataUnit.KibiByte , "KibiByte"  ),
            //new(DataUnit.MebiByte , "MebiByte"  ),
            //new(DataUnit.GibiByte , "GibiByte"  ),
            //new(DataUnit.TebiByte , "TebiByte"  ),
            //new(DataUnit.PebiByte , "PebiByte"  ),
            //new(DataUnit.ExbiByte , "ExbiByte"  ),
            //new(DataUnit.ZebiByte , "ZebiByte"  ),
            //new(DataUnit.YobiByte , "YobiByte"  ),
            //new(DataUnit.RobiByte , "RobiByte"  ),
            //new(DataUnit.QuebiByte, "QuebiByte" ),

            new (DataUnit.Bit      , "Bit"       ),
            new (DataUnit.KiloBit  , "KiloBit"   ),
            new (DataUnit.MegaBit  , "MegaBit"   ),
            new (DataUnit.GigaBit  , "GigaBit"   ),
            new (DataUnit.TeraBit  , "TeraBit"   ),
            //new (DataUnit.PetaBit  , "PetaBit"   ),
            //new (DataUnit.ExaBit   , "ExaBit"    ),
            //new (DataUnit.ZettaBit , "ZettaBit"  ),
            //new (DataUnit.YottaBit , "YottaBit"  ),
            //new (DataUnit.RonnaBit , "RonnaBit"  ),
            //new (DataUnit.QuettaBit, "QuettaBit" ),

            //new(DataUnit.KibiBit , "KibiBit"  ),
            //new(DataUnit.MebiBit , "MebiBit"  ),
            //new(DataUnit.GibiBit , "GibiBit"  ),
            //new(DataUnit.TebiBit , "TebiBit"  ),
            //new(DataUnit.PebiBit , "PebiBit"  ),
            //new(DataUnit.ExbiBit , "ExbiBit"  ),
            //new(DataUnit.ZebiBit , "ZebiBit"  ),
            //new(DataUnit.YobiBit , "YobiBit"  ),
            //new(DataUnit.RobiBit , "RobiBit"  ),
            //new(DataUnit.QuebiBit, "QuebiBit" ),
        };

        #endregion
    }
}
