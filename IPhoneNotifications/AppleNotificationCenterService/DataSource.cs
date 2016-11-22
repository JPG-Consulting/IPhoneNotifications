using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.Runtime.InteropServices.WindowsRuntime;

namespace IPhoneNotifications.AppleNotificationCenterService
{
    public enum NotificationAttribute : byte
    {
        AppIdentifier = 0x0,
        Title = 0x1,
        Subtitle = 0x2,
        Message = 0x3
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GetNotificationAttributesData
    {
        public byte CommandId;
        public UInt32 NotificationUID;
        public byte AttributeId1;
        public UInt16 AttributeId1MaxLen;
        public byte AttributeId2;
        public UInt16 AttributeId2MaxLen;

        public byte[] ToArray()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write(this.CommandId);
            writer.Write(this.NotificationUID);
            writer.Write(this.AttributeId1);
            writer.Write(this.AttributeId1MaxLen);
            writer.Write(this.AttributeId2);
            writer.Write(this.AttributeId2MaxLen);

            return stream.ToArray();
        }
    }

    public class DataSource
    {
        public readonly GattCharacteristic GattCharacteristic;

        public event Action<NotificationSourceData> ValueChanged;

        private NotificationSourceData _NotificationSourceData;

        public NotificationSourceData NotificationSourceData
        {
            get
            {
                return _NotificationSourceData;
            }
            set
            {
                _NotificationSourceData = value;
            }
        }

        public DataSource(GattCharacteristic characteristic)
        {
            GattCharacteristic = characteristic;
            GattCharacteristic.ValueChanged += GattCharacteristic_ValueChanged;
        }

        public GattCharacteristicProperties CharacteristicProperties
        {
            get
            {
                return GattCharacteristic.CharacteristicProperties;
            }
        }

        public async void Refresh()
        {
            await GattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
        }
        
        private void GattCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var stream = args.CharacteristicValue.AsStream();
            var br = new BinaryReader(stream);

            var cmdId = br.ReadByte();
            var notUid = br.ReadUInt32();
            var attr1 = (NotificationAttribute)br.ReadByte();
            var attr1len = br.ReadUInt16();
            var attr1val = br.ReadChars(attr1len);
            var attr2 = (NotificationAttribute)br.ReadByte();
            var attr2len = br.ReadUInt16();
            var attr2val = br.ReadChars(attr2len);

            throw new NotImplementedException();
        }
    }
}
