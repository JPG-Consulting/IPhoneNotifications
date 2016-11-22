using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace IPhoneNotifications
{
    public class AppleNotificationCenterService
    {
        public string BluetoothLEDeviceId;
        public string BluetoothLEDeviceName = "No device selected";

        private BluetoothLEDevice bluetoothLeDevice = null;

        private GattDeviceService NotificationProviderService = null;

        public GattCharacteristic NotificationSource;
        public GattCharacteristic ControlPoint;
        public GattCharacteristic DataSource;
        
        public async void Connect()
        {
            ClearBluetoothLEDevice();

            try
            {
                // BT_Code: BluetoothLEDevice.FromIdAsync must be called from a UI thread because it may prompt for consent.
                bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(BluetoothLEDeviceId);
                bluetoothLeDevice.ConnectionStatusChanged += BluetoothLeDevice_ConnectionStatusChanged;
            }
            catch (Exception ex) when ((uint)ex.HResult == 0x800710df)
            {
                // ERROR_DEVICE_NOT_AVAILABLE because the Bluetooth radio is not on.
            }

            if (bluetoothLeDevice != null)
            {
                Guid ancsUuid = new Guid("7905F431-B5CE-4E99-A40F-4B1E122D00D0");

                try
                {
                    NotificationProviderService = bluetoothLeDevice.GetGattService(ancsUuid);
                }
                catch (Exception ex)
                {
                    throw new Exception("Apple Notification Center Service not found.");
                }
                

                if (NotificationProviderService == null)
                {
                    throw new Exception("Apple Notification Center Service not found.");
                }
                else
                {
                    //// Enum the characteristics for ANCS
                    //IReadOnlyList<GattCharacteristic> characteristics = null;
                    //try
                    //{
                    //    // BT_Code: Get all the child characteristics of a service.
                    //    characteristics = NotificationProviderService.GetAllCharacteristics();
                    //}
                    //catch (Exception ex)
                    //{
                    //    throw new Exception("Restricted service. Can't read characteristics: " + ex.Message);
                    //}

                    Guid notificationSourceUuid = new Guid("9FBF120D-6301-42D9-8C58-25E699A21DBD");
                    Guid controlPointUuid = new Guid("69D1D8F3-45E1-49A8-9821-9BBDFDAAD9D9");
                    Guid dataSourceUuid = new Guid("22EAC6E9-24D6-4BB5-BE44-B36ACE7C7BFB");

                    NotificationSource = NotificationProviderService.GetCharacteristics(notificationSourceUuid).First();
                    ControlPoint       = NotificationProviderService.GetCharacteristics(controlPointUuid).First();
                    DataSource         = NotificationProviderService.GetCharacteristics(dataSourceUuid).First();
                    //foreach (GattCharacteristic c in characteristics)
                    //{
                    //    if (c.Uuid == notificationSourceUuid)
                    //    {
                    //        NotificationSource = c;
                    //    }
                    //    else if (c.Uuid == controlPointUuid)
                    //    {
                    //        ControlPoint = c;
                    //    }
                    //    else if (c.Uuid == dataSourceUuid)
                    //    {
                    //        DataSource = c;
                    //    }
                    //}
                }
            }
            else
            {
                ClearBluetoothLEDevice();
                throw new Exception("Failed to connect to device.");
            }
        }

        private void DataSource_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void NotificationSource_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void ClearBluetoothLEDevice()
        {
            if (NotificationProviderService != null)
            {
                NotificationProviderService?.Dispose();
                NotificationProviderService = null;
            }

            if (ControlPoint != null)
            {
                ControlPoint = null;
            }

            if (NotificationSource != null)
            {
                NotificationSource = null;
            }

            if (DataSource != null)
            {
                DataSource = null;
            }

            try
            {
                bluetoothLeDevice.ConnectionStatusChanged -= BluetoothLeDevice_ConnectionStatusChanged;
            }
            catch (Exception ex)
            {
                // Do nothing
            }
            
            bluetoothLeDevice?.Dispose();
            bluetoothLeDevice = null;
        }

        private async void BluetoothLeDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            if (sender.ConnectionStatus == Windows.Devices.Bluetooth.BluetoothConnectionStatus.Connected)
            {
                NotificationSource.ValueChanged += NotificationSource_ValueChanged;
                DataSource.ValueChanged += DataSource_ValueChanged;


                if (NotificationSource.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                {
                    // Set the notify enable flag
                    try
                    {
                        
                        await NotificationSource.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
                    }
                }

                if (DataSource.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                { 
                    // Set the notify enable flag
                    try
                    {
                        await DataSource.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
                    }
                }
            }
            else
            {
                NotificationSource.ValueChanged -= NotificationSource_ValueChanged;
                DataSource.ValueChanged -= DataSource_ValueChanged;
            }
        }
    }
}
