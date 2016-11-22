using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPhoneNotifications.AppleNotificationCenterService
{
    public enum CategoryID : byte
    {
        Other = 0,
        IncomingCall = 1,
        MissedCall = 2,
        Voicemail = 3,
        Social = 4,
        Schedule = 5,
        Email = 6,
        News = 7,
        HealthAndFitness = 8,
        BusinessAndFinance = 9,
        Location = 10,
        Entertainment = 11
        //Todo: reserved to 255
    }

    public enum EventID : byte
    {
        NotificationAdded = 0,
        NotificationModified = 1,
        NotificationRemoved = 2
        //Todo: reserved to 255
    }

    [Flags]
    public enum EventFlags : byte
    {
        EventFlagSilent = 1 << 0,
        EventFlagImportant = 1 << 1,
        EventFlagPreExisting = 1 << 2,
        EventFlagPositiveAction = 1 << 3,
        EventFlagNegativeAction = 1 << 4,
        //Reserved1 = 1 << 5,
        //Reserved2 = 1 << 6,
        //Reserved3 = 1 << 7
    }

    public enum CommandID : Byte
    {
        GetNotificationAttributes = 0,
        GetAppAttributes = 1,
        PerformNotificationAction = 2
        // Reserved CommandID values = 3–255
    }

    public enum NotificationAttributeID : byte
    {
        AppIdentifier = 0,
        Title = 1, // Needs to be followed by a 2-bytes max length parameter
        Subtitle = 2, // Needs to be followed by a 2-bytes max length parameter
        Message = 3, // Needs to be followed by a 2-bytes max length parameter
        MessageSize = 4,
        Date = 5,
        PositiveActionLabel = 6,
        NegativeActionLabel = 7,
        // Reserved NotificationAttributeID values = 8–255
    }

    public enum ActionID : byte
    {
        Positive = 0,
        Negative = 1,
        // Reserved ActionID values = 2–255
    }

    public enum AppAttributeID : byte
    {
        DisplayName = 0,
        // Reserved AppAttributeID values = 1–255
    }
}
