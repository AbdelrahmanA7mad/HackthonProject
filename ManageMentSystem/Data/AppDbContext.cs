using ManageMentSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ManageMentSystem.Data
{
    public class AppDbContext : IdentityDbContext <ApplicationUser>
	{
		private readonly IHttpContextAccessor? _httpContextAccessor;

        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor? httpContextAccessor = null) : base(options)
		{
            _httpContextAccessor = httpContextAccessor;
		}
		public DbSet<Product> Products { get; set; }
		public DbSet<Customer> Customers { get; set; }
		public DbSet<Sale> Sales { get; set; }
		public DbSet<SaleItem> SaleItems { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<StoreAccount> StoreAccounts { get; set; }
		public DbSet<GeneralDebt> GeneralDebts { get; set; }
		// Identity provides Users via Set<ApplicationUser>()
		public DbSet<PaymentMethodOption> PaymentMethodOptions { get; set; }
		public DbSet<CustomerPayment> CustomerPayments { get; set; }
		public DbSet<PaymentAllocation> PaymentAllocations { get; set; }

        public DbSet<Installment> Installments { get; set; }
        public DbSet<InstallmentItem> InstallmentItems { get; set; }
        public DbSet<InstallmentPayment> InstallmentPayments { get; set; }
        public DbSet<Invoice> Invoices { get; set; }

    // New SaaS Entities
    // Removed Employee, Role, Permissions custom entities

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<InventorySettings> InventorySettings { get; set; }
    public DbSet<TabVisibilitySettings> TabVisibilitySettings { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

            // Inventory Settings -> Tenant
            modelBuilder.Entity<InventorySettings>()
                .HasOne(s => s.Tenant)
                .WithMany() // One-to-Many but we might not need the collection on Tenant
                .HasForeignKey(s => s.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tab Visibility Settings -> Tenant
            modelBuilder.Entity<TabVisibilitySettings>()
                .HasOne(s => s.Tenant)
                .WithMany()
                .HasForeignKey(s => s.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            

            // Tenant Configuration
            modelBuilder.Entity<Tenant>()
                .HasIndex(t => t.Name)
                .IsUnique();
                
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Restrict);


            // ... Existing configurations ...
            // We need to keep existing business logic but eventually migrate OwnerId FKs to TenantId FKs
            // For now, let's just make sure the basic Identity setup works.
			
			
			
			// Configure Category-Product relationship
			modelBuilder.Entity<Product>()
				.HasOne(p => p.Category)
				.WithMany(c => c.Products)
				.HasForeignKey(p => p.CategoryId)
				.OnDelete(DeleteBehavior.SetNull);

			// Configure Product-Barcode unique constraint
			modelBuilder.Entity<Product>()
				.HasIndex(p => p.Barcode)
				.IsUnique()
				.HasFilter("\"Barcode\" IS NOT NULL");

			// Configure Customer-Sales relationship
			modelBuilder.Entity<Customer>()
				.HasMany(c => c.Sales)
				.WithOne(s => s.Customer)
				.HasForeignKey(s => s.CustomerId)
				.OnDelete(DeleteBehavior.Restrict);

			// Configure Sale-SaleItem relationship
			modelBuilder.Entity<Sale>()
				.HasMany(s => s.SaleItems)
				.WithOne(si => si.Sale)
				.HasForeignKey(si => si.SaleId)
				.OnDelete(DeleteBehavior.Cascade);

			// Configure Product-SaleItem relationship (keep operations when product is deleted)
			modelBuilder.Entity<SaleItem>()
				.HasOne(si => si.Product)
				.WithMany()
				.HasForeignKey(si => si.ProductId)
				.OnDelete(DeleteBehavior.SetNull);
			// Configure StoreAccount-GeneralDebt relationship
			modelBuilder.Entity<StoreAccount>()
				.HasOne<GeneralDebt>()
				.WithMany(gd => gd.StoreAccounts)
				.HasForeignKey("GeneralDebtId")
				.OnDelete(DeleteBehavior.Cascade);

			// Configure CustomerPayment relationships
			modelBuilder.Entity<CustomerPayment>()
				.HasOne(cp => cp.Customer)
				.WithMany(c => c.Payments)
				.HasForeignKey(cp => cp.CustomerId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<PaymentAllocation>()
				.HasOne(pa => pa.CustomerPayment)
				.WithMany(cp => cp.Allocations)
				.HasForeignKey(pa => pa.CustomerPaymentId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<PaymentAllocation>()
				.HasOne(pa => pa.Sale)
				.WithMany()
				.HasForeignKey(pa => pa.SaleId)
				.OnDelete(DeleteBehavior.Cascade);

		

            // Configure Installment-InstallmentItem relationship
            modelBuilder.Entity<InstallmentItem>()
                .HasOne(ii => ii.Installment)
                .WithMany(i => i.InstallmentItems)
                .HasForeignKey(ii => ii.InstallmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Product-InstallmentItem relationship (keep operations when product is deleted)
            modelBuilder.Entity<InstallmentItem>()
                .HasOne(ii => ii.Product)
                .WithMany()
                .HasForeignKey(ii => ii.ProductId)
                .OnDelete(DeleteBehavior.SetNull);



            // Invoice -> User relationship (one-to-one)
            modelBuilder.Entity<Invoice>()
				.HasOne(i => i.User)
				.WithMany()
				.HasForeignKey(i => i.UserId)
				.OnDelete(DeleteBehavior.Cascade);



            // PurchaseInvoice -> User (CreatedByUser) - already exists but keeping for consistency

            // ==============================================
            // MULTI-TENANCY: TENANT RELATIONSHIPS
            // ==============================================
            // Explicit Tenant relationships for all multi-tenant entities

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Tenant)
                .WithMany()
                .HasForeignKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Customer>()
                .HasOne(c => c.Tenant)
                .WithMany()
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Tenant)
                .WithMany()
                .HasForeignKey(s => s.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.Tenant)
                .WithMany()
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

       

            modelBuilder.Entity<Installment>()
                .HasOne(i => i.Tenant)
                .WithMany()
                .HasForeignKey(i => i.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

           
            // ==============================================
            // MULTI-TENANCY: PERFORMANCE INDEXES
            // ==============================================
            // Composite indexes on TenantId + frequently queried fields

            // Product indexes
            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.TenantId, p.Name })
                .HasDatabaseName("IX_Products_TenantId_Name");

            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.TenantId, p.CategoryId })
                .HasDatabaseName("IX_Products_TenantId_CategoryId");

            // Customer indexes
            modelBuilder.Entity<Customer>()
                .HasIndex(c => new { c.TenantId, c.PhoneNumber })
                .HasDatabaseName("IX_Customers_TenantId_PhoneNumber");

            modelBuilder.Entity<Customer>()
                .HasIndex(c => new { c.TenantId, c.FullName })
                .HasDatabaseName("IX_Customers_TenantId_FullName");

            // Sale indexes
            modelBuilder.Entity<Sale>()
                .HasIndex(s => new { s.TenantId, s.SaleDate })
                .HasDatabaseName("IX_Sales_TenantId_SaleDate");

            modelBuilder.Entity<Sale>()
                .HasIndex(s => new { s.TenantId, s.CustomerId })
                .HasDatabaseName("IX_Sales_TenantId_CustomerId");

           

            // Installment indexes
            modelBuilder.Entity<Installment>()
                .HasIndex(i => new { i.TenantId, i.Status })
                .HasDatabaseName("IX_Installments_TenantId_Status");

            modelBuilder.Entity<Installment>()
                .HasIndex(i => new { i.TenantId, i.CustomerId })
                .HasDatabaseName("IX_Installments_TenantId_CustomerId");

            // Category indexes
            modelBuilder.Entity<Category>()
                .HasIndex(c => new { c.TenantId, c.Name })
                .HasDatabaseName("IX_Categories_TenantId_Name");

           

            // ==============================================
            // MULTI-TENANCY: GLOBAL QUERY FILTERS
            // ==============================================
            // Automatic tenant isolation - all queries filtered by TenantId
            // This ensures data isolation at the EF Core level

            if (_httpContextAccessor != null)
            {
                // Product
                modelBuilder.Entity<Product>().HasQueryFilter(p => 
                    _httpContextAccessor.HttpContext == null || 
                    _httpContextAccessor.HttpContext.User == null ||
                    p.TenantId == _httpContextAccessor.HttpContext.User.FindFirstValue("TenantId"));

                // Customer
                modelBuilder.Entity<Customer>().HasQueryFilter(c => 
                    _httpContextAccessor.HttpContext == null || 
                    _httpContextAccessor.HttpContext.User == null ||
                    c.TenantId == _httpContextAccessor.HttpContext.User.FindFirstValue("TenantId"));

                // Sale
                modelBuilder.Entity<Sale>().HasQueryFilter(s => 
                    _httpContextAccessor.HttpContext == null || 
                    _httpContextAccessor.HttpContext.User == null ||
                    s.TenantId == _httpContextAccessor.HttpContext.User.FindFirstValue("TenantId"));

                // Category
                modelBuilder.Entity<Category>().HasQueryFilter(c => 
                    _httpContextAccessor.HttpContext == null || 
                    _httpContextAccessor.HttpContext.User == null ||
                    c.TenantId == _httpContextAccessor.HttpContext.User.FindFirstValue("TenantId"));

        
                // Installment
                modelBuilder.Entity<Installment>().HasQueryFilter(i => 
                    _httpContextAccessor.HttpContext == null || 
                    _httpContextAccessor.HttpContext.User == null ||
                    i.TenantId == _httpContextAccessor.HttpContext.User.FindFirstValue("TenantId"));

       

                // InventorySettings
                modelBuilder.Entity<InventorySettings>().HasQueryFilter(s => 
                    _httpContextAccessor.HttpContext == null || 
                    _httpContextAccessor.HttpContext.User == null ||
                    s.TenantId == _httpContextAccessor.HttpContext.User.FindFirstValue("TenantId"));

                // TabVisibilitySettings
                modelBuilder.Entity<TabVisibilitySettings>().HasQueryFilter(s => 
                    _httpContextAccessor.HttpContext == null || 
                    _httpContextAccessor.HttpContext.User == null ||
                    s.TenantId == _httpContextAccessor.HttpContext.User.FindFirstValue("TenantId"));

                // StoreAccount
                modelBuilder.Entity<StoreAccount>().HasQueryFilter(sa => 
                    _httpContextAccessor.HttpContext == null || 
                    _httpContextAccessor.HttpContext.User == null ||
                    sa.TenantId == _httpContextAccessor.HttpContext.User.FindFirstValue("TenantId"));

                // GeneralDebt
                modelBuilder.Entity<GeneralDebt>().HasQueryFilter(gd => 
                    _httpContextAccessor.HttpContext == null || 
                    _httpContextAccessor.HttpContext.User == null ||
                    gd.TenantId == _httpContextAccessor.HttpContext.User.FindFirstValue("TenantId"));

                // CustomerPayment
                modelBuilder.Entity<CustomerPayment>().HasQueryFilter(cp => 
                    _httpContextAccessor.HttpContext == null || 
                    _httpContextAccessor.HttpContext.User == null ||
                    cp.TenantId == _httpContextAccessor.HttpContext.User.FindFirstValue("TenantId"));

        
                // PaymentMethodOption
                modelBuilder.Entity<PaymentMethodOption>().HasQueryFilter(pmo => 
                    _httpContextAccessor.HttpContext == null || 
                    _httpContextAccessor.HttpContext.User == null ||
                    pmo.TenantId == _httpContextAccessor.HttpContext.User.FindFirstValue("TenantId") ||
                    pmo.TenantId == null);

        

                // Invoice
                modelBuilder.Entity<Invoice>().HasQueryFilter(inv => 
                    _httpContextAccessor.HttpContext == null || 
                    _httpContextAccessor.HttpContext.User == null ||
                    inv.TenantId == _httpContextAccessor.HttpContext.User.FindFirstValue("TenantId"));
            }

            // Invoice table mapping (keep old table name for backward compatibility)
            modelBuilder.Entity<Invoice>().ToTable("Inovice");

            // Invoice -> Tenant relationship
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Tenant)
                .WithMany()
                .HasForeignKey(i => i.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // NOTE: Global Query Filters are now ENABLED
            // This provides automatic tenant isolation at the database query level
            // To bypass filters (e.g., for admin operations), use .IgnoreQueryFilters()

		}



	}
}
