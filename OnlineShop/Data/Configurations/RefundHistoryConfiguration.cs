using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Models;

namespace OnlineShop.Data.Configurations
{
    public class RefundHistoryConfiguration : IEntityTypeConfiguration<RefundHistory>
    {
        public void Configure(EntityTypeBuilder<RefundHistory> builder)
        {
            builder.ToTable("RefundHistories");

            builder.HasKey(r => r.RefundId);

            builder.Property(r => r.OrderId)
                .IsRequired();

            builder.Property(r => r.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(r => r.RefundAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(r => r.RefundDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(r => r.RefundStatus)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            builder.Property(r => r.Reason)
                .HasMaxLength(500);

            // Configure relationships
            builder.HasOne(r => r.Order)
                .WithMany()
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}