/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.MVVM.ComponentModel;
using LibreDiagnostics.Models.Globals;
using LibreDiagnostics.Models.Interfaces;

namespace LibreDiagnostics.Models.Software
{
    /// <summary>
    /// Clock model representing the current date and time.
    /// </summary>
    public class Clock : ViewModelBase, IIcon
    {
        #region Properties

        string _CurrentDate;
        public string CurrentDate
        {
            get { return _CurrentDate; }
            set { SetField(ref _CurrentDate, value); }
        }

        string _CurrentTime;
        public string CurrentTime
        {
            get { return _CurrentTime; }
            set { SetField(ref _CurrentTime, value); }
        }

        string _IconData;
        public string IconData
        {
            get { return _IconData; }
            set { SetField(ref _IconData, value); }
        }

        #endregion

        #region Public

        public void Update()
        {
            var dt = DateTime.Now;

            CurrentDate = dt.ToString(Global.Settings.DateFormat);
            CurrentTime = dt.ToString(Global.Settings.TimeFormat);
        }

        #endregion
    }
}
