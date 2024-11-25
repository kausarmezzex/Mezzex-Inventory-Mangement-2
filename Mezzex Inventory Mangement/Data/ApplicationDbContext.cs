using Mezzex_Inventory_Mangement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mezzex_Inventory_Mangement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<SellingChannel> SellingChannel { get; set; }
        public DbSet<ManageCompany> ManageCompany { get; set; }
        public DbSet<UserCompanyAssignment> UserCompanyAssignments { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<PageRoleMapping> PageRoleMappings { get; set; }
        // Constructor
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUserRole<int>>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });


            // PageRoleMapping Many-to-Many Configuration
            modelBuilder.Entity<PageRoleMapping>()
                .HasOne(prm => prm.Page)
                .WithMany(p => p.PageRoleMappings)
                .HasForeignKey(prm => prm.PageId);

            modelBuilder.Entity<PageRoleMapping>()
                .HasOne(prm => prm.Role)
                .WithMany()
                .HasForeignKey(prm => prm.RoleId);
            modelBuilder.Entity<ApplicationUser>()
           .HasIndex(u => u.PhoneNumber)
           .IsUnique();
        }
    }
}
