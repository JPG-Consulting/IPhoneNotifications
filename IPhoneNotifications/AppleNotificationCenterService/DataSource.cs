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

        public event Action<AppleNotificationEventArgs> NotificationAttributesReceived;

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

        public async void SubscribeAsync()
        {
            await GattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
        }
        
        private void GattCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            CommandID commandID = (CommandID)args.CharacteristicValue.GetByte(0);

            switch (commandID)
            {
                case CommandID.GetAppAttributes:
                    break;
                case CommandID.GetNotificationAttributes:
                    NotificationAttributeCollection reply = new NotificationAttributeCollection(args.CharacteristicValue);

                    if (NotificationSourceData.NotificationUID == reply.NotificationUID)
                    {
                        NotificationAttributesReceived?.Invoke(new AppleNotificationEventArgs(_NotificationSourceData, reply));
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Missing data source notification.");
                        throw new Exception("Missing data source notification");
                    }
                    break;
                case CommandID.PerformNotificationAction:
                    break;
            }
        }
    }
}
