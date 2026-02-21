/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.Core.Interfaces;
using BlackSharp.MVVM.ComponentModel;
using LibreDiagnostics.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LibreDiagnostics.Models.Platform
{
    public class ScreenModel : ViewModelBase, ICloneable<ScreenModel>
    {
        #region Properties

        string _Name;
        [JsonIgnore]
        public string Name
        {
            get { return _Name; }
            set { SetField(ref _Name, value); }
        }

        ScreenIdentificationStrategy _Strategy;
        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public ScreenIdentificationStrategy Strategy
        {
            get { return _Strategy; }
            set { SetField(ref _Strategy, value); }
        }

        string _ScreenID;
        [JsonProperty]
        public string ScreenID
        {
            get { return _ScreenID; }
            set { SetField(ref _ScreenID, value); }
        }

        #endregion

        #region Public

        public ScreenModel Clone()
        {
            return MemberwiseClone() as ScreenModel;
        }

        #endregion
    }
}
