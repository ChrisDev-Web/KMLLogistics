using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
