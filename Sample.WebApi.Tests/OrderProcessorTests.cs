using Cleipnir.ResilientFunctions.Domain;
using Sample.WebApi.Ordering;
using Shouldly;
using EmailClientStub = Sample.WebApi.Tests.Stubs.EmailClientStub;
using LogisticsClientStub = Sample.WebApi.Tests.Stubs.LogisticsClientStub;
using PaymentProviderClientStub = Sample.WebApi.Tests.Stubs.PaymentProviderClientStub;

namespace Sample.WebApi.Tests;

[TestClass]
public class OrderProcessorTests
{
    private PaymentProviderClientStub PaymentProviderClientStub { get; set; } = null!;
    private LogisticsClientStub LogisticsClientStub { get; set; } = null!;
    private EmailClientStub EmailClientStub { get; set; } = null!;
    
    [TestInitialize]
    public void InitializeTest()
    {
        PaymentProviderClientStub = new PaymentProviderClientStub();
        LogisticsClientStub = new LogisticsClientStub();
        EmailClientStub = new EmailClientStub();
    }
    
    [TestMethod]
    public async Task OrderProcessorSunshineScenario()
    {
        var sut = new OrderProcessor.Inner(
            PaymentProviderClientStub,
            EmailClientStub,
            LogisticsClientStub
        );

        var order = new Order(
            OrderId: "MK-54321",
            CustomerId: Guid.NewGuid(),
            ProductIds: new[] { Guid.NewGuid(), Guid.NewGuid() },
            TotalPrice: 120M
        );
        var scrapbookSaved = false;
        var scrapbook = new OrderProcessor.Scrapbook();
        scrapbook.Initialize(onSave: () => { scrapbookSaved = true; return Task.CompletedTask; });

        await sut.ProcessOrder(order, scrapbook);
        scrapbookSaved.ShouldBeTrue();
        scrapbook.ProductsShippedStatus.ShouldBe(WorkStatus.Completed);
        
        PaymentProviderClientStub.CaptureInvocations.Count.ShouldBe(1);
        PaymentProviderClientStub.ReserveInvocations.Count.ShouldBe(1);
        PaymentProviderClientStub.CancelReservationInvocations.Count.ShouldBe(0);
        
        LogisticsClientStub.ShipProductsInvocations.Single().ShouldBe(Tuple.Create(order.CustomerId, order.ProductIds));
        EmailClientStub.SendOrderConfirmationInvocations.Single().ShouldBe(Tuple.Create(order.CustomerId, order.ProductIds));
    }
    
    [TestMethod]
    public async Task OrderProcessorFailsOnRetryWhenLogisticsWorkHasStartedButNotCompleted()
    {
        var sut = new OrderProcessor.Inner(
            PaymentProviderClientStub,
            EmailClientStub,
            LogisticsClientStub
        );

        var order = new Order(
            OrderId: "MK-54321",
            CustomerId: Guid.NewGuid(),
            ProductIds: new[] { Guid.NewGuid(), Guid.NewGuid() },
            TotalPrice: 120M
        );
        var scrapbookSaved = false;
        var scrapbook = new OrderProcessor.Scrapbook
        {
            TransactionId = Guid.NewGuid(),
            ProductsShippedStatus = WorkStatus.Started
        };
        scrapbook.Initialize(onSave: () => { scrapbookSaved = true; return Task.CompletedTask; });

        await Should.ThrowAsync<InvalidOperationException>(() => sut.ProcessOrder(order, scrapbook));
        
        scrapbookSaved.ShouldBeFalse();
        scrapbook.ProductsShippedStatus.ShouldBe(WorkStatus.Started);
        EmailClientStub.SendOrderConfirmationInvocations.ShouldBeEmpty();
        LogisticsClientStub.ShipProductsInvocations.ShouldBeEmpty();
        PaymentProviderClientStub.ReserveInvocations.Count.ShouldBe(1);
        PaymentProviderClientStub.CaptureInvocations.ShouldBeEmpty();
        PaymentProviderClientStub.CancelReservationInvocations.ShouldBeEmpty();
    }
    
    [TestMethod]
    public async Task ScrapbookSpecifiedTransactionIdIsUsedOnRetry()
    {
        var transactionId = Guid.NewGuid();

        var sut = new OrderProcessor.Inner(
            PaymentProviderClientStub,
            EmailClientStub,
            LogisticsClientStub
        );

        var order = new Order(
            OrderId: "MK-54321",
            CustomerId: Guid.NewGuid(),
            ProductIds: new[] { Guid.NewGuid(), Guid.NewGuid() },
            TotalPrice: 120M
        );
        var scrapbookSaved = false;
        var scrapbook = new OrderProcessor.Scrapbook { TransactionId = transactionId }; 
        scrapbook.Initialize(onSave: () => { scrapbookSaved = true; return Task.CompletedTask; });

        await sut.ProcessOrder(order, scrapbook);
        
        scrapbookSaved.ShouldBeTrue();
        scrapbook.ProductsShippedStatus.ShouldBe(WorkStatus.Completed);
        
        PaymentProviderClientStub.CaptureInvocations.Single().ShouldBe(transactionId);
        PaymentProviderClientStub.ReserveInvocations.Single().ShouldBe(Tuple.Create(transactionId, order.CustomerId, order.TotalPrice));
        PaymentProviderClientStub.CancelReservationInvocations.ShouldBeEmpty();
    }
}