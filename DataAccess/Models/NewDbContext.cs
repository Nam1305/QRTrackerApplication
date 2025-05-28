using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Models;

public partial class NewDbContext : DbContext
{
    public NewDbContext()
    {
    }

    public NewDbContext(DbContextOptions<NewDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<GeneratedTray> GeneratedTrays { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<TrayScan> TrayScans { get; set; }

    public virtual DbSet<WorkSession> WorkSessions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlite("Data Source=C:/Users/vnintern01/Downloads/NewDB.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GeneratedTray>(entity =>
        {
            entity.ToTable("GeneratedTray");

            entity.Property(e => e.GeneratedTrayId).HasColumnName("GeneratedTrayID");
            entity.Property(e => e.ProductId)
                .HasColumnType("integer(10)")
                .HasColumnName("ProductID");
            entity.Property(e => e.QrcodeContent).HasColumnName("QRCodeContent");

            entity.HasOne(d => d.Product).WithMany(p => p.GeneratedTrays)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Product");

            entity.HasIndex(e => e.ProductCode, "IX_Product_ProductCode").IsUnique();

            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.QuantityPerBox).HasColumnType("integer(10)");
            entity.Property(e => e.QuantityPerTray).HasColumnType("integer(10)");
            entity.Property(e => e.TrayPerBox).HasColumnType("integer(10)");
        });

        modelBuilder.Entity<TrayScan>(entity =>
        {
            entity.ToTable("TrayScan");

            entity.Property(e => e.TrayScanId).HasColumnName("TrayScanID");
            entity.Property(e => e.GeneratedTrayId)
                .HasColumnType("integer(10)")
                .HasColumnName("GeneratedTrayID");
            entity.Property(e => e.ScanDate).HasColumnType("date");
            entity.Property(e => e.ScanTime).HasColumnType("time");
            entity.Property(e => e.SessionId)
                .HasColumnType("integer(10)")
                .HasColumnName("SessionID");
            entity.Property(e => e.TrayQrcode).HasColumnName("TrayQRCode");

            entity.HasOne(d => d.GeneratedTray).WithMany(p => p.TrayScans)
                .HasForeignKey(d => d.GeneratedTrayId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Session).WithMany(p => p.TrayScans)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<WorkSession>(entity =>
        {
            entity.HasKey(e => e.SessionId);

            entity.ToTable("WorkSession");

            entity.HasIndex(e => e.BoxSequence, "IX_WorkSession_BoxSequence").IsUnique();

            entity.Property(e => e.SessionId).HasColumnName("SessionID");
            entity.Property(e => e.ProductId)
                .HasColumnType("integer(10)")
                .HasColumnName("ProductID");
            entity.Property(e => e.ScanDate).HasColumnType("date");
            entity.Property(e => e.ScanTime).HasColumnType("time");

            entity.HasOne(d => d.Product).WithMany(p => p.WorkSessions)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
