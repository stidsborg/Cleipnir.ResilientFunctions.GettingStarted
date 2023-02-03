using Sample.WebApi.Ordering;

namespace Sample.WebApi.Tests.Stubs;

public class PaymentProviderClientStub : IPaymentProviderClient
{
    public List<Tuple<Guid, Guid, decimal>> ReserveInvocations { get; } = new();
    public List<Guid> CaptureInvocations { get; } = new();
    public List<Guid> CancelReservationInvocations { get; } = new();
    
    public Task Reserve(Guid transactionId, Guid customerId, decimal amount)
    {
        ReserveInvocations.Add(Tuple.Create(transactionId, customerId, amount));
        return Task.CompletedTask;
    }

    public Task Capture(Guid transactionId)
    {
        CaptureInvocations.Add(transactionId);
        return Task.CompletedTask;
    }

    public Task CancelReservation(Guid transactionId)
    {
        CancelReservationInvocations.Add(transactionId);
        return Task.CompletedTask;
    }
}