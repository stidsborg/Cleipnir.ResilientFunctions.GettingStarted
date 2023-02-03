namespace Sample.WebApi.Ordering;

public static class IoCBindings
{
    public static void AddOrderProcessingBindings(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<OrderProcessor>();
        serviceCollection.AddTransient<OrderProcessor.Inner>();
        serviceCollection.AddSingleton<IEmailClient, EmailClientStub>();
        serviceCollection.AddSingleton<ILogisticsClient, LogisticsClientStub>();
        serviceCollection.AddSingleton<IPaymentProviderClient, PaymentProviderClientStub>();
    }
}