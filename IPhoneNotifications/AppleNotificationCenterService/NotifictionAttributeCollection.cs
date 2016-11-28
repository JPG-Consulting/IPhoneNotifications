using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace IPhoneNotifications.AppleNotificationCenterService
{
    public class NotificationAttributeCollection
    {
        public readonly UInt32 NotificationUID;
        private Dictionary<NotificationAttributeID, string> _dictionary;

        public NotificationAttributeCollection(IBuffer value)
        {
            _dictionary = new Dictionary<NotificationAttributeID, string>();

            var stream = value.AsStream();
            var br = new BinaryReader(stream);

            // Read and validate the command ID.
            CommandID commandID = (CommandID)br.ReadByte();
            if (commandID != CommandID.GetNotificationAttributes)
            {
                throw new Exception("Invalid command.");
            }

            // Read the notification UID
            NotificationUID = br.ReadUInt32();
            
            // Read Attributes
            while (stream.Position < stream.Length)
            {
                NotificationAttributeID attributeID = (NotificationAttributeID)br.ReadByte();
                UInt16 attributeLength = br.ReadUInt16();
                String attributeValue = Encoding.UTF8.GetString(br.ReadBytes(attributeLength));

                _dictionary.Add(attributeID, attributeValue);
            }
        }

        public string this[NotificationAttributeID key]
        {
            get
            {
                return _dictionary[key];
            }
        }

        public int Count
        {
            get
            {
                return _dictionary.Count;
            }
        }

        public ICollection<NotificationAttributeID> Keys
        {
            get
            {
                return _dictionary.Keys;
            }
        }

        public ICollection<string> Values
        {
            get
            {
                return _dictionary.Values;
            }
        }
        
        public bool ContainsKey(NotificationAttributeID key)
        {
            return _dictionary.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<NotificationAttributeID, string>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }
        
        public bool TryGetValue(NotificationAttributeID key, out string value)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }
}
