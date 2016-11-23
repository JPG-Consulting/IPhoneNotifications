using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPhoneNotifications.AppleNotificationCenterService
{
    public class AppleNotificationEventArgs
    {
        public readonly NotificationSourceData NotificationSource;
        public readonly GetNotificationAttributesCommand NotificationAttributes;

        public AppleNotificationEventArgs(NotificationSourceData source, GetNotificationAttributesCommand notificationAttributes)
        {
            NotificationSource = source;
            NotificationAttributes = notificationAttributes;
        }

        public String Title
        {
            get
            {
                foreach (NotificationAttribute attr in NotificationAttributes.Attributes)
                {
                    if (attr.ID == NotificationAttributeID.Title)
                    {
                        return attr.Value;
                    }
                }

                return "";
            }
        }

        public String Message
        {
            get
            {
                foreach (NotificationAttribute attr in NotificationAttributes.Attributes)
                {
                    if (attr.ID == NotificationAttributeID.Message)
                    {
                        return attr.Value;
                    }
                }

                return "";
            }
        }

        public String PositiveActionLabel
        {
            get
            {
                foreach (NotificationAttribute attr in NotificationAttributes.Attributes)
                {
                    if (attr.ID == NotificationAttributeID.PositiveActionLabel)
                    {
                        return attr.Value;
                    }
                }

                return "Positive";
            }
        }

        public String NegativeActionLabel
        {
            get
            {
                foreach (NotificationAttribute attr in NotificationAttributes.Attributes)
                {
                    if (attr.ID == NotificationAttributeID.NegativeActionLabel)
                    {
                        return attr.Value;
                    }
                }

                return "Negative";
            }
        }
    }
}
