using Radzen;

namespace MojePwa.Client.Extensions;

/// <summary>
/// Slouží ke zkrácení zápisů
/// </summary>
public static class NotificationsExtensions
{
    public static void Error(this NotificationService n, string summary, string detail = "", Action<NotificationMessage>? onClick = null)
        => n.Notify(NotificationSeverity.Error, summary, detail, click: onClick);

    public static void Warn(this NotificationService n, string summary, string detail = "", Action<NotificationMessage>? onClick = null)
        => n.Notify(NotificationSeverity.Warning, summary, detail, click: onClick);

    public static void Info(this NotificationService n, string summary, string detail = "", Action<NotificationMessage>? onClick = null)
        => n.Notify(NotificationSeverity.Info, summary, detail, click: onClick);

    public static void Success(this NotificationService n, string summary, string detail = "", Action<NotificationMessage>? onClick = null)
        => n.Notify(NotificationSeverity.Success, summary, detail, click: onClick);
}
