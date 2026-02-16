using Kiyaslasana.EL.Constants;
using Kiyaslasana.EL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kiyaslasana.DAL.Data;

public sealed class KiyaslasanaDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    // Change this constant if the existing physical table name differs in production.
    public const string TelefonlarTableName = "telefonlar";
    public const string BlogPostsTableName = "blog_posts";

    public KiyaslasanaDbContext(DbContextOptions<KiyaslasanaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Telefon> Telefonlar => Set<Telefon>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Telefon>(entity =>
        {
            entity.ToTable(TelefonlarTableName);
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Slug).HasMaxLength(TelefonConstraints.SlugMaxLength);
            entity.Property(x => x.Marka).HasMaxLength(TelefonConstraints.MarkaMaxLength);
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.HasIndex(x => new { x.Marka, x.Slug });

            // Keep exact compatibility for non-standard numeric column names.
            entity.Property(x => x.Ses35MmJack).HasColumnName("ses_3_5mm_jack");
            entity.Property(x => x.Ses35MmJackTr).HasColumnName("ses_3_5mm_jack_tr");
        });

        builder.Entity<BlogPost>(entity =>
        {
            entity.ToTable(BlogPostsTableName);
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title)
                .HasMaxLength(BlogPostConstraints.TitleMaxLength)
                .IsRequired();
            entity.Property(x => x.Slug)
                .HasMaxLength(BlogPostConstraints.SlugMaxLength)
                .IsRequired();
            entity.Property(x => x.Excerpt)
                .HasMaxLength(BlogPostConstraints.ExcerptMaxLength);
            entity.Property(x => x.MetaTitle)
                .HasMaxLength(BlogPostConstraints.MetaTitleMaxLength);
            entity.Property(x => x.MetaDescription)
                .HasMaxLength(BlogPostConstraints.MetaDescriptionMaxLength);
            entity.Property(x => x.ContentRaw).IsRequired();
            entity.Property(x => x.ContentSanitized).IsRequired();

            entity.HasIndex(x => x.Slug).IsUnique();
            entity.HasIndex(x => x.PublishedAt);
        });
    }
}
