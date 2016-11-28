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
    public class NotificationConsumer
    {
        public string BluetoothLEDeviceId;
        public string BluetoothLEDeviceName = "No device selected";

        private BluetoothLEDevice bluetoothLeDevice = null;

        private GattDeviceService GattService = null;

        public NotificationSource NotificationSource;
        public ControlPoint ControlPoint;
        public DataSource DataSource;

        private Dictionary<UInt32, NotificationSourceData> Notifications;

        public event TypedEventHandler<NotificationConsumer, AppleNotificationEventArgs> NotificationAdded;
        public event TypedEventHandler<NotificationConsumer, AppleNotificationEventArgs> NotificationModified;
        public event TypedEventHandler<NotificationConsumer, AppleNotificationEventArgs> NotificationRemoved;

        public static Action<IActivatedEventArgs> OnToastNotification = args => { };
        
        public NotificationConsumer()
        {
            Notifications = new Dictionary<UInt32, NotificationSourceData>();

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
                        await ControlPoint.PerformNotificationActionAsync(Convert.ToUInt32(args["uid"]), ActionID.Positive);
                        break;
                    case "negative":
                        await ControlPoint.PerformNotificationActionAsync(Convert.ToUInt32(args["uid"]), ActionID.Negative);
                        break;
                }
            }
        }

        private void DataSource_NotificationAttributesReceived(NotificationAttributeCollection attributes)
        {
            // Is it a known notification?
            if (Notifications.ContainsKey(attributes.NotificationUID) == false)
            {
                return;
            }

            NotificationSourceData sourceData = Notifications[attributes.NotificationUID];

            switch (sourceData.EventId)
            {
                case EventID.NotificationAdded:
                    NotificationAdded?.Invoke(this, new AppleNotificationEventArgs(sourceData, attributes));
                    break;
                case EventID.NotificationModified:
                    NotificationModified?.Invoke(this, new AppleNotificationEventArgs(sourceData, attributes));
                    break;
                case EventID.NotificationRemoved:
                    NotificationRemoved?.Invoke(this, new AppleNotificationEventArgs(sourceData, attributes));
                    break;
            }

            // Remove the notification from the list
            Notifications.Remove(sourceData.NotificationUID);
        }

        /// <summary>
        /// When the value is changed a new notification ha arrived. We need to send a query about notification
        /// to the ControlPoint to get the actual notification message.
        /// </summary>
        /// <param name="obj"></param>
        private async void NotificationSource_ValueChanged(NotificationSourceData obj)
        {
            // We don't care about old notifications
            if (obj.EventFlags.HasFlag(EventFlags.EventFlagPreExisting))
            {
                return;
            }

            // TODO: Check this out. Not sure why but sometime I get a Notification UID = 0
            //       which breaks everything.
            if (obj.NotificationUID == 0)
            {
                return;
            }

            // Store the notification
            if (Notifications.ContainsKey(obj.NotificationUID))
            {
                Notifications[obj.NotificationUID] = obj;
            }
            else
            {
                Notifications.Add(obj.NotificationUID, obj);
            }
            
            // Build the attributes list for the GetNotificationAttributtes command.   
            List<NotificationAttributeID> attributes = new List<NotificationAttributeID>();
            attributes.Add(NotificationAttributeID.AppIdentifier);
            attributes.Add(NotificationAttributeID.Title);
            attributes.Add(NotificationAttributeID.Message);
            
            if (obj.EventFlags.HasFlag(EventFlags.EventFlagPositiveAction))
            {
                attributes.Add(NotificationAttributeID.PositiveActionLabel);
            }

            if (obj.EventFlags.HasFlag(EventFlags.EventFlagNegativeAction))
            {
                attributes.Add(NotificationAttributeID.NegativeActionLabel);
            }

            try
            {
                var communicationStatus = await ControlPoint.GetNotificationAttributesAsync(obj.NotificationUID, attributes);
            }
            catch (Exception ex)
            { 
                // keep quiet...
            }
        }
    }
}
