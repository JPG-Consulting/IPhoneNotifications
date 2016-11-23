using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.Runtime.InteropServices.WindowsRuntime;

namespace IPhoneNotifications.AppleNotificationCenterService
{
    public class ControlPoint
    {
        public readonly GattCharacteristic GattCharacteristic;

        public ControlPoint(GattCharacteristic characteristic)
        {
            GattCharacteristic = characteristic;
        }

        public async void PerformNotificationActionAsync(UInt32 notificationUID, ActionID actionID)
        {
            GattCommunicationStatus status;

            //Relay notification action back to device
            NotificationActionData command = new NotificationActionData(CommandID.PerformNotificationAction, notificationUID, actionID);

            var bytes = command.ToArray();

            try
            {
                status = await GattCharacteristic.WriteValueAsync(bytes.AsBuffer(), GattWriteOption.WriteWithResponse);
            }
            catch (Exception)
            {

            }
        }
    }
}
