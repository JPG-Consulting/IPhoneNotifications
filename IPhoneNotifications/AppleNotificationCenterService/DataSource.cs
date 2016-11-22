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

        public async void GetNotificationData(NotificationSourceData notificationSourceData)
        {
            //Ask for more data through the control point characteristic
            var attributes = new GetNotificationAttributesData
            {
                CommandId = 0x0,
                NotificationUID = notificationSourceData.NotificationUID,
                AttributeId1 = (byte)NotificationAttribute.Title,
                AttributeId1MaxLen = 16,
                AttributeId2 = (byte)NotificationAttribute.Message,
                AttributeId2MaxLen = 32
            };

            var bytes = attributes.ToArray();

            try
            { 
                var status = await GattCharacteristic.WriteValueAsync(bytes.AsBuffer(), GattWriteOption.WriteWithResponse);
            }
            catch (Exception)
            {

            }
        }

        private void GattCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
