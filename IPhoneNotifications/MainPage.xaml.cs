using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using IPhoneNotifications.AppleNotificationCenterService;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Data.Xml.Dom;
using Microsoft.QueryStringDotNET;
using Windows.UI.Notifications;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IPhoneNotifications
{
    public enum NotifyType
    {
        StatusMessage,
        ErrorMessage
    };

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;

        public NotificationConsumer ANCS;

        public string SelectedBleDeviceId;

        public string SelectedBleDeviceName = "No device selected";

        public MainPage()
        {
            this.InitializeComponent();

            // This is a static public property that allows downstream pages to get a handle to the MainPage instance
            // in order to call methods that are in this class.
            Current = this;

            ANCS = new NotificationConsumer();
            ANCS.NotificationAdded += ANCS_NotificationAdded;
            ANCS.NotificationRemoved += ANCS_NotificationRemoved;
        }

        private void ANCS_NotificationRemoved(NotificationConsumer sender, NotificationSourceData args)
        {
            var toastHistory = ToastNotificationManager.History;

            try
            {
                toastHistory.Remove(args.NotificationUID.ToString());
            }
            catch (Exception)
            {
                // Just be silent
            }
        }

        private void ANCS_NotificationAdded(NotificationConsumer sender, AppleNotificationEventArgs args)
        {
            XmlDocument toastXml = null;

            ToastVisual toastVisual = new ToastVisual()
            {
                BindingGeneric = new ToastBindingGeneric()
                {
                    Children = {
                        new AdaptiveText()
                        {
                            Text = args.NotificationAttributes[NotificationAttributeID.Title]
                        },
                        new AdaptiveText
                        {
                            Text = args.NotificationAttributes[NotificationAttributeID.Message]
                        }
                    },
                },
            };

            // toast actions
            ToastActionsCustom toastActions = new ToastActionsCustom();

            if (args.NotificationSource.EventFlags.HasFlag(EventFlags.EventFlagPositiveAction))
            {
                string positiveActionLabel = "Positive";
                if (args.NotificationAttributes.ContainsKey(NotificationAttributeID.PositiveActionLabel))
                {
                    positiveActionLabel = args.NotificationAttributes[NotificationAttributeID.PositiveActionLabel];
                }

                toastActions.Buttons.Add(new ToastButton(positiveActionLabel, new QueryString() {
                    {"action", "positive" },
                    {"uid", args.NotificationAttributes.NotificationUID.ToString() }
                }.ToString())
                {
                    ActivationType = ToastActivationType.Foreground
                });
            }

            if (args.NotificationSource.EventFlags.HasFlag(EventFlags.EventFlagNegativeAction))
            {
                string negativeActionLabel = "Negative";
                if (args.NotificationAttributes.ContainsKey(NotificationAttributeID.NegativeActionLabel))
                {
                    negativeActionLabel = args.NotificationAttributes[NotificationAttributeID.NegativeActionLabel];
                }

                toastActions.Buttons.Add(new ToastButton(negativeActionLabel, new QueryString() {
                    {"action", "negative" },
                    {"uid", args.NotificationAttributes.NotificationUID.ToString() }
                }.ToString())
                {
                    ActivationType = ToastActivationType.Foreground
                });
            }

            ToastContent toastContent = new ToastContent()
            {
                Visual = toastVisual,
                Scenario = ToastScenario.Default,
                Actions = toastActions,
            };

            toastXml = toastContent.GetXml();

            ToastNotification toastNotification = new ToastNotification(toastXml)
            {
                ExpirationTime = DateTime.Now.AddMinutes(5),
                Tag = args.NotificationSource.NotificationUID.ToString()
            };

            ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
        }

        /// <summary>
        /// Used to display messages to the user
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="type"></param>
        public void NotifyUser(string strMessage, NotifyType type)
        {
            switch (type)
            {
                case NotifyType.StatusMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                    break;
                case NotifyType.ErrorMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                    break;
            }
            StatusBlock.Text = strMessage;

            // Collapse the StatusBlock if it has no text to conserve real estate.
            StatusBorder.Visibility = (StatusBlock.Text != String.Empty) ? Visibility.Visible : Visibility.Collapsed;
            if (StatusBlock.Text != String.Empty)
            {
                StatusBorder.Visibility = Visibility.Visible;
                StatusPanel.Visibility = Visibility.Visible;
            }
            else
            {
                StatusBorder.Visibility = Visibility.Collapsed;
                StatusPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void BluetoothButton_OnClick(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as FrameworkElement;
            if (radioButton != null)
            {
                this.MainFrame.Navigate(typeof(BluetoothServer), radioButton.Tag);
            }
        }

        private void Hamburger_Click(object sender, RoutedEventArgs e)
        {
            Splitter.IsPaneOpen = !Splitter.IsPaneOpen;
        }
    }
}
