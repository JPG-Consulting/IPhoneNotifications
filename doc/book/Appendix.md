# Appendix

The following tables list important values used in the ANCS.


## CategoryID Values

Name                         | Value
-----------------------------|------
CategoryIDOther              | = 0,
CategoryIDIncomingCall       | = 1,
CategoryIDMissedCall         | = 2,
CategoryIDVoicemail          | = 3,
CategoryIDSocial             | = 4,
CategoryIDSchedule           | = 5,
CategoryIDEmail              | = 6,
CategoryIDNews               | = 7,
CategoryIDHealthAndFitness   | = 8,
CategoryIDBusinessAndFinance | = 9,
CategoryIDLocation           | = 10,
CategoryIDEntertainment      | = 11,
Reserved CategoryID values   | = 12–255


## EventID Values

Name                         | Value
-----------------------------|------
EventIDNotificationAdded     | = 0,
EventIDNotificationModified  | = 1,
EventIDNotificationRemoved   | = 2,
Reserved EventID values      | = 3–255


## EventFlags

Name                         | Value
-----------------------------|------
EventFlagSilent              | = (1 << 0),
EventFlagImportant           | = (1 << 1),
EventFlagPreExisting         | = (1 << 2),
EventFlagPositiveAction      | = (1 << 3),
EventFlagNegativeAction      | = (1 << 4),
Reserved EventFlags          | = (1 << 5)–(1 << 7)


## CommandID Values

Name                               | Value
-----------------------------------|------
CommandIDGetNotificationAttributes | = 0,
CommandIDGetAppAttributes          | = 1,
CommandIDPerformNotificationAction | = 2,
Reserved CommandID values          | = 3–255


## NotificationAttributeID Values

Name                                       | Value
-------------------------------------------|------
NotificationAttributeIDAppIdentifier       | = 0,
NotificationAttributeIDTitle               | = 1, (Needs to be followed by a 2-bytes max length parameter)
NotificationAttributeIDSubtitle            | = 2, (Needs to be followed by a 2-bytes max length parameter)
NotificationAttributeIDMessage             | = 3, (Needs to be followed by a 2-bytes max length parameter)
NotificationAttributeIDMessageSize         | = 4,
NotificationAttributeIDDate                | = 5,
NotificationAttributeIDPositiveActionLabel | = 6,
NotificationAttributeIDNegativeActionLabel | = 7,
Reserved NotificationAttributeID values    | = 8–255

>Note: The format of the NotificationAttributeIDMessageSize constant is a string that represents the integral value of the message size. The format of the NotificationAttributeIDDate constant is a string that uses the Unicode Technical Standard (UTS) #35 date format pattern yyyyMMdd'T'HHmmSS. The format of all the other constants in Table 3-5 are UTF-8 strings.


## ActionID Values

Name                     | Value
-------------------------|------
ActionIDPositive         | = 0,
ActionIDNegative         | = 1,
Reserved ActionID values | = 2–255


## AppAttributeID Values

Name                           | Value
-------------------------------|------
AppAttributeIDDisplayName      | = 0,
Reserved AppAttributeID values | = 1–255
