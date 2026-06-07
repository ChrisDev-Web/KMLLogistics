namespace E1___Sosa_Morales.Models;

public class HomeIndexViewModel
{
    public bool IsConnected { get; set; }

    public string Message { get; set; } = string.Empty;

    public string DatabaseName { get; set; } = "KMLLogistics";

    public string? ServerName { get; set; }
}
