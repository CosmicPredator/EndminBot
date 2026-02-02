using Microsoft.EntityFrameworkCore;

namespace EndfieldBot.DB;

public class EndfieldBotDbContext : DbContext
{
    public EndfieldBotDbContext(DbContextOptions<EndfieldBotDbContext> options)
        : base(options)
    {
        //Database.EnsureCreated();
    }

    public DbSet<RedeemCodes> RedeemCodes => Set<RedeemCodes>();
    public DbSet<CurrentEvents> CurrentEvents => Set<CurrentEvents>();
}