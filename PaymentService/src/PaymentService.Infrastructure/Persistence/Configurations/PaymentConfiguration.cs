using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.OrderId).IsRequired();

        // Una orden se paga una sola vez: refuerza la idempotencia a nivel base de datos
        builder.HasIndex(p => p.OrderId).IsUnique();

        // Value Object Money como "complex type" (EF Core 8): se guarda en dos columnas
        // de la misma tabla Payments (Amount y Currency), sin tabla ni Id propios.
        builder.ComplexProperty(p => p.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Amount").HasColumnType("decimal(18,2)").IsRequired();
            money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3).IsRequired();
        });

        builder.Property(p => p.Status).IsRequired();
        builder.Property(p => p.TransactionId).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Reason).HasMaxLength(300);
        builder.Property(p => p.ProcessedAt).IsRequired();

        builder.Ignore(p => p.IsApproved);
    }
}
