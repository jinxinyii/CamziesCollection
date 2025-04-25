using CapstoneCC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using static CapstoneCC.Models.OrderModel;

namespace CapstoneCC.Services
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<SalesTransaction> SalesTransactions { get; set; }
        public DbSet<Analytics> Analytics { get; set; }
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<OrderModel.Order> Orders { get; set; }
        public DbSet<OrderModel.OrderProduct> OrderProducts { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var admin = new IdentityRole("admin");
            admin.NormalizedName = "admin";

            var staff = new IdentityRole("staff");
            staff.NormalizedName = "staff";

            var reseller = new IdentityRole("reseller");
            reseller.NormalizedName = "reseller";

            var customer = new IdentityRole("customer");
            customer.NormalizedName = "customer";

            builder.Entity<OrderModel.OrderProduct>()
                .HasKey(op => op.OrderProductId);

            builder.Entity<OrderModel.OrderProduct>()
                .HasOne(op => op.Order)
                .WithMany(o => o.OrderProducts)
                .HasForeignKey(op => op.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}