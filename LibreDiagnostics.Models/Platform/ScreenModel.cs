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
using Newtonsoft.Json;

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

        int _ScreenIndex;
        [JsonProperty]
        public int ScreenIndex
        {
            get { return _ScreenIndex; }
            set { SetField(ref _ScreenIndex, value); }
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
