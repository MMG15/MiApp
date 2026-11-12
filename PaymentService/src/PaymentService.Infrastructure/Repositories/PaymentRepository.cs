using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    public PaymentRepository(PaymentDbContext context) => _context = context;

    public async Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        => await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrderId == orderId, ct);

    public async Task<IReadOnlyList<Payment>> GetAllAsync(CancellationToken ct = default)
        => await _context.Payments
            .AsNoTracking()
            .OrderByDescending(p => p.ProcessedAt)
            .ToListAsync(ct);

    public async Task AddAsync(Payment payment, CancellationToken ct = default)
        => await _context.Payments.AddAsync(payment, ct);
}
