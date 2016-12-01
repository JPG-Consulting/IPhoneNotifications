using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPhoneNotifications.AppleNotificationCenterService
{
    /// <summary>
    /// 
    /// </summary>
    class UnknownCommandException : Exception
    {
        public UnknownCommandException() : base("Unknown command. The commandID was not recognized by the NP.")
        {
        }

        public UnknownCommandException(Exception innerException) : base("Unknown command. The commandID was not recognized by the NP.", innerException)
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class InvalidCommandException : Exception
    {
        public InvalidCommandException() : base("Invalid command.The command was improperly formatted.")
        {
        }

        public InvalidCommandException(Exception innerException) : base("Invalid command.The command was improperly formatted.", innerException)
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class InvalidParameterException : Exception
    {
        public InvalidParameterException() : base("Invalid parameter. One of the parameters (for example, the NotificationUID) does not refer to an existing object on the NP.")
        {
        }

        public InvalidParameterException(Exception innerException) : base("Invalid parameter. One of the parameters (for example, the NotificationUID) does not refer to an existing object on the NP.", innerException)
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ActionFailedException : Exception
    {
        public ActionFailedException() : base("Action failed. The action was not performed.")
        {
        }

        public ActionFailedException(Exception innerException) : base("Action failed. The action was not performed.", innerException)
        {
        }
    }


}
