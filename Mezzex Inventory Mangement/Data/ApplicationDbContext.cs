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
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<BlockedChannel> BlockedChannels { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }

        // Constructor
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite Key for IdentityUserRole
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

            // Ensure PhoneNumber is unique for ApplicationUser
            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            // Category Hierarchy (Self-referencing Relationship)
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascading delete

            modelBuilder.Entity<Category>()
                .Property(c => c.CategoryType)
                .HasConversion<string>(); // Store the enum as a string in the database

            modelBuilder.Entity<Brand>()
               .HasMany(b => b.Categories)
               .WithMany(c => c.Brands)
               .UsingEntity(j => j.ToTable("BrandCategories"));

           modelBuilder.Entity<Supplier>()
                .HasMany(s => s.Companies)
                .WithMany(c => c.Suppliers)
                .UsingEntity(j => j.ToTable("SupplierCompanies"));
        }
    }
}
