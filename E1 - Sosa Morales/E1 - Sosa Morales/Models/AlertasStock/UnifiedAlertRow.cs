namespace E1___Sosa_Morales.Models.AlertasStock;

public class UnifiedAlertRow
{
    public string Kind { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Level { get; set; } = "warning";
    public string LevelLabel { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public DateTime EventAt { get; set; }
    public int NotificationCount { get; set; }
    public bool IsActive { get; set; }
    public string? LastSentByUsername { get; set; }

    public string NotificationKey => $"{Kind}-{Id}-{EventAt.Ticks}";
}

public class AlertNotificationItem
{
    public string Key { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Level { get; set; } = "warning";
}
