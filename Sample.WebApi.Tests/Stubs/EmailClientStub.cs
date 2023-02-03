using Sample.WebApi.Ordering;

namespace Sample.WebApi.Tests.Stubs;

public class EmailClientStub : IEmailClient
{
    public List<Tuple<Guid, IEnumerable<Guid>>> SendOrderConfirmationInvocations { get; } = new();
    public Task SendOrderConfirmation(Guid customerId, IEnumerable<Guid> productIds)
    {
        SendOrderConfirmationInvocations.Add(Tuple.Create(customerId, productIds));
        return Task.CompletedTask;
    }
}