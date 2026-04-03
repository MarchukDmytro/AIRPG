using AIRPG.Features;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace AIRPG.Core.Data;

public class ItemDbContext : DbContext
{
    public DbSet<MetaItem> Items { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder o)
    {
        string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Items.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        o.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<MetaItem>()
            .HasDiscriminator<string>("ItemType")
            .HasValue<WeaponData>("Weapon")
            .HasValue<ArmorData>("Armor");

        b.Entity<MetaItem>()
            .Ignore(x => x.ItemImage)
            .Ignore(x => x.Type);

        b.Entity<WeaponData>()
            .OwnsMany(w => w.Damage, d => d.ToJson());
    }
}