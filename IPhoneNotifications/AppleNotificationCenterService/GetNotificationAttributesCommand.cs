using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPhoneNotifications.AppleNotificationCenterService
{
    public class GetNotificationAttributesCommand
    {
        public CommandID CommandID;
        public UInt32 NotificationUID;
        public List<NotificationAttribute> Attributes;

        public GetNotificationAttributesCommand(CommandID commandID, UInt32 notificationUID)
        {
            CommandID = commandID;
            NotificationUID = notificationUID;
            Attributes = new List<NotificationAttribute>();
        }

        public byte[] ToArray()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write((byte)this.CommandID);
            writer.Write(this.NotificationUID);

            foreach (NotificationAttribute a in Attributes)
            {
                writer.Write((byte)a.ID);
                if (a.MaxLength != 0)
                {
                    writer.Write(a.MaxLength);
                }
            }
            
            return stream.ToArray();
        }
    }

    
}
