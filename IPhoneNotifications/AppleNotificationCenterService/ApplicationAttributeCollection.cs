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
    public class ApplicationAttributeCollection
    {
        public readonly string AppIdentifier;
        private Dictionary<AppAttributeID, string> _dictionary;

        public ApplicationAttributeCollection(IBuffer value)
        {
            _dictionary = new Dictionary<AppAttributeID, string>();

            var stream = value.AsStream();
            var br = new BinaryReader(stream);

            // Read and validate the command ID.
            CommandID commandID = (CommandID)br.ReadByte();
            if (commandID != CommandID.GetAppAttributes)
            {
                throw new Exception("Invalid command.");
            }

            // Read the app identifier
            byte tByte;
            List<byte> appIdentifierBytes = new List<byte>();

            while ((stream.Position < stream.Length) && ((tByte = br.ReadByte()) != 0))
            {
                appIdentifierBytes.Add(tByte);
            }

            AppIdentifier = Encoding.UTF8.GetString(appIdentifierBytes.ToArray());
            
            // Read Attributes
            while (stream.Position < stream.Length)
            {
                AppAttributeID attributeID = (AppAttributeID)br.ReadByte();
                UInt16 attributeLength = br.ReadUInt16();
                String attributeValue = Encoding.UTF8.GetString(br.ReadBytes(attributeLength));

                _dictionary.Add(attributeID, attributeValue);
            }
        }

        public string this[AppAttributeID key]
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

        public ICollection<AppAttributeID> Keys
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
        
        public bool ContainsKey(AppAttributeID key)
        {
            return _dictionary.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<AppAttributeID, string>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }
        
        public bool TryGetValue(AppAttributeID key, out string value)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }
}
