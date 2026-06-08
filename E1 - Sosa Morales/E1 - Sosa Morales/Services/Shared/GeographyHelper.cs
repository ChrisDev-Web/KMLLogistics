using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.Shared;

public static class GeographyHelper
{
    public static async Task<GeographyInfo?> GetByDistrictIdAsync(ApplicationDbContext context, int idDistrict)
    {
        var rows = await context.Database.SqlQueryRaw<GeographyInfo>(
            "EXEC dbo.sp_district_geography_get_by_id @id_district",
            new SqlParameter("@id_district", idDistrict)).ToListAsync();

        return rows.FirstOrDefault();
    }
}
