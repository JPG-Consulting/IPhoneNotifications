using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.ApplicationModel.Activation;
using Microsoft.QueryStringDotNET;

namespace IPhoneNotifications.AppleNotificationCenterService
{
    public class NotificationProviderService
    {
        public string BluetoothLEDeviceId;
        public string BluetoothLEDeviceName = "No device selected";

        private BluetoothLEDevice bluetoothLeDevice = null;

        private GattDeviceService GattService = null;

        public NotificationSource NotificationSource;
        public ControlPoint ControlPoint;
        public DataSource DataSource;

        public event TypedEventHandler<NotificationProviderService, AppleNotificationEventArgs> NotificationAdded;
        public event TypedEventHandler<NotificationProviderService, AppleNotificationEventArgs> NotificationModified;
        public event TypedEventHandler<NotificationProviderService, AppleNotificationEventArgs> NotificationRemoved;

        public event TypedEventHandler<NotificationProviderService, AppleNotificationEventArgs> NotificationReceived;
        public static Action<IActivatedEventArgs> OnToastNotification = args => { };
        
        public NotificationProviderService()
        {
            OnToastNotification = OnToastNotificationReceived;
        }

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
                    Guid controlPointUuid       = new Guid("69D1D8F3-45E1-49A8-9821-9BBDFDAAD9D9");
                    Guid dataSourceUuid         = new Guid("22EAC6E9-24D6-4BB5-BE44-B36ACE7C7BFB");

                    try
                    {
                        ControlPoint       = new ControlPoint(GattService.GetCharacteristics(controlPointUuid).First());
                        DataSource         = new DataSource(GattService.GetCharacteristics(dataSourceUuid).First());
                        NotificationSource = new NotificationSource(GattService.GetCharacteristics(notificationSourceUuid).First());
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
                DataSource.NotificationAttributesReceived -= DataSource_NotificationAttributesReceived;
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
                DataSource.NotificationAttributesReceived += DataSource_NotificationAttributesReceived;
                NotificationSource.ValueChanged += NotificationSource_ValueChanged;

                DataSource.SubscribeAsync();
                NotificationSource.SubscribeAsync();
            }
            else
            {
                DataSource.NotificationAttributesReceived -= DataSource_NotificationAttributesReceived;
                NotificationSource.ValueChanged -= NotificationSource_ValueChanged;
            }
        }

        public async void OnToastNotificationReceived(IActivatedEventArgs e)
        {
            // Handle toast activation
            if (e is ToastNotificationActivatedEventArgs)
            {
                var toastActivationArgs = e as ToastNotificationActivatedEventArgs;

                // Parse the query string
                QueryString args = QueryString.Parse(toastActivationArgs.Argument);

                // See what action is being requested 
                switch (args["action"])
                {
                    case "positive":
                        ControlPoint.PerformNotificationActionAsync(Convert.ToUInt32(args["uid"]), ActionID.Positive);
                        break;
                    case "negative":
                        ControlPoint.PerformNotificationActionAsync(Convert.ToUInt32(args["uid"]), ActionID.Negative);
                        break;
                }
            }
        }

        private void DataSource_NotificationAttributesReceived(AppleNotificationEventArgs obj)
        {
            switch (obj.NotificationSource.EventId)
            {
                case EventID.NotificationAdded:
                    NotificationAdded?.Invoke(this, obj);
                    break;
                case EventID.NotificationModified:
                    NotificationModified?.Invoke(this, obj);
                    break;
                case EventID.NotificationRemoved:
                    NotificationRemoved?.Invoke(this, obj);
                    break;
            }
        }

        private async void NotificationSource_ValueChanged(NotificationSourceData obj)
        {
            DataSource.NotificationSourceData = obj;

            var command = new GetNotificationAttributesCommand(CommandID.GetNotificationAttributes, obj.NotificationUID);
            command.Attributes.Add(new NotificationAttribute(NotificationAttributeID.AppIdentifier));
            command.Attributes.Add(new NotificationAttribute(NotificationAttributeID.Title, 64));
            command.Attributes.Add(new NotificationAttribute(NotificationAttributeID.Message, 128));
            
            if (obj.EventFlags.HasFlag(EventFlags.EventFlagPositiveAction))
            {
                command.Attributes.Add(new NotificationAttribute(NotificationAttributeID.PositiveActionLabel));
            }

            if (obj.EventFlags.HasFlag(EventFlags.EventFlagNegativeAction))
            {
                command.Attributes.Add(new NotificationAttribute(NotificationAttributeID.NegativeActionLabel));
            }

            var bytes = command.ToArray();

            ////Ask for more data through the control point characteristic
            //var attributes = new GetNotificationAttributesData
            //{
            //    CommandId = (byte)CommandID.GetNotificationAttributes,
            //    NotificationUID = obj.NotificationUID,
            //    AttributeId1 = (byte)NotificationAttributeID.Title,
            //    AttributeId1MaxLen = 16,
            //    AttributeId2 = (byte)NotificationAttributeID.Message,
            //    AttributeId2MaxLen = 32
            //};
            
            //var bytes2 = attributes.ToArray();

            try
            {
                var status = await ControlPoint.GattCharacteristic.WriteValueAsync(bytes.AsBuffer(), GattWriteOption.WriteWithResponse);
                if (status == GattCommunicationStatus.Success)
                {
                    // Raise an event?
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
