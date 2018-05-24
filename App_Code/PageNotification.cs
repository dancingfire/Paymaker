using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Web;
using System.Web.UI;

/// <summary>
/// A single page notification.
/// NOTE: This class must be session serializable. Any attributes MUST be added to the serializable methods at the end of the class.
/// </summary>
///

public enum PageNotificationType {
    Success = 0,
    Error = 1,
    Warning = 2,
    Info = 3
}

/// <summary>
/// Controls the notifications that appears on the page. This is done by adding notifications from anywhere to this manager. The manager will display them once when the user loads the page.
/// Every page in the app can decided where to place the message. Messages will only be shown if the page contains a Panel called pPageNotification.
/// </summary>
[Serializable()]
public class PageNotificationManager : ISerializable {
    private ArrayList alMessages = new ArrayList();

    public PageNotificationManager() {
    }

    /// <summary>
    /// Auto instantiate the object
    /// </summary>
    public static PageNotificationManager PageNotificationInfo {
        get {
            if (null == HttpContext.Current.Session["PageNotification"])
                HttpContext.Current.Session["PageNotification"] = new PageNotificationManager();
            else {
                PageNotificationManager oP = null;
                try {
                    oP = (PageNotificationManager)HttpContext.Current.Session["PageNotification"];
                } catch {
                    ;
                } finally {
                    if (oP == null)
                        HttpContext.Current.Session["PageNotification"] = new PageNotificationManager();
                    else
                        HttpContext.Current.Session["PageNotification"] = oP;
                }
            }
            return (PageNotificationManager)HttpContext.Current.Session["PageNotification"];
        }
    }

    /// <summary>
    /// Add a new message to the page notices
    /// </summary>
    /// <param name="NotificationText"></param>
    /// <param name="NotificationType"></param>
    public void addPageNotification(PageNotificationType NotificationType, string Title, string Notice, bool Closable = true, string CssStyle = "") {
        if (String.IsNullOrEmpty(Title))
            Title = "Notification";
        alMessages.Add(new PageNotification(NotificationType, Title, Notice, Closable, CssStyle));
        HttpContext.Current.Session["PageNotification"] = this;
    }

    /// <summary>
    /// Clear the notice
    /// </summary>
    public void clearNotifications() {
        alMessages.Clear();
        HttpContext.Current.Session["PageNotification"] = this;
    }

    /// <summary>
    /// Creates a panel to display the page notification. If order for the panel to display, the page must have a hidden panel called pPageNotification
    /// </summary>
    public void showPageNotification(bool rootDirectoryPage = false) {
        Control oContainer = null;
        //Check to see if we can show messages on this load
        if (G.oRoot != null) {
            oContainer = G.oRoot.FindControl("pPageNotification");
            if (oContainer == null)
                return;
        }

        if (alMessages.Count == 0)
            return;

        string szCSSClass = "";
        foreach (PageNotification oPN in alMessages) {
            string szNotice = @"
                <img src='../sys_images/{3}' class='NoticeCloseIcon' onclick=""$(this).parent().slideUp().hide();"" title='Close this message' onmouseover=""this.src='../sys_images/{4}';"" onmouseout=""this.src='../sys_images/{3}';"" />
                <div class='NoticeContent'>
                    <div class='{0}'>{1}</div>
                    <div style='float: left; clear: left; width: 95%'>{2}</div>
                </div>
            ";

            if (rootDirectoryPage)
                szNotice = szNotice.Replace("../", "./");

            if (!oPN.Closable)
                szNotice = szNotice.Replace("class='NoticeCloseIcon'", "class='NoticeCloseIcon' style='display: none;'");

            switch (oPN.NotificationType) {
                case PageNotificationType.Success:
                    szCSSClass = "NoticeSuccess";
                    szNotice = String.Format(szNotice, "NoticeSuccessHeader", oPN.Title, oPN.Notification, "NoticeSuccessClose.jpg", "NoticeSuccessCloseMO.jpg");
                    break;

                case PageNotificationType.Error:
                    szCSSClass = "NoticeError";
                    szNotice = String.Format(szNotice, "NoticeErrorHeader", oPN.Title, oPN.Notification, "NoticeErrorClose.jpg", "NoticeErrorCloseMO.jpg");
                    break;

                case PageNotificationType.Warning:
                    szCSSClass = "NoticeWarning";
                    szNotice = String.Format(szNotice, "NoticeWarningHeader", oPN.Title, oPN.Notification, "NoticeWarningClose.jpg", "NoticeWarningCloseMO.jpg");
                    break;

                case PageNotificationType.Info:
                    szCSSClass = "NoticeInfo";
                    szNotice = String.Format(szNotice, "NoticeInfoHeader", oPN.Title, oPN.Notification, "NoticeInfoClose.jpg", "NoticeInfoCloseMO.jpg");
                    break;
            }
            LiteralControl oNote = HTML.createDiv("dPageNotification", "Notice " + szCSSClass, szNotice, oPN.CssStyle);
            oContainer.Controls.Add(oNote);
            oContainer.Visible = true;
        }
        clearNotifications();
    }

    public PageNotificationManager(SerializationInfo info, StreamingContext ctxt) {
        //Get the values from info and assign them to the appropriate properties
        alMessages = (ArrayList)info.GetValue("alMessages", typeof(ArrayList));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext ctxt) {
        info.AddValue("alMessages", alMessages);
    }
}

[Serializable()]
public class PageNotification : ISerializable {
    private string szNotification = "";
    private string szTitle = "";
    private PageNotificationType Type = PageNotificationType.Info;
    private bool blnClosable = true;
    private string szCssStyle = "";

    public PageNotification(PageNotificationType Type, string Title, string Notification, bool Closable, string CssStyle) {
        this.szNotification = Notification;
        this.szTitle = Title;
        this.Type = Type;
        this.blnClosable = Closable;
        this.szCssStyle = CssStyle;
    }

    /// <summary>
    /// Text of the notification
    /// </summary>
    public string Notification {
        get { return szNotification; }
    }

    /// <summary>
    /// Title of the notification
    /// </summary>
    public string Title {
        get { return szTitle; }
    }

    /// <summary>
    /// Type of notification
    /// </summary>
    public PageNotificationType NotificationType {
        get { return Type; }
    }

    /// <summary>
    /// Is the notification closable
    /// </summary>
    public bool Closable {
        get { return blnClosable; }
    }

    /// <summary>
    /// Css attributes to be included in the "style" tag for this notification
    /// </summary>
    public string CssStyle {
        get { return szCssStyle; }
    }

    public PageNotification(SerializationInfo info, StreamingContext ctxt) {
        //Get the values from info and assign them to the appropriate properties
        szNotification = (String)info.GetValue("Notice", typeof(string));
        szTitle = (String)info.GetValue("Title", typeof(string));
        Type = (PageNotificationType)info.GetValue("Type", typeof(int));
        blnClosable = (Boolean)info.GetValue("Closable", typeof(bool));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext ctxt) {
        info.AddValue("Title", szTitle);
        info.AddValue("Notice", szNotification);
        info.AddValue("Type", (int)Type);
        info.AddValue("Closable", blnClosable);
    }
}