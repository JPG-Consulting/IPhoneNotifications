using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPhoneNotifications.AppleNotificationCenterService
{
    public class NotificationAttribute
    {
        public NotificationAttributeID ID;
        public UInt16 Length = 0;
        private String _value = null;
        public UInt16 MaxLength = 0;

        public String Value
        {
            get { return _value; }
        }

        public NotificationAttribute(NotificationAttributeID attributeID)
        {
            ID = attributeID;

            if ((attributeID == NotificationAttributeID.Title) || (attributeID == NotificationAttributeID.Subtitle))
            {
                MaxLength = 16;
            }
            else if (attributeID == NotificationAttributeID.Message)
            {
                MaxLength = 32;
            }
        }

        public NotificationAttribute(NotificationAttributeID attributeID, UInt16 maxLength)
        {
            ID = attributeID;
            MaxLength = maxLength;

            if (maxLength == 0)
            {
                if ((attributeID == NotificationAttributeID.Title) || (attributeID == NotificationAttributeID.Subtitle))
                {
                    MaxLength = 16;
                }
                else if (attributeID == NotificationAttributeID.Message)
                {
                    MaxLength = 32;
                }
            }
        }

        public NotificationAttribute(NotificationAttributeID attributeID, String value)
        {
            ID = attributeID;
            _value = value;
            Length = (UInt16)value.Length;
        }
    }
}
