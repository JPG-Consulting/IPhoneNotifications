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
        public readonly NotificationAttributeCollection NotificationAttributes;

        public AppleNotificationEventArgs(NotificationSourceData source, NotificationAttributeCollection notificationAttributes)
        {
            NotificationSource = source;
            NotificationAttributes = notificationAttributes;
        }
    }
}
