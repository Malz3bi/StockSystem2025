using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace StockSystem2025.Models;

public partial class StockdbContext : DbContext
{
    public StockdbContext()
    {
    }

    public StockdbContext(DbContextOptions<StockdbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CompanyDetailsView> CompanyDetailsViews { get; set; }

    public virtual DbSet<CompanyTable> CompanyTables { get; set; }

    public virtual DbSet<Criteria> Criterias { get; set; }

    public virtual DbSet<DigitalAnalysis> DigitalAnalyses { get; set; }

    public virtual DbSet<DigitalAnalysisDatum> DigitalAnalysisData { get; set; }

    public virtual DbSet<EconLinksType> EconLinksTypes { get; set; }

    public virtual DbSet<EconomicLink> EconomicLinks { get; set; }

    public virtual DbSet<FollowList> FollowLists { get; set; }

    public virtual DbSet<FollowListCompany> FollowListCompanies { get; set; }

    public virtual DbSet<Formula> Formulas { get; set; }

    public virtual DbSet<GeneralDetailedStockView> GeneralDetailedStockViews { get; set; }

    public virtual DbSet<GeneralIndicatorView> GeneralIndicatorViews { get; set; }

    public virtual DbSet<Medium> Mediums { get; set; }

    public virtual DbSet<ProfessionalFibonacci> ProfessionalFibonaccis { get; set; }

    public virtual DbSet<ProfessionalFibonacciDatum> ProfessionalFibonacciData { get; set; }

    public virtual DbSet<RecommendationsResultsView> RecommendationsResultsViews { get; set; }

    public virtual DbSet<RefreshedPage> RefreshedPages { get; set; }

    public virtual DbSet<SectorDetailedStockView> SectorDetailedStockViews { get; set; }

    public virtual DbSet<SectorIndicatorVeiw> SectorIndicatorVeiws { get; set; }

    public virtual DbSet<Setting> Settings { get; set; }

    public virtual DbSet<StockPrevDayView> StockPrevDayViews { get; set; }

    public virtual DbSet<StockTable> StockTables { get; set; }

    public virtual DbSet<User> Users { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<CompanyDetailsView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("CompanyDetailsView");

            entity.Property(e => e.CIndicatorIn).HasColumnName("C_IndicatorIn");
            entity.Property(e => e.CIndicatorOut).HasColumnName("C_IndicatorOut");
            entity.Property(e => e.ParentIndicator)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.PrevSclose).HasColumnName("PrevSclose");
            entity.Property(e => e.Sclose).HasColumnName("Sclose");
            entity.Property(e => e.Sdate)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Shigh).HasColumnName("Shigh");
            entity.Property(e => e.Slow).HasColumnName("Slow");
            entity.Property(e => e.Sname).HasMaxLength(50);
            entity.Property(e => e.Sticker)
                .HasMaxLength(4)
                .IsUnicode(false);
        });

        modelBuilder.Entity<CompanyTable>(entity =>
        {
            entity.HasKey(e => e.CompanyCode).HasName("PK__tmp_ms_x__11A0134A8B5287C1");

            entity.ToTable("CompanyTable");

            entity.HasIndex(e => e.ParentIndicator, "IX_CompanyTable_ParentIndicator");

            entity.Property(e => e.CompanyCode)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.CompanyId).ValueGeneratedOnAdd();
            entity.Property(e => e.CompanyName).HasMaxLength(50);
            entity.Property(e => e.Follow).HasDefaultValue(false);
            entity.Property(e => e.ParentIndicator)
                .HasMaxLength(4)
                .IsUnicode(false);

            entity.HasOne(d => d.ParentIndicatorNavigation).WithMany(p => p.InverseParentIndicatorNavigation)
                .HasForeignKey(d => d.ParentIndicator)
                .HasConstraintName("FK_CompanyTable_CompanyTable");
        });

        modelBuilder.Entity<Criteria>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Color)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ImageUrl)
                .IsUnicode(false)
                .HasColumnName("ImageURL");
            entity.Property(e => e.IsGeneral).HasDefaultValue(false);
            entity.Property(e => e.IsIndicator).HasComment("0  = false, 1 = true, 2 = all");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Note).HasColumnType("ntext");
            entity.Property(e => e.Separater).HasMaxLength(100);
            entity.Property(e => e.Type).HasMaxLength(100);
            entity.Property(e => e.UserId)
                .HasDefaultValue(1)
                .HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Criteria)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Criterias_Users");
        });

        modelBuilder.Entity<DigitalAnalysis>(entity =>
        {
            entity.ToTable("DigitalAnalysis");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CompanyCode)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.ShowDescriptionColumn).HasDefaultValue(true);
            entity.Property(e => e.WavesCount).HasDefaultValue(1);
            entity.Property(e => e.WavesVisibility)
                .HasMaxLength(400)
                .IsUnicode(false);
        });

        modelBuilder.Entity<DigitalAnalysisDatum>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Color)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DigitalAnalysisId).HasColumnName("DigitalAnalysisID");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Visible).HasDefaultValue(true);

            entity.HasOne(d => d.DigitalAnalysis).WithMany(p => p.DigitalAnalysisData)
                .HasForeignKey(d => d.DigitalAnalysisId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DigitalAnalysisData_DigitalAnalysis");
        });

        modelBuilder.Entity<EconLinksType>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.TypeName).HasMaxLength(200);
        });

        modelBuilder.Entity<EconomicLink>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.TypeId).HasColumnName("TypeID");

            entity.HasOne(d => d.Type).WithMany(p => p.EconomicLinks)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EconomicLinks_EconLinksTypes");
        });

        modelBuilder.Entity<FollowList>(entity =>
        {
            entity.ToTable("FollowList");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Color)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.UserId)
                .HasDefaultValue(1)
                .HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.FollowLists)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FollowList_Users");
        });

        modelBuilder.Entity<FollowListCompany>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CompanyCode)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.FollowListId).HasColumnName("FollowListID");

            entity.HasOne(d => d.CompanyCodeNavigation).WithMany(p => p.FollowListCompanies)
                .HasForeignKey(d => d.CompanyCode)
                .HasConstraintName("FK_FollowListCompanies_CompanyTable");

            entity.HasOne(d => d.FollowList).WithMany(p => p.FollowListCompanies)
                .HasForeignKey(d => d.FollowListId)
                .HasConstraintName("FK_FollowListCompanies_FollowList");
        });

        modelBuilder.Entity<Formula>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CriteriaId).HasColumnName("CriteriaID");
            entity.Property(e => e.FormulaValues)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.Criteria).WithMany(p => p.Formulas)
                .HasForeignKey(d => d.CriteriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Formulas_Criterias");
        });

        modelBuilder.Entity<GeneralDetailedStockView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("GeneralDetailedStockView");

            entity.Property(e => e.CIndicatorIn).HasColumnName("C_IndicatorIn");
            entity.Property(e => e.CIndicatorOut).HasColumnName("C_IndicatorOut");
            entity.Property(e => e.GIndicatorIn).HasColumnName("G_IndicatorIn");
            entity.Property(e => e.GIndicatorOut).HasColumnName("G_IndicatorOut");
            entity.Property(e => e.PrevSclose).HasColumnName("PrevSclose");
            entity.Property(e => e.SIndicatorIn).HasColumnName("S_IndicatorIn");
            entity.Property(e => e.SIndicatorOut).HasColumnName("S_IndicatorOut");
            entity.Property(e => e.Sclose).HasColumnName("Sclose");
            entity.Property(e => e.Sdate)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Shigh).HasColumnName("Shigh");
            entity.Property(e => e.Slow).HasColumnName("Slow");
            entity.Property(e => e.Sname).HasMaxLength(50);
            entity.Property(e => e.Sticker)
                .HasMaxLength(4)
                .IsUnicode(false);
        });

        modelBuilder.Entity<GeneralIndicatorView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("GeneralIndicatorView");

            entity.Property(e => e.Sticker)
                .HasMaxLength(4)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Medium>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ForChart).HasDefaultValue(true);
            entity.Property(e => e.ForTable).HasDefaultValue(true);
            entity.Property(e => e.UserId)
                .HasDefaultValue(1)
                .HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Media)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mediums_Users");
        });

        modelBuilder.Entity<ProfessionalFibonacci>(entity =>
        {
            entity.ToTable("ProfessionalFibonacci");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CompanyCode)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.ShowDescriptionColumn).HasDefaultValue(true);
        });

        modelBuilder.Entity<ProfessionalFibonacciDatum>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Color)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ProfessionalFibonacciId).HasColumnName("ProfessionalFibonacciID");
            entity.Property(e => e.Visible).HasDefaultValue(true);

            entity.HasOne(d => d.ProfessionalFibonacci).WithMany(p => p.ProfessionalFibonacciData)
                .HasForeignKey(d => d.ProfessionalFibonacciId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProfessionalFibonacciData_ProfessionalFibonacci");
        });

        modelBuilder.Entity<RecommendationsResultsView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("RecommendationsResultsView");

            entity.Property(e => e.ParentIndicator)
            .HasMaxLength(4)
            .IsUnicode(false);
            entity.Property(e => e.NextSclose).HasColumnName("NextSclose");
            entity.Property(e => e.NextShigh).HasColumnName("NextShigh");
            entity.Property(e => e.OpeningGapRate).HasColumnName("openingGapRate");
            entity.Property(e => e.OpeningGapValue).HasColumnName("openingGapValue");
            entity.Property(e => e.PrevSclose).HasColumnName("PrevSclose");
            entity.Property(e => e.PrevShigh).HasColumnName("PrevShigh");
            entity.Property(e => e.PrevSlow).HasColumnName("PrevSlow");
            entity.Property(e => e.Sclose).HasColumnName("Sclose");
            entity.Property(e => e.Sdate)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Shigh).HasColumnName("Shigh");
            entity.Property(e => e.Slow).HasColumnName("Slow");
            entity.Property(e => e.Sname).HasMaxLength(50);
            entity.Property(e => e.Sticker)
                .HasMaxLength(4)
                .IsUnicode(false);
        });

        modelBuilder.Entity<RefreshedPage>(entity =>
        {
            entity.ToTable("RefreshedPage");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.PageUrl)
                .IsUnicode(false)
                .HasColumnName("PageURL");
        });

        modelBuilder.Entity<SectorDetailedStockView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("SectorDetailedStockView");

            entity.Property(e => e.GIndicatorIn).HasColumnName("G_IndicatorIn");
            entity.Property(e => e.GIndicatorOut).HasColumnName("G_IndicatorOut");
            entity.Property(e => e.PrevSclose).HasColumnName("PrevSclose");
            entity.Property(e => e.SIndicatorIn).HasColumnName("S_IndicatorIn");
            entity.Property(e => e.SIndicatorOut).HasColumnName("S_IndicatorOut");
            entity.Property(e => e.Sclose).HasColumnName("Sclose");
            entity.Property(e => e.Sdate)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Shigh).HasColumnName("Shigh");
            entity.Property(e => e.Slow).HasColumnName("Slow");
            entity.Property(e => e.Sname).HasMaxLength(50);
            entity.Property(e => e.Sticker)
                .HasMaxLength(4)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SectorIndicatorVeiw>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("SectorIndicatorVeiw");

            entity.Property(e => e.ParentIndicator)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.Sticker)
                .HasMaxLength(4)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Setting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Settings__3214EC2795A6AAB3");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<StockPrevDayView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("StockPrevDayView");

            entity.Property(e => e.ParentIndicator)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.PrevSclose).HasColumnName("PrevSclose");
            entity.Property(e => e.PrevShigh).HasColumnName("PrevShigh");
            entity.Property(e => e.PrevSlow).HasColumnName("PrevSlow");
            entity.Property(e => e.Sclose).HasColumnName("Sclose");
            entity.Property(e => e.Sdate)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Shigh).HasColumnName("Shigh");
            entity.Property(e => e.Slow).HasColumnName("Slow");
            entity.Property(e => e.Sname).HasMaxLength(50);
            entity.Property(e => e.Sticker)
                .HasMaxLength(4)
                .IsUnicode(false);
        });

        modelBuilder.Entity<StockTable>(entity =>
        {
            entity.HasKey(e => new { e.Sticker, e.Sdate });

            entity.ToTable("StockTable");

            entity.Property(e => e.Sticker)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.Sdate)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Sclose).HasColumnName("SClose");
            entity.Property(e => e.Shigh).HasColumnName("SHigh");
            entity.Property(e => e.Slow).HasColumnName("SLow");
            entity.Property(e => e.Sname).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "IX_Users_Email").IsUnique();

            entity.HasIndex(e => e.UserName, "IX_Users_UserName").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Bundle).HasDefaultValue(1);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.EmailCode).HasMaxLength(50);
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);




}
