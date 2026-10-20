using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
        
namespace Infrastructure.Persistence.Configurations;

public class TradeConfiguration : IEntityTypeConfiguration<Trade>
{
    public void Configure(EntityTypeBuilder<Trade> builder)
    {
        builder.ToTable("Trades");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Instrument)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(t => t.Direction)
            .IsRequired();

        builder.Property(t => t.EntryPrice)
            .HasPrecision(18, 4);

        builder.Property(t => t.ExitPrice)
            .HasPrecision(18, 4);

        builder.Property(t => t.PointValue)
            .HasPrecision(18, 4);

        builder.Property(t => t.CommissionPerUnit)
            .HasPrecision(18, 4);

        builder.Property(t => t.Commission)
            .HasPrecision(18, 4);

        builder.Property(t => t.ProfitLoss)
            .HasPrecision(18, 4);

        builder.Property(t => t.Quantity)
            .IsRequired();

        builder.Property(t => t.EntryTime)
            .IsRequired();

        builder.Property(t => t.ExitTime)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .IsRequired();
        
        builder.HasOne(t => t.User)
            .WithMany(u => u.Trades)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // A trade must always belong to a market. Deleting a market that still
        // has trades is refused (surfaced as 409 by the service) rather than
        // silently discarding trade history.
        builder.HasOne(t => t.Market)
            .WithMany(m => m.Trades)
            .HasForeignKey(t => t.MarketId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}