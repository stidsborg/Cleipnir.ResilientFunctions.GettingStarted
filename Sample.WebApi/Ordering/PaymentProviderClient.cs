using Serilog;

namespace Sample.WebApi.Ordering;

public interface IPaymentProviderClient
{
    Task Reserve(Guid transactionId, Guid customerId, decimal amount);
    Task Capture(Guid transactionId);
    Task CancelReservation(Guid transactionId);
}

public class PaymentProviderClientStub : IPaymentProviderClient
{
    public Task Reserve(Guid transactionId, Guid customerId, decimal amount)
        => Task
            .Delay(100)
            .ContinueWith(_ => Log.Logger.ForContext<IPaymentProviderClient>().Information($"PAYMENT_PROVIDER: Reserved '{amount}'"))
            .ContinueWith(_ => Guid.NewGuid());
    public Task Capture(Guid transactionId) 
        => Task.Delay(100).ContinueWith(_ => 
            Log.Logger.Information("PAYMENT_PROVIDER: Reserved amount captured")
        );
    public Task CancelReservation(Guid transactionId) 
        => Task.Delay(100).ContinueWith(_ => 
            Log.Logger.Information("PAYMENT_PROVIDER: Reservation cancelled")
        );
}