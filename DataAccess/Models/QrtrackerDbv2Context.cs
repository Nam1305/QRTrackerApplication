using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccess.Models;

public partial class QrtrackerDbv2Context : DbContext
{
    public QrtrackerDbv2Context()
    {
    }

    public QrtrackerDbv2Context(DbContextOptions<QrtrackerDbv2Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<ErrorLog> ErrorLogs { get; set; }

    public virtual DbSet<GeneratedTray> GeneratedTrays { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<TrayScan> TrayScans { get; set; }

    public virtual DbSet<WorkSession> WorkSessions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory());
        //builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        var configuration = builder.Build();
        var connectionStringUrl = Path.Combine(Directory.GetCurrentDirectory(), "Database", "QRTrackerDBV2.db");
        optionsBuilder.UseSqlite($"Data Source={connectionStringUrl}");
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.SupervisorId);

            entity.ToTable("Account");

            entity.HasIndex(e => e.Username, "IX_Account_Username").IsUnique();
        });

        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.ToTable("ErrorLog");

            entity.Property(e => e.ErrorLogId).HasColumnName("ErrorLogID");
            entity.Property(e => e.ErrorDate).HasColumnType("DATE");
            entity.Property(e => e.ErrorTime).HasColumnType("TIME");
            entity.Property(e => e.SessionId).HasColumnName("SessionID");

            entity.HasOne(d => d.Session).WithMany(p => p.ErrorLogs).HasForeignKey(d => d.SessionId);
        });

        modelBuilder.Entity<GeneratedTray>(entity =>
        {
            entity.ToTable("GeneratedTray");

            entity.Property(e => e.GeneratedTrayId).HasColumnName("GeneratedTrayID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
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
        });

        modelBuilder.Entity<TrayScan>(entity =>
        {
            entity.ToTable("TrayScan");

            entity.Property(e => e.TrayScanId).HasColumnName("TrayScanID");
            entity.Property(e => e.GeneratedTrayId).HasColumnName("GeneratedTrayID");
            entity.Property(e => e.ScanDate).HasColumnType("DATE");
            entity.Property(e => e.ScanTime).HasColumnType("TIME");
            entity.Property(e => e.SessionId).HasColumnName("SessionID");
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
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.ScanDate).HasColumnType("DATE");
            entity.Property(e => e.ScanTime).HasColumnType("TIME");

            entity.HasOne(d => d.Product).WithMany(p => p.WorkSessions)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
