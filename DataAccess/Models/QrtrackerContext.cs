using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace DataAccess.Models;

public partial class QrtrackerContext : DbContext
{
    public QrtrackerContext()
    {
    }

    public QrtrackerContext(DbContextOptions<QrtrackerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Box> Boxes { get; set; }

    public virtual DbSet<Kanban> Kanbans { get; set; }

    public virtual DbSet<PackingRecord> PackingRecords { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory());
        builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        var configuration = builder.Build();
        optionsBuilder.UseSqlite(configuration.GetConnectionString("Default"));
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Box>(entity =>
        {
            entity.HasIndex(e => e.ProductCode, "IX_Boxes_ProductCode").IsUnique();

            entity.Property(e => e.BoxId).HasColumnName("BoxID");
            entity.Property(e => e.ProductCode).HasColumnType("varchar(255)");
            entity.Property(e => e.Qrcontent)
                .HasColumnType("varchar(255)")
                .HasColumnName("QRContent");
            entity.Property(e => e.QuantityPerBox).HasColumnType("integer(10)");
            entity.Property(e => e.QuantityPerTray).HasColumnType("integer(3)");
            entity.Property(e => e.Status).HasColumnType("varchar(255)");
            entity.Property(e => e.TrayPerBox).HasColumnType("INTEGER(10)");
        });

        modelBuilder.Entity<Kanban>(entity =>
        {
            entity.Property(e => e.KanbanId).HasColumnName("KanbanID");
            entity.Property(e => e.KanBanSequence).HasColumnType("varchar(255)");
            entity.Property(e => e.ProductCode).HasColumnType("varchar(255)");
            entity.Property(e => e.Qrcontent)
                .HasColumnType("varchar(255)")
                .HasColumnName("QRContent");
            entity.Property(e => e.QuantityPerBox).HasColumnType("integer(10)");
            entity.Property(e => e.QuantityPerTray).HasColumnType("integer(10)");
            entity.Property(e => e.Status).HasColumnType("varchar(255)");
            entity.Property(e => e.TrayPerBox).HasColumnType("INTEGER(10)");
        });

        modelBuilder.Entity<PackingRecord>(entity =>
        {
            entity.Property(e => e.PackingRecordId).HasColumnName("PackingRecordID");
            entity.Property(e => e.BoxId)
                .HasColumnType("integer(10)")
                .HasColumnName("BoxID");
            entity.Property(e => e.KanbanId)
                .HasColumnType("integer(10)")
                .HasColumnName("KanbanID");
            entity.Property(e => e.ScanDate).HasColumnType("date");
            entity.Property(e => e.ScanTime).HasColumnType("time");

            entity.HasOne(d => d.Box).WithMany(p => p.PackingRecords)
                .HasForeignKey(d => d.BoxId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Kanban).WithMany(p => p.PackingRecords)
                .HasForeignKey(d => d.KanbanId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
