using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace IPhoneNotifications.AppleNotificationCenterService
{
    public class ControlPoint
    {
        public readonly GattCharacteristic GattCharacteristic;

        public ControlPoint(GattCharacteristic characteristic)
        {
            GattCharacteristic = characteristic;
        }


    }
}
