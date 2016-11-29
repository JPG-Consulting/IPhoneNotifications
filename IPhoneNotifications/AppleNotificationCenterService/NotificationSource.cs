using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation;

namespace IPhoneNotifications.AppleNotificationCenterService
{
    //https://developer.apple.com/library/ios/documentation/CoreBluetooth/Reference/AppleNotificationCenterServiceSpecification/Specification/Specification.html
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NotificationSourceData
    {
        public EventID EventId;
        public EventFlags EventFlags;
        public CategoryID CategoryId;
        public byte CategoryCount;

        public UInt32 NotificationUID;
    }

    public class NotificationSource
    {
        public readonly GattCharacteristic GattCharacteristic;
        private bool isValueChangedHandlerRegistered = false;

        public event Action<NotificationSourceData> ValueChanged;

        public GattCharacteristicProperties CharacteristicProperties
        {
            get
            {
                return GattCharacteristic.CharacteristicProperties;
            }
        }

        public NotificationSource(GattCharacteristic characteristic)
        {
            GattCharacteristic = characteristic;
        }

        private void AddValueChangedHandler()
        {
            if (!isValueChangedHandlerRegistered)
            {
                GattCharacteristic.ValueChanged += GattCharacteristic_ValueChanged;
                isValueChangedHandlerRegistered = true;
            }
        }

        private void RemoveValueChangedHandler()
        {
            if (isValueChangedHandlerRegistered)
            {
                GattCharacteristic.ValueChanged -= GattCharacteristic_ValueChanged;
                isValueChangedHandlerRegistered = false;
            }
        }

        private void GattCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var valueBytes = WindowsRuntimeBufferExtensions.ToArray(args.CharacteristicValue);
            var dat = ByteArrayToNotificationSourceData(valueBytes);

            //We dont care about old notifications
            if (dat.EventFlags.HasFlag(EventFlags.EventFlagPreExisting))
            {
                return;
            }

            ValueChanged?.Invoke(dat);
        }

        public async void UnsubscribeAsync()
        {
            RemoveValueChangedHandler();

            try
            {
                var result = await
                        GattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                            GattClientCharacteristicConfigurationDescriptorValue.None);
                if (result != GattCommunicationStatus.Success)
                {
                    System.Diagnostics.Debug.WriteLine("Error deregistering for notifications: {result}");
                    // TODO: Error
                    //rootPage.NotifyUser($"Error registering for notifications: {result}", NotifyType.ErrorMessage);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        public async void SubscribeAsync()
        {
            if (GattCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
            {
                try
                {
                    // BT_Code: Must write the CCCD in order for server to send notifications.
                    // We receive them in the ValueChanged event handler.
                    // Note that this sample configures either Indicate or Notify, but not both.
                    var result = await
                            GattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                                GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    if (result == GattCommunicationStatus.Success)
                    {
                        AddValueChangedHandler();
                        //rootPage.NotifyUser("Successfully registered for notifications", NotifyType.StatusMessage);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Error registering for notifications: {result}");
                        // TODO: Error
                        //rootPage.NotifyUser($"Error registering for notifications: {result}", NotifyType.ErrorMessage);
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
            else
            {
                throw new Exception("Notification source characteristic does not have a notify property.");
            }
        }

        private NotificationSourceData ByteArrayToNotificationSourceData(byte[] packet)
        {
            GCHandle pinnedPacket = GCHandle.Alloc(packet, GCHandleType.Pinned);
            var msg = Marshal.PtrToStructure<NotificationSourceData>(pinnedPacket.AddrOfPinnedObject());
            pinnedPacket.Free();

            return msg;
        }

    }
}
