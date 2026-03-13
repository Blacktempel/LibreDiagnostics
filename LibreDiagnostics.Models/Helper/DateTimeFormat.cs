/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2026 Florian K.
*
*/

namespace LibreDiagnostics.Models.Helper
{
    public class DateTimeFormat
    {
        #region Constructor

        public DateTimeFormat(string format)
        {
            Format = format;
        }

        #endregion

        #region Properties

        public string Format  { get; set; }
        public string Preview { get; set; }

        #endregion

        #region Public

        public void UpdatePreview(DateTime? dateTimePreview = null)
        {
            var dt = dateTimePreview ?? DateTime.Now;
            Preview = dt.ToString(Format);
        }

        public static DateTimeFormat GetDefaultDateFormat(DateTime? dateTimePreview = null)
        {
            var dt = dateTimePreview ?? DateTime.Now;

            var item = new DateTimeFormat("d. MMMM yyyy");
            item.Preview = dt.ToString(item.Format);

            return item;
        }

        public static DateTimeFormat GetDefaultTimeFormat(DateTime? dateTimePreview = null)
        {
            var dt = dateTimePreview ?? DateTime.Now;

            var item = new DateTimeFormat("T");
            item.Preview = dt.ToString(item.Format);

            return item;
        }

        #endregion
    }
}
