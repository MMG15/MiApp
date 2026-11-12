using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiApp.Domain.Entities;

namespace MiApp.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();
        builder.Property(o => o.UserId).IsRequired();
        builder.Property(o => o.Status).IsRequired();
        builder.Property(o => o.CreatedAt).IsRequired();

        // Datos que agrega el pago (vienen de PaymentService)
        builder.Property(o => o.TransactionId).HasMaxLength(50);
        builder.Property(o => o.PaymentProcessedAt);

        // Total es calculado (Money): no es una columna
        builder.Ignore(o => o.Total);

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // EF lee y escribe la lista privada _items (Items es de solo lectura)
        builder.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(o => o.UserId);
    }
}
