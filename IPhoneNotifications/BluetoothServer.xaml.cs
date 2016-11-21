using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace IPhoneNotifications
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BluetoothServer : Page
    {
        private MainPage rootPage = MainPage.Current;

        private DeviceWatcher deviceWatcher;

        private ObservableCollection<BluetoothLEDeviceDisplay> ResultCollection = new ObservableCollection<BluetoothLEDeviceDisplay>();

        public BluetoothServer()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            StopBleDeviceWatcher();

            // Save the selected device's ID for use in other scenarios.
            var bleDeviceDisplay = ResultsListView.SelectedItem as BluetoothLEDeviceDisplay;
            if (bleDeviceDisplay != null)
            {
                rootPage.SelectedBleDeviceId = bleDeviceDisplay.Id;
                rootPage.SelectedBleDeviceName = bleDeviceDisplay.Name;
            }
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (deviceWatcher == null)
            {
                StartBleDeviceWatcher();

                rootPage.NotifyUser($"Device watcher started.", NotifyType.StatusMessage);
            }
        }

        #region Device discovery

        private void EnumerateButton_Click()
        {
            if (deviceWatcher == null)
            {
                StartBleDeviceWatcher();

                rootPage.NotifyUser($"Device watcher started.", NotifyType.StatusMessage);
            }
            else
            {
                StopBleDeviceWatcher();

                rootPage.NotifyUser($"Device watcher stopped.", NotifyType.StatusMessage);
            }
        }

        /// <summary>
        ///     Starts a device watcher that looks for all nearby BT devices (paired or unpaired). Attaches event handlers and
        ///     populates the collection of devices.
        /// </summary>
        private void StartBleDeviceWatcher()
        {
            EnumerateButton.Content = "Stop enumerating";

            // Additional properties we would like about the device.
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            // BT_Code: Currently Bluetooth APIs don't provide a selector to get ALL devices that are both paired and non-paired.
            deviceWatcher =
                    DeviceInformation.CreateWatcher(
                        "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")",
                        requestedProperties,
                        DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;

            // Start over with an empty collection.
            ResultCollection.Clear();

            // Start the watcher.
            deviceWatcher.Start();
        }

        /// <summary>
        ///     Stops a device watcher that looks for all nearby BT devices (paired or unpaired). Dettaches event handlers.
        /// </summary>
        private void StopBleDeviceWatcher()
        {
            if (deviceWatcher != null)
            {
                // Unregister the event handlers.
                deviceWatcher.Added -= DeviceWatcher_Added;
                deviceWatcher.Updated -= DeviceWatcher_Updated;
                deviceWatcher.Removed -= DeviceWatcher_Removed;
                deviceWatcher.EnumerationCompleted -= DeviceWatcher_EnumerationCompleted;
                deviceWatcher.Stopped -= DeviceWatcher_Stopped;

                // Stop the watcher.
                deviceWatcher.Stop();
                deviceWatcher = null;
            }

            EnumerateButton.Content = "Start enumerating";
        }

        private async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == deviceWatcher)
                {
                    // Make sure device name isn't blank or already present in the list.
                    if (deviceInfo.Name != string.Empty && FindBluetoothLEDeviceDisplay(deviceInfo.Id) == null)
                    {
                        ResultCollection.Add(new BluetoothLEDeviceDisplay(deviceInfo));
                    }
                }
            });
        }

        private async void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == deviceWatcher)
                {
                    BluetoothLEDeviceDisplay bleDeviceDisplay = FindBluetoothLEDeviceDisplay(deviceInfoUpdate.Id);
                    if (bleDeviceDisplay != null)
                    {
                        bleDeviceDisplay.Update(deviceInfoUpdate);
                    }
                }
            });
        }

        private async void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == deviceWatcher)
                {
                    // Find the corresponding DeviceInformation in the collection and remove it.
                    BluetoothLEDeviceDisplay bleDeviceDisplay = FindBluetoothLEDeviceDisplay(deviceInfoUpdate.Id);
                    if (bleDeviceDisplay != null)
                    {
                        ResultCollection.Remove(bleDeviceDisplay);
                    }
                }
            });
        }

        private async void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object e)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == deviceWatcher)
                {
                    rootPage.NotifyUser($"{ResultCollection.Count} devices found. Enumeration completed.",
                        NotifyType.StatusMessage);
                }
            });
        }

        private async void DeviceWatcher_Stopped(DeviceWatcher sender, object e)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == deviceWatcher)
                {
                    rootPage.NotifyUser($"No longer watching for devices.",
                            sender.Status == DeviceWatcherStatus.Aborted ? NotifyType.ErrorMessage : NotifyType.StatusMessage);
                }
            });
        }

        private BluetoothLEDeviceDisplay FindBluetoothLEDeviceDisplay(string id)
        {
            foreach (BluetoothLEDeviceDisplay bleDeviceDisplay in ResultCollection)
            {
                if (bleDeviceDisplay.Id == id)
                {
                    return bleDeviceDisplay;
                }
            }
            return null;
        }

        #endregion

        #region Pairing

        private bool isBusy = false;

        private async void PairButton_Click()
        {
            // Do not allow a new Pair operation to start if an existing one is in progress.
            if (isBusy)
            {
                return;
            }

            isBusy = true;

            rootPage.NotifyUser("Pairing started. Please wait...", NotifyType.StatusMessage);

            // For more information about device pairing, including examples of
            // customizing the pairing process, see the DeviceEnumerationAndPairing sample.

            // Capture the current selected item in case the user changes it while we are pairing.
            var bleDeviceDisplay = ResultsListView.SelectedItem as BluetoothLEDeviceDisplay;

            // BT_Code: Pair the currently selected device.
            DevicePairingResult result = await bleDeviceDisplay.DeviceInformation.Pairing.PairAsync();
            rootPage.NotifyUser($"Pairing result = {result.Status}",
                result.Status == DevicePairingResultStatus.Paired || result.Status == DevicePairingResultStatus.AlreadyPaired
                    ? NotifyType.StatusMessage
                    : NotifyType.ErrorMessage);

            isBusy = false;
        }

        #endregion

        private readonly Guid _ancsServiceUiid = new Guid("7905F431-B5CE-4E99-A40F-4B1E122D00D0");
        private readonly Guid _notificationSourceCharacteristicUuid = new Guid("9FBF120D-6301-42D9-8C58-25E699A21DBD");
        private readonly Guid _controlPointCharacteristicUuid = new Guid("69D1D8F3-45E1-49A8-9821-9BBDFDAAD9D9");
        private readonly Guid _dataSourceCharacteristicUuid = new Guid("22EAC6E9-24D6-4BB5-BE44-B36ACE7C7BFB");

        private BluetoothLEDevice bluetoothLeDevice = null;

        private ObservableCollection<BluetoothLEAttributeDisplay> ServiceCollection = new ObservableCollection<BluetoothLEAttributeDisplay>();


        private void ClearBluetoothLEDevice()
        {
            bluetoothLeDevice?.Dispose();
            bluetoothLeDevice = null;
        }

        private async void ConnectButton_Click()
        {
            ConnectButton.IsEnabled = false;

            ClearBluetoothLEDevice();
            try
            {
                // BT_Code: BluetoothLEDevice.FromIdAsync must be called from a UI thread because it may prompt for consent.
                var bleDeviceDisplay = ResultsListView.SelectedItem as BluetoothLEDeviceDisplay;
                if (bleDeviceDisplay != null)
                {
                    rootPage.SelectedBleDeviceId = bleDeviceDisplay.Id;
                    rootPage.SelectedBleDeviceName = bleDeviceDisplay.Name;
                }

                bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(rootPage.SelectedBleDeviceId);
            }
            catch (Exception ex) when ((uint)ex.HResult == 0x800710df)
            {
                // ERROR_DEVICE_NOT_AVAILABLE because the Bluetooth radio is not on.
            }

            if (bluetoothLeDevice != null)
            {
                var hasANCS = false;

                // BT_Code: GattServices returns a list of all the supported services of the device.
                // If the services supported by the device are expected to change
                // during BT usage, subscribe to the GattServicesChanged event.
                foreach (var service in bluetoothLeDevice.GattServices)
                {
                    if (service.Uuid == _ancsServiceUiid)
                    {
                        hasANCS = true;
                    }
                    //ServiceCollection.Add(new BluetoothLEAttributeDisplay(service));
                    // 
                }
                //ConnectButton.Visibility = Visibility.Collapsed;
                //ServiceList.Visibility = Visibility.Visible;
                if (!hasANCS)
                {
                    ClearBluetoothLEDevice();
                    rootPage.NotifyUser("ANCS service not found.", NotifyType.ErrorMessage);
                }
            }
            else
            {
                ClearBluetoothLEDevice();
                rootPage.NotifyUser("Failed to connect to device.", NotifyType.ErrorMessage);
            }
            ConnectButton.IsEnabled = true;
        }
    }
}
