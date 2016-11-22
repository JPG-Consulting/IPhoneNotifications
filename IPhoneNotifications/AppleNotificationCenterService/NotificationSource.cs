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
    public enum CategoryId : byte
    {
        Other = 0,
        IncomingCall = 1,
        MissedCall = 2,
        Voicemail = 3,
        Social = 4,
        Schedule = 5,
        Email = 6,
        News = 7,
        HealthAndFitness = 8,
        BusinessAndFinance = 9,
        Location = 10,
        Entertainment = 11
        //Todo: reserved to 255
    }

    [Flags]
    public enum EventFlags : byte
    {
        EventFlagSilent = 1 << 0,
        EventFlagImportant = 1 << 1,
        EventFlagPreExisting = 1 << 2,
        EventFlagPositiveAction = 1 << 3,
        EventFlagNegativeAction = 1 << 4,
        Reserved1 = 1 << 5,
        Reserved2 = 1 << 6,
        Reserved3 = 1 << 7
    }


    //https://developer.apple.com/library/ios/documentation/CoreBluetooth/Reference/AppleNotificationCenterServiceSpecification/Specification/Specification.html
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NotificationSourceData
    {
        public byte EventId;
        public EventFlags EventFlags;
        public CategoryId CategoryId;
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
            //throw new NotImplementedException();
        }

        public async void Refresh()
        {
            await GattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
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
