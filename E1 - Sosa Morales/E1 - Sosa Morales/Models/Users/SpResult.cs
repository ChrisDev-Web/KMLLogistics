using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.Users;

public class SpResult
{
    [Column("success")]
    public int Success { get; set; }

    [Column("message")]
    public string Message { get; set; } = string.Empty;
}
