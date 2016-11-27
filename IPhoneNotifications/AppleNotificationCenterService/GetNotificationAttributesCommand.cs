using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace IPhoneNotifications.AppleNotificationCenterService
{
    public class GetNotificationAttributesCommand
    {
        public CommandID CommandID;
        public UInt32 NotificationUID;
        public readonly List<NotificationAttribute> Attributes;

        public GetNotificationAttributesCommand(CommandID commandID, UInt32 notificationUID)
        {
            CommandID = commandID;
            NotificationUID = notificationUID;
            Attributes = new List<NotificationAttribute>();
        }

        public GetNotificationAttributesCommand(IBuffer characteristicValue)
        {
            var stream = characteristicValue.AsStream();
            var br = new BinaryReader(stream);

            CommandID       = (CommandID)br.ReadByte();
            NotificationUID = br.ReadUInt32();
            Attributes = new List<NotificationAttribute>();

            // Read Attributes
            while (stream.Position < stream.Length)
            {
                NotificationAttributeID attributeID = (NotificationAttributeID)br.ReadByte();
                UInt16 attributeLength = br.ReadUInt16();
                String value = Encoding.UTF8.GetString(br.ReadBytes(attributeLength));

                Attributes.Add(new NotificationAttribute(attributeID, value));
            }
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
