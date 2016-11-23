using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPhoneNotifications.AppleNotificationCenterService
{
    public class NotificationActionData
    {
        CommandID CommandID;
        UInt32 NotificationUID;
        ActionID ActionID;

        public NotificationActionData(CommandID commandId, UInt32 notificationUID, ActionID actionId)
        {
            CommandID = commandId;
            NotificationUID = notificationUID;
            ActionID = actionId;
        }

        public byte[] ToArray()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write((byte)this.CommandID);
            writer.Write(this.NotificationUID);
            writer.Write((byte)this.ActionID);

            return stream.ToArray();
        }
    }
}
