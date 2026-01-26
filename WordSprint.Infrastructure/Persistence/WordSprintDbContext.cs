using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WordSprint.Core.Entities;
using WordSprint.Infrastructure.Identity;

namespace WordSprint.Infrastructure.Persistence;

public class WordSprintDbContext : IdentityDbContext<ApplicationUser>
{
    public WordSprintDbContext(DbContextOptions<WordSprintDbContext> options) : base(options) { }

    public DbSet<Word> Words => Set<Word>();
    public DbSet<UserWord> UserWords => Set<UserWord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Word>(e =>
        {
            e.Property(x => x.English).HasMaxLength(100).IsRequired();
            e.Property(x => x.Turkish).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.English);
        });

        builder.Entity<UserWord>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.WordId }).IsUnique();

            e.HasOne(x => x.Word)
             .WithMany()
             .HasForeignKey(x => x.WordId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
