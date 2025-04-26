using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using OnlineShop.Models;

namespace OnlineShop.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<CategoryProduct> CategoryProducts { get; set; }
        public DbSet<CategorySize> CategorySizes { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<TicketReply> TicketReplies { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<CartItemEntity> CartItems { get; set; }

        public DbSet<Slider> Sliders { get; set; }

        public virtual DbSet<AdCategory> AdCategories { get; set; }

        public virtual DbSet<AdClickLog> AdClickLogs { get; set; }
        public virtual DbSet<AdTemplatePosition> AdTemplatePositions { get; set; } // Thêm bảng AdTemplatePosition vào>

        public virtual DbSet<AdPlacement> AdPlacements { get; set; }

        public virtual DbSet<AdPosition> AdPositions { get; set; }

        public virtual DbSet<AdTemplate> AdTemplates { get; set; }

        public virtual DbSet<Advertisement> Advertisements { get; set; }

        public virtual DbSet<Blog> Blogs { get; set; }

        public virtual DbSet<BlogCategory> BlogCategories { get; set; }

        public virtual DbSet<Comment> Comments { get; set; }

        public virtual DbSet<Tag> Tags { get; set; }

        public virtual DbSet<LikeOfBlog> LikeOfBlogs { get; set; }
        public virtual DbSet<TagBlog> TagBlogs { get; set; }

        public virtual DbSet<Thumbnail> Thumbnails { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Product Configuration
            builder.Entity<Product>()
                .Property(p => p.ProductPrice)
                .HasPrecision(18, 2); // Precision for decimal values

            builder.Entity<Product>()
                .HasOne(p => p.CategoryProduct)
                .WithMany(cp => cp.Products)
                .HasForeignKey(p => p.CategoryProductId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            builder.Entity<ProductSize>()
                .HasKey(ps => new { ps.ProductID, ps.CategorySizeID });


            builder.Entity<ProductSize>()
                .HasOne(ps => ps.Product)
                .WithMany(p => p.ProductSizes)
                .HasForeignKey(ps => ps.ProductID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProductSize>()
             .HasOne(ps => ps.CategorySize)
             .WithMany(cs => cs.ProductSizes)
             .HasForeignKey(ps => ps.CategorySizeID)
             .OnDelete(DeleteBehavior.Cascade);

            // CategoryProduct Configuration
            builder.Entity<CategoryProduct>()
                .HasIndex(cp => cp.CategoryProductName)
                .IsUnique(); // Ensure unique category names

            // ProductSize Configuration
            builder.Entity<CategorySize>()
                 .HasIndex(cs => cs.CategorySizeName)
                 .IsUnique(); // Ensure unique size names


            builder.Entity<UserAddress>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ensure only one default address per user
            builder.Entity<UserAddress>()
                .HasIndex(ua => new { ua.UserId, ua.IsDefault })
                .HasFilter("[IsDefault] = 1")
                .IsUnique();

            // CartItemEntity Configuration
            builder.Entity<CartItemEntity>()
                .HasKey(ci => ci.Id);

            builder.Entity<CartItemEntity>()
                .Property(ci => ci.Id)
                .ValueGeneratedOnAdd(); // Tự động tăng

            builder.Entity<CartItemEntity>()
                .HasOne(ci => ci.User)
                .WithMany()
                .HasForeignKey(ci => ci.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CartItemEntity>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CartItemEntity>()
                .HasOne(ci => ci.CategorySize)
                .WithMany()
                .HasForeignKey(ci => ci.CategorySizeId)
                .OnDelete(DeleteBehavior.SetNull); // Nếu xóa CategorySize, đặt CategorySizeId thành null

            // Cấu hình cho Slider
            builder.Entity<Slider>()
                .Property(s => s.Id)
                .ValueGeneratedOnAdd();

            builder.Entity<Order>()
                .HasMany(o => o.OrderDetails)
                .WithOne(od => od.Order)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId);
            // Ticket Reply Configuration
            builder.Entity<TicketReply>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TicketReply>()
                .Property(r => r.Message)
                .IsRequired();

            // UserAddress Configuration
            builder.Entity<UserAddress>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ensure only one default address per user
            builder.Entity<UserAddress>()
                .HasIndex(ua => new { ua.UserId, ua.IsDefault })
                .HasFilter("[IsDefault] = 1")
                .IsUnique();
            builder.Entity<AdCategory>(entity =>
            {
                entity.ToTable("AdCategory");

                entity.HasIndex(e => e.AdId, "IX_AdCategory_AdId");

                entity.HasIndex(e => e.CategoryId, "IX_AdCategory_CategoryId");

                entity.HasOne(d => d.Ad).WithMany(p => p.AdCategories)
                    .HasForeignKey(d => d.AdId)
                    .HasConstraintName("FK_AdCategory_Advertisement");
                entity.HasOne(d => d.Category).WithMany(p => p.AdCategories)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_AdCategory_CategoryProduct");
            });

            builder.Entity<AdClickLog>(entity =>
            {
                entity.ToTable("AdCLickLog");

                entity.HasIndex(e => e.AdId, "IX_AdCLickLog_AdId");

                entity.Property(e => e.ClickedAt).HasColumnType("date");
                entity.Property(e => e.CreatedAt).HasColumnType("date");
                entity.Property(e => e.IpAddress).HasMaxLength(100);
                entity.Property(e => e.UserDevice).HasMaxLength(1000);

                entity.HasOne(d => d.Ad).WithMany(p => p.AdClickLogs)
                    .HasForeignKey(d => d.AdId)
                    .HasConstraintName("FK_AdCLickLog_Advertisement");
            });

            builder.Entity<AdPlacement>(entity =>
            {
                entity.ToTable("AdPlacement");

                entity.HasIndex(e => e.AdId, "IX_AdPlacement_AdId");

                entity.HasIndex(e => e.BlogId, "IX_AdPlacement_BlogId");

                entity.HasIndex(e => e.PositionId, "IX_AdPlacement_PositionId");

                entity.Property(e => e.CreatedAt).HasColumnType("date");

                entity.HasOne(d => d.Ad).WithMany(p => p.AdPlacements)
                    .HasForeignKey(d => d.AdId)
                    .HasConstraintName("FK_AdPlacement_Advertisement");

                entity.HasOne(d => d.Blog).WithMany(p => p.AdPlacements)
                    .HasForeignKey(d => d.BlogId)
                    .HasConstraintName("FK_AdPlacement_Blog");

                entity.HasOne(d => d.Position).WithMany(p => p.AdPlacements)
                    .HasForeignKey(d => d.PositionId)
                    .HasConstraintName("FK_AdPlacement_AdPosition");
            });

            builder.Entity<AdPosition>(entity =>
            {
                entity.ToTable("AdPosition");

                entity.Property(e => e.Position).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Name).HasMaxLength(200);
            });

            builder.Entity<AdTemplate>(entity =>
            {
                entity.ToTable("AdTemplate");

                entity.Property(e => e.CreatedAt).HasColumnType("date");
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.PreviewImageUrl).HasMaxLength(500);
            });

            builder.Entity<AdTemplatePosition>(entity =>
            {
                entity.ToTable("AdTemplatePosition");

                entity.HasOne(d => d.AdTemplate)
                .WithMany(p => p.AdTemplatePositions)
                .HasForeignKey(d => d.AdTemplateId)
                .OnDelete(DeleteBehavior.Cascade)
                ;

                entity.HasOne(d => d.AdPosition)
                .WithMany(p => p.AdTemplatePositions)
                .HasForeignKey(d => d.PositionId)
                .OnDelete(DeleteBehavior.Cascade)
                ;
            });

            builder.Entity<Advertisement>(entity =>
            {
                entity.ToTable("Advertisement");

                entity.HasIndex(e => e.AdTemplateId, "IX_Advertisement_AdTemplateId");

                entity.Property(e => e.CreatedAt).HasColumnType("date");
                entity.Property(e => e.EndDate).HasColumnType("date");
                entity.Property(e => e.ImageUrl)
                    .HasMaxLength(500)
                    .HasColumnName("ImageURL");
                entity.Property(e => e.LinkUrl).HasMaxLength(500);
                entity.Property(e => e.StartDate).HasColumnType("date");
                entity.Property(e => e.Title).HasMaxLength(255);
                entity.Property(e => e.UpdatedAt).HasColumnType("date");

                entity.HasOne(d => d.AdTemplate).WithMany(p => p.Advertisements)
                    .HasForeignKey(d => d.AdTemplateId)
                    .HasConstraintName("FK_Advertisement_AdTemplate");
            });

            builder.Entity<Blog>(entity =>
            {
                entity.ToTable("Blog");

                entity.HasIndex(e => e.CategoryId, "IX_Blog_CategoryId");

                entity.Property(e => e.AuthorId).HasMaxLength(450);
                entity.Property(e => e.LastUpdated).HasColumnType("date");
                entity.Property(e => e.PublishedDate).HasColumnType("date");
                entity.Property(e => e.Summary).HasMaxLength(500);

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Blogs)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK_Blog_BlogCategory");

                entity.HasOne(d => d.Thumbnail)
                    .WithMany(p => p.Blogs)
                    .HasForeignKey(d => d.ThumbnailId);

                // Thiết lập quan hệ 1-1 với ApplicationUser
                entity.HasOne(d => d.User)
                    .WithMany(b => b.Blogs)
                    .HasForeignKey(d => d.AuthorId) // AuthorId là khóa ngoại trỏ tới IdentityUser.Id
                    .HasConstraintName("FK_Blog_ApplicationUser");

                entity.HasMany(d => d.LikeOfBlogs)
                   .WithOne(b => b.Blog)
                   .HasForeignKey(d => d.BlogId) // AuthorId là khóa ngoại trỏ tới IdentityUser.Id
                   .HasConstraintName("FK_Blog_LikeOfBlog");
            });

            builder.Entity<LikeOfBlog>(entity =>
            {
                entity.ToTable("LikeOfBlog");

                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.HasOne(d => d.User)
                    .WithMany(b => b.LikeOfBlogs)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_LikeOfBlog_ApplicationUser")
                    ;
            });
            builder.Entity<BlogCategory>(entity =>
            {
                entity.ToTable("BlogCategory");

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            builder.Entity<Comment>(entity =>
            {
                entity.ToTable("Comment");

                entity.Property(e => e.CreatedAt).HasColumnType("date");
                entity.Property(e => e.TargetType).HasMaxLength(100);
                entity.Property(e => e.UpdatedAt).HasColumnType("date");
                entity.Property(e => e.UserId).HasMaxLength(450);
            });

            builder.Entity<Tag>(entity =>
            {
                entity.ToTable("Tag");

                entity.Property(e => e.CreateAt).HasColumnType("date");
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Slug).HasMaxLength(120);
            });

            builder.Entity<TagBlog>(entity =>
            {
                entity.ToTable("TagBlog");

                entity.HasIndex(e => e.BlogId, "IX_TagBlog_BlogId");

                entity.HasIndex(e => e.TagId, "IX_TagBlog_TagId");

                entity.Property(e => e.CreatedAt).HasColumnType("date");

                entity.HasOne(d => d.Blog).WithMany(p => p.TagBlogs)
                    .HasForeignKey(d => d.BlogId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TagBlog_Blog");

                entity.HasOne(d => d.Tag).WithMany(p => p.TagBlogs)
                    .HasForeignKey(d => d.TagId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TagBlog_Tag");
            });

            builder.Entity<Thumbnail>(entity =>
            {
                entity.ToTable("Thumbnail");

                entity.Property(e => e.ThumbnailName).HasDefaultValueSql("(N'')");
                entity.Property(e => e.ThumnailUrl).HasMaxLength(500);
            });
        }
    }
}