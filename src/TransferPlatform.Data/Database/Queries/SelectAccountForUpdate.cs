using Microsoft.EntityFrameworkCore;

namespace TransferPlatform.Data.Database.Queries
{
    public static class SelectAccountForUpdate
    {
        public static string PrepareQuery(AppDbContext context)
        {
            return context.Database.IsSqlite() ?
            """
                SELECT *
                FROM "Accounts"
                WHERE "Id" = {0}
            """
            :
            """
                SELECT *
                FROM "Accounts"
                WHERE "Id" = {0}
                FOR UPDATE
            """;
        }
    }
}
