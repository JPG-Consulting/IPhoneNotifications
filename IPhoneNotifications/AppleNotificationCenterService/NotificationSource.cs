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
            GattCharacteristic.ValueChanged += GattCharacteristic_ValueChanged;
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

        public async void SubscribeAsync()
        {
            if (GattCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
            {
                await GattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
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
