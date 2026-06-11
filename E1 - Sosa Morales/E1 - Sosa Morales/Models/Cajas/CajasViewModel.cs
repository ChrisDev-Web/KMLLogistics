using E1___Sosa_Morales.Models.Dashboard;

namespace E1___Sosa_Morales.Models.Cajas;

public class CajasViewModel
{
    public ModuleViewModel Module { get; set; } = new();
}

public class BoxListItem { public int IdBox { get; set; } public decimal? Weight { get; set; } public decimal? Height { get; set; } public decimal? Width { get; set; } public decimal? Length { get; set; } public decimal? Volume { get; set; } public byte Status { get; set; } public int TotalCount { get; set; } }
public class BoxDetail { public int IdBox { get; set; } public decimal? Weight { get; set; } public decimal? Height { get; set; } public decimal? Width { get; set; } public decimal? Length { get; set; } public decimal? Volume { get; set; } public byte Status { get; set; } public DateTime? CreatedAt { get; set; } public DateTime? UpdatedAt { get; set; } }
public class BoxSpResult { public int Success { get; set; } public string Message { get; set; } = ""; public int? IdBox { get; set; } }
public class BoxActionResult { public int Success { get; set; } public string Message { get; set; } = ""; }
public class BoxOption { public int IdBox { get; set; } public string Name { get; set; } = ""; }
