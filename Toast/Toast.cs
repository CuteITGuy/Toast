using System;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.UI.Notifications;


namespace CB.Windows.UI
{
    public class Toast
    {
        #region  Properties & Indexers
        public DateTimeOffset? ExpirationTime { get; set; }
        public string ImageSource { get; set; } = "";
        public string[] Lines { get; set; }
        #endregion


        #region Events
        public event TypedEventHandler<ToastNotification, object> Activated;
        public event TypedEventHandler<ToastNotification, ToastDismissedEventArgs> Dismissed;
        public event TypedEventHandler<ToastNotification, ToastFailedEventArgs> Failed;
        #endregion


        #region Methods
        public void Show()
        {
            var toast = CreateToast();
            var toastNotifier = ToastNotificationManager.CreateToastNotifier("Toast");
            toastNotifier.Show(toast);
        }
        #endregion


        #region Event Handlers
        protected virtual void OnActivated(ToastNotification sender, object args)
        {
            Activated?.Invoke(sender, args);
        }

        protected virtual void OnDismissed(ToastNotification sender, ToastDismissedEventArgs args)
        {
            Dismissed?.Invoke(sender, args);
        }

        protected virtual void OnFailed(ToastNotification sender, ToastFailedEventArgs args)
        {
            Failed?.Invoke(sender, args);
        }
        #endregion


        #region Implementation
        private ToastNotification CreateToast()
        {
            var toastXml = CreateToastContent();
            var toast = new ToastNotification(toastXml);
            if (ExpirationTime.HasValue)
            {
                toast.ExpirationTime = ExpirationTime;
            }
            toast.Activated += OnActivated;
            toast.Dismissed += OnDismissed;
            toast.Failed += OnFailed;
            return toast;
        }

        private XmlDocument CreateToastContent()
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(GetToastTemplateType());

            var txtElements = toastXml.GetElementsByTagName("text");
            for (var i = 0; i < txtElements.Length; i++)
            {
                txtElements[i].AppendChild(toastXml.CreateTextNode(Lines[i]));
            }

            if (!string.IsNullOrWhiteSpace(ImageSource))
            {
                var imgElement = toastXml.GetElementsByTagName("image")[0] as XmlElement;
                imgElement?.SetAttribute("src", ImageSource);
            }
            return toastXml;
        }

        private ToastTemplateType GetToastTemplateType()
        {
            if (string.IsNullOrWhiteSpace(ImageSource))
            {
                switch (Lines.Length)
                {
                    case 1:
                        return ToastTemplateType.ToastText01;
                    case 2:
                        return IsLine1Longer() ? ToastTemplateType.ToastText03 : ToastTemplateType.ToastText02;
                    case 3:
                        return ToastTemplateType.ToastText04;
                    default:
                        throw new Exception();
                }
            }
            switch (Lines.Length)
            {
                case 1:
                    return ToastTemplateType.ToastImageAndText01;
                case 2:
                    return IsLine1Longer()
                               ? ToastTemplateType.ToastImageAndText03 : ToastTemplateType.ToastImageAndText02;
                case 3:
                    return ToastTemplateType.ToastImageAndText04;
                default:
                    throw new Exception();
            }
        }

        private bool IsLine1Longer()
        {
            return !string.IsNullOrWhiteSpace(Lines[0]) &&
                   (string.IsNullOrWhiteSpace(Lines[1]) || Lines[0].Length > Lines[1].Length);
        }
        #endregion
    }
}