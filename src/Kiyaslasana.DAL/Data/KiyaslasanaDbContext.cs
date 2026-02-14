using Kiyaslasana.EL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kiyaslasana.DAL.Data;

public sealed class KiyaslasanaDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    // Change this constant if the existing physical table name differs in production.
    public const string TelefonlarTableName = "telefonlar";

    public KiyaslasanaDbContext(DbContextOptions<KiyaslasanaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Telefon> Telefonlar => Set<Telefon>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Telefon>(entity =>
        {
            entity.ToTable(TelefonlarTableName);
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.Marka).HasMaxLength(450);
            entity.HasIndex(x => new { x.Marka, x.Slug });

            // Keep exact compatibility for non-standard numeric column names.
            entity.Property(x => x.Ses35MmJack).HasColumnName("ses_3_5mm_jack");
            entity.Property(x => x.Ses35MmJackTr).HasColumnName("ses_3_5mm_jack_tr");
        });
    }
}
