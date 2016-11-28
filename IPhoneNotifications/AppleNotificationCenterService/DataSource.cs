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
    public class DataSource
    {
        public readonly GattCharacteristic GattCharacteristic;

        public event Action<NotificationAttributeCollection> NotificationAttributesReceived;

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
                    NotificationAttributeCollection attributes = new NotificationAttributeCollection(args.CharacteristicValue);

                    NotificationAttributesReceived?.Invoke(attributes);
                    break;
                case CommandID.PerformNotificationAction:
                    break;
            }
        }
    }
}
