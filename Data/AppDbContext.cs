using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Model;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Partner> Partners { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Itinerary> Itineraries { get; set; }
    public DbSet<ItineraryBooking> ItineraryBookings { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<ComplianceReport> ComplianceReports { get; set; }
    public DbSet<KPIReport> KPIReports { get; set; }
    public DbSet<RetentionPolicy> RetentionPolicies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<AuditLog>()
        .HasOne(a => a.User)
        .WithMany(u => u.AuditLogs)
        .HasForeignKey(a => a.UserId);

    modelBuilder.Entity<Booking>()
        .HasOne(b => b.User)
        .WithMany(u => u.Bookings)
        .HasForeignKey(b => b.UserId)
        .OnDelete(DeleteBehavior.Restrict); 

    modelBuilder.Entity<Booking>()
        .HasOne(b => b.Partner)
        .WithMany(p => p.Bookings)
        .HasForeignKey(b => b.PartnerId)
        .OnDelete(DeleteBehavior.Restrict); 

    modelBuilder.Entity<Booking>()
        .HasOne(b => b.Inventory)
        .WithMany(i => i.Bookings)
        .HasForeignKey(b => b.InventoryId)
        .OnDelete(DeleteBehavior.Restrict); 

    modelBuilder.Entity<Inventory>()
        .HasOne(i => i.Partner)
        .WithMany(p => p.Inventories)
        .HasForeignKey(i => i.PartnerId);

    modelBuilder.Entity<Reservation>()
        .HasOne(r => r.Booking)
        .WithMany(b => b.Reservations)
        .HasForeignKey(r => r.BookingId);

    modelBuilder.Entity<Itinerary>()
        .HasOne(i => i.User)
        .WithMany()
        .HasForeignKey(i => i.UserId);

    modelBuilder.Entity<ItineraryBooking>()
        .HasOne(ib => ib.Itinerary)
        .WithMany(i => i.ItineraryBookings)
        .HasForeignKey(ib => ib.ItineraryId)
        .OnDelete(DeleteBehavior.Restrict); 

    modelBuilder.Entity<ItineraryBooking>()
        .HasOne(ib => ib.Booking)
        .WithMany(b => b.ItineraryBookings)
        .HasForeignKey(ib => ib.BookingId);

    modelBuilder.Entity<Invoice>()
        .HasOne(inv => inv.Booking)
        .WithMany(b => b.Invoices)
        .HasForeignKey(inv => inv.BookingId);

    modelBuilder.Entity<Payment>()
        .HasOne(p => p.Invoice)
        .WithMany(inv => inv.Payments)
        .HasForeignKey(p => p.InvoiceId);
    
    modelBuilder.Entity<AuditLog>()
        .HasOne(a => a.User)
        .WithMany()
        .HasForeignKey(a => a.UserId)
        .OnDelete(DeleteBehavior.NoAction);
}

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }
}