/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.MVVM.ComponentModel;

using StorageDeviceDIT = DiskInfoToolkit.StorageDevice;
using StorageDeviceLHM = LibreHardwareMonitor.Hardware.Storage.StorageDevice;

namespace LibreDiagnostics.MVVM.ViewModels
{
    public partial class StorageViewModel : ViewModelBase
    {
        #region Constructor

        protected StorageViewModel(object o)
        {
            //Design time
        }

        public StorageViewModel()
        {
            //Run time
        }

        #endregion

        #region Properties

        StorageDeviceLHM _StorageDevice;
        public StorageDeviceLHM StorageDevice
        {
            get { return _StorageDevice; }
            set { SetField(ref _StorageDevice, value); Storage = _StorageDevice.Storage; }
        }

        StorageDeviceDIT _Storage;
        public StorageDeviceDIT Storage
        {
            get { return _Storage; }
            set { SetField(ref _Storage, value); }
        }

        #endregion
    }

    public class MockStorageViewModel : StorageViewModel
    {
        public MockStorageViewModel() : base(null) { }
    }
}
