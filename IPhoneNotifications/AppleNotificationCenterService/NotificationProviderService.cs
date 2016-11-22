using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.Runtime.InteropServices.WindowsRuntime;

namespace IPhoneNotifications.AppleNotificationCenterService
{
    public class NotificationProviderService
    {
        public string BluetoothLEDeviceId;
        public string BluetoothLEDeviceName = "No device selected";

        private BluetoothLEDevice bluetoothLeDevice = null;

        private GattDeviceService GattService = null;

        public NotificationSource NotificationSource;
        public GattCharacteristic ControlPoint;
        public DataSource DataSource;
        
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
                    GattService = bluetoothLeDevice.GetGattService(ancsUuid);
                }
                catch (Exception ex)
                {
                    throw new Exception("Apple Notification Center Service not found.");
                }
                

                if (GattService == null)
                {
                    throw new Exception("Apple Notification Center Service not found.");
                }
                else
                {
                    Guid notificationSourceUuid = new Guid("9FBF120D-6301-42D9-8C58-25E699A21DBD");
                    Guid controlPointUuid = new Guid("69D1D8F3-45E1-49A8-9821-9BBDFDAAD9D9");
                    Guid dataSourceUuid = new Guid("22EAC6E9-24D6-4BB5-BE44-B36ACE7C7BFB");

                    try
                    {
                        NotificationSource = new NotificationSource(GattService.GetCharacteristics(notificationSourceUuid).First());
                        ControlPoint = GattService.GetCharacteristics(controlPointUuid).First();
                        DataSource = new DataSource(GattService.GetCharacteristics(dataSourceUuid).First());
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
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

        private void ClearBluetoothLEDevice()
        {
            GattService?.Dispose();
            GattService = null;
            

            if (ControlPoint != null)
            {
                ControlPoint = null;
            }

            if (NotificationSource != null)
            {
                NotificationSource.ValueChanged -= NotificationSource_ValueChanged;
                NotificationSource = null;
            }

            if (DataSource != null)
            {
                //DataSource.ValueChanged -= DataSource_ValueChanged;
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
                //NotificationSource.ValueChanged += NotificationSource_ValueChanged;
                NotificationSource.ValueChanged += NotificationSource_ValueChanged;

                if (NotificationSource.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                {
                    // Set the notify enable flag
                    try
                    {
                        NotificationSource.Refresh();  
                        //await NotificationSource.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
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
                        DataSource.Refresh();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
                    }
                }
            }
            else
            {
                //NotificationSource.ValueChanged -= NotificationSource_ValueChanged;
               // DataSource.ValueChanged -= DataSource_ValueChanged;
            }
        }

        private async void NotificationSource_ValueChanged(NotificationSourceData obj)
        {
            DataSource.NotificationSourceData = obj;
            
            //Ask for more data through the control point characteristic
            var attributes = new GetNotificationAttributesData
            {
                CommandId = (byte)CommandID.GetNotificationAttributes,
                NotificationUID = obj.NotificationUID,
                AttributeId1 = (byte)NotificationAttributeID.Title,
                AttributeId1MaxLen = 16,
                AttributeId2 = (byte)NotificationAttributeID.Message,
                AttributeId2MaxLen = 32
            };

            var bytes = attributes.ToArray();

            try
            {
                var status = await ControlPoint.WriteValueAsync(bytes.AsBuffer(), GattWriteOption.WriteWithResponse);
            }
            catch (Exception)
            {

            }
        }
    }
}
