using Microsoft.EntityFrameworkCore;
using PharmacyWarehouse.Models;

namespace PharmacyWarehouse.Data;

public partial class PharmacyWarehouseContext : DbContext
{
    public PharmacyWarehouseContext()
    {
    }

    public PharmacyWarehouseContext(DbContextOptions<PharmacyWarehouseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Batch> Batches { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<DocumentLine> DocumentLines { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<InventoryResult> InventoryResults { get; set; }

    public virtual DbSet<MovementJournal> MovementJournals { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<WriteOff> WriteOffs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=localhost;Database=PharmacyWarehouse1;Trusted_Connection=True;TrustServerCertificate = True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Batch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Партии__3214EC27A3B57D1D");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ArrivalDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.BatchNumber).HasMaxLength(50);
            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.SellingPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Product).WithMany(p => p.Batches)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Партии__Товар_ID__5535A963");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Batches)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Партии__Поставщи__5629CD9C");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Категори__3214EC278AD91EA3");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Документ__3214EC278A955341");

            entity.HasIndex(e => e.Number, "UQ__Документ__063C4BF7E214F413").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Amount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Counterparty).HasMaxLength(200);
            entity.Property(e => e.Date).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Number).HasMaxLength(50);
            entity.Property(e => e.Type).HasMaxLength(20);

            entity.HasOne(d => d.Supplier).WithMany(p => p.Documents)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK__Документы__Поста__5CD6CB2B");
        });

        modelBuilder.Entity<DocumentLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Строки_д__3214EC2788683C48");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Batch).WithMany(p => p.DocumentLines)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Строки_до__Парти__60A75C0F");

            entity.HasOne(d => d.Document).WithMany(p => p.DocumentLines)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Строки_до__Докум__5FB337D6");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Инвентар__3214EC27DA017416");

            entity.ToTable("Inventory");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.InventoryDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsCompleted).HasDefaultValue(false);
            entity.Property(e => e.ResponsiblePerson).HasMaxLength(100);
        });

        modelBuilder.Entity<InventoryResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Результа__3214EC27083AC792");

            entity.Property(e => e.Id).HasColumnName("ID");

            entity.HasOne(d => d.Inventory).WithMany(p => p.InventoryResults)
                .HasForeignKey(d => d.InventoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Результат__Инвен__6754599E");

            entity.HasOne(d => d.Product).WithMany(p => p.InventoryResults)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Результат__Товар__68487DD7");
        });

        modelBuilder.Entity<MovementJournal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Журнал_д__3214EC270E454362");

            entity.ToTable("MovementJournal");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.OperationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OperationType).HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(100);

            entity.HasOne(d => d.Batch).WithMany(p => p.MovementJournals)
                .HasForeignKey(d => d.BatchId)
                .HasConstraintName("FK__Журнал_дв__Парти__778AC167");

            entity.HasOne(d => d.Document).WithMany(p => p.MovementJournals)
                .HasForeignKey(d => d.DocumentId)
                .HasConstraintName("FK__Журнал_дв__Докум__76969D2E");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Уведомле__3214EC27FA9591D9");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.Batch).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.BatchId)
                .HasConstraintName("FK__Уведомлен__Парти__00200768");

            entity.HasOne(d => d.Product).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__Уведомлен__Товар__7F2BE32F");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Товары__3214EC2788EA4D93");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Manufacturer).HasMaxLength(100);
            entity.Property(e => e.MinRemainder).HasDefaultValue(5);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.ReleaseForm).HasMaxLength(50);
            entity.Property(e => e.RequiresPrescription).HasDefaultValue(false);
            entity.Property(e => e.UnitOfMeasure)
                .HasMaxLength(20)
                .HasDefaultValue("шт.");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Товары__Категори__5165187F");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Поставщи__3214EC2715F63432");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.ContactPerson).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<WriteOff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Списания__3214EC27C5A2EC8D");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.Reason).HasMaxLength(200);

            entity.HasOne(d => d.Document).WithMany(p => p.WriteOffs)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Списания__Докуме__7A672E12");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
