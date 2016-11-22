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
        
        public byte[] ToArray()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write((byte)this.CommandID);
            writer.Write(this.NotificationUID);
            
            
            return stream.ToArray();
        }
    }

    
}
