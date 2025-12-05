using EventSpark.Core.Entities;
using EventSpark.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventSpark.Infrastructure.Data
{
    public class EventSparkDbContext : DbContext
    {
        public EventSparkDbContext(DbContextOptions<EventSparkDbContext> options)
            : base(options)
        {
        }

        public DbSet<EventCategory> EventCategories { get; set; } = null!;
        public DbSet<Event> Events { get; set; } = null!;
        public DbSet<TicketType> TicketTypes { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<Ticket> Tickets { get; set; } = null!;
        public DbSet<CheckInLog> CheckInLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================
            // EventCategory
            // ==========================
            modelBuilder.Entity<EventCategory>(entity =>
            {
                entity.ToTable("EventCategories");

                entity.HasKey(e => e.EventCategoryId);

                entity.Property(e => e.Name)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.Slug)
                      .HasMaxLength(100);
            });

            // ==========================
            // Event
            // ==========================
            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable("Events");

                entity.HasKey(e => e.EventId);

                entity.Property(e => e.OrganizerId)
                      .HasMaxLength(450)
                      .IsRequired();

                entity.Property(e => e.Title)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(e => e.Description)
                      .IsRequired();

                entity.Property(e => e.VenueName)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(e => e.VenueAddress)
                      .HasMaxLength(400);

                entity.Property(e => e.City)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.ImagePath)
                      .HasMaxLength(400);

                // Enum → tinyint
                entity.Property(e => e.Status)
                      .HasConversion<byte>();

                // RowVersion
                entity.Property(e => e.RowVersion)
                      .IsRowVersion();

                // Relationships
                entity.HasOne(e => e.Category)
                      .WithMany(c => c.Events)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================
            // TicketType
            // ==========================
            modelBuilder.Entity<TicketType>(entity =>
            {
                entity.ToTable("TicketTypes");

                entity.HasKey(t => t.TicketTypeId);

                entity.Property(t => t.Name)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(t => t.Description)
                      .HasMaxLength(400);

                entity.Property(t => t.Price)
                      .HasColumnType("decimal(10,2)");

                entity.Property(t => t.RowVersion)
                      .IsRowVersion();

                entity.HasOne(t => t.Event)
                      .WithMany(e => e.TicketTypes)
                      .HasForeignKey(t => t.EventId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================
            // Order
            // ==========================
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");

                entity.HasKey(o => o.OrderId);

                entity.Property(o => o.BuyerId)
                      .HasMaxLength(450)
                      .IsRequired();

                entity.Property(o => o.Status)
                      .HasConversion<byte>();

                entity.Property(o => o.TotalAmount)
                      .HasColumnType("decimal(10,2)");

                entity.Property(o => o.PaymentReference)
                      .HasMaxLength(100);

                entity.Property(o => o.EmailSnapshot)
                      .HasMaxLength(256);

                entity.Property(o => o.FullNameSnapshot)
                      .HasMaxLength(200);
            });

            // ==========================
            // OrderItem
            // ==========================
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");

                entity.HasKey(oi => oi.OrderItemId);

                entity.Property(oi => oi.UnitPrice)
                      .HasColumnType("decimal(10,2)");

                entity.Property(oi => oi.TicketTypeNameSnapshot)
                      .HasMaxLength(100)
                      .IsRequired();

                // Computed column in SQL (LineTotal = Quantity * UnitPrice)
                entity.Property(oi => oi.LineTotal)
                      .HasColumnType("decimal(10,2)")
                      .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.Items)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.TicketType)
                      .WithMany(tt => tt.OrderItems)
                      .HasForeignKey(oi => oi.TicketTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================
            // Ticket
            // ==========================
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.ToTable("Tickets");

                entity.HasKey(t => t.TicketId);

                entity.Property(t => t.TicketNumber)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(t => t.QrCodeValue)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(t => t.Status)
                      .HasConversion<byte>();

                entity.Property(t => t.CheckedInByUserId)
                      .HasMaxLength(450);

                entity.Property(t => t.RowVersion)
                      .IsRowVersion();

                entity.HasOne(t => t.OrderItem)
                      .WithMany(oi => oi.Tickets)
                      .HasForeignKey(t => t.OrderItemId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.Event)
                      .WithMany(e => e.Tickets)
                      .HasForeignKey(t => t.EventId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================
            // CheckInLog
            // ==========================
            modelBuilder.Entity<CheckInLog>(entity =>
            {
                entity.ToTable("CheckInLogs");

                entity.HasKey(c => c.CheckInLogId);

                entity.Property(c => c.ScannedByUserId)
                      .HasMaxLength(450);

                entity.Property(c => c.Result)
                      .HasConversion<byte>();

                entity.Property(c => c.RawCode)
                      .HasMaxLength(200);

                entity.Property(c => c.Message)
                      .HasMaxLength(400);

                entity.HasOne(c => c.Ticket)
                      .WithMany(t => t.CheckInLogs)
                      .HasForeignKey(c => c.TicketId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(c => c.Event)
                      .WithMany() // no collection nav on Event side
                      .HasForeignKey(c => c.EventId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
