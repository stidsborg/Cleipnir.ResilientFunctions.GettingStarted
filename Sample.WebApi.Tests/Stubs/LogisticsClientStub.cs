using Sample.WebApi.Ordering;

namespace Sample.WebApi.Tests.Stubs;

public class LogisticsClientStub : ILogisticsClient
{
    public List<Tuple<Guid, IEnumerable<Guid>>> ShipProductsInvocations = new();
    
    public Task ShipProducts(Guid customerId, IEnumerable<Guid> productIds)
    {
        ShipProductsInvocations.Add(Tuple.Create(customerId, productIds));
        return Task.CompletedTask;
    }
}