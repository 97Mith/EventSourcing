using Marten;
using Marten.Events.Aggregation;
using Marten.Events.Projections;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .Build();

        var store = DocumentStore.For(options =>
        {
            options.Connection("Server=localhost;Port=5432;UserId=postgres;Password=8545;Database=proto;");
            options.Projections.Add<Projector>(ProjectionLifecycle.Inline);
            options.Projections.Snapshot<Projection3>(SnapshotLifecycle.Async);
        });

        using var session = store.OpenSession();

        
        var cartResult = await CreateCartAsync(session);
        Console.WriteLine(cartResult);

        var appendResult = await AppendToCartAsync(session);
        Console.WriteLine(appendResult);

        var p1Result = await GetProjection1Async(session);
        Console.WriteLine(p1Result);

        var p2Result = await GetProjection2Async(session);
        Console.WriteLine(p2Result);

        
    }

    static async Task<string> CreateCartAsync(IDocumentSession session)
    {
        session.Events.StartStream(
            new AddedToCart("Under Pants", 5),
            new UptadedShippingInformation("Luiz F, 1880", "36436666")
            );

        await session.SaveChangesAsync();
        return "Cart created!";
    }

    static async Task<string> AppendToCartAsync(IDocumentSession session)
    {
        session.Events.Append(
            new Guid(" 018d83c1-eeb7-42a3-a7ba-24bfcb056798"),
            new AddedToCart("Jacket", 2)
            );

        await session.SaveChangesAsync();
        return "Item appended to cart!";
    }

    static async Task<string> GetProjection1Async(IDocumentSession session)
    {
        var projection1 = await session.Events.AggregateStreamAsync<Projection1>(new Guid("018d83c1-eeb7-42a3-a7ba-24bfcb056798"));
        return $"Projection1 Products: {string.Join(", ", projection1.Products)}";
    }

    static async Task<string> GetProjection2Async(IDocumentSession session)
    {
        var projection2 = await session.Events.AggregateStreamAsync<Projection2>(new Guid("018d83c1-eeb7-42a3-a7ba-24bfcb056798"));
        return $"Projection2 Total: {projection2.Total}";
    }

    
}


public class Projection1
{
    public Guid Id { get; set; }
    public List<string> Products { get; set; } = new();

    public void Apply(AddedToCart e)
    {
        Products.Add(e.Products);
    }
}

public class Projection2
{
    public Guid Id { get; set; }
    public int Total { get; set; }
}

public class Projector : SingleStreamProjection<Projection2>
{
    public void Apply(Projection2 snapshot, AddedToCart e)
    {
        snapshot.Total += e.Qty;
    }
}

public class Projection3
{
    public Guid Id { get; set; }
    public List<string> Products { get; set; } = new();
    public string PhoneNumber { get; set; }

    public void Apply(UptadedShippingInformation e)
    {
        PhoneNumber = e.PhoneNumber;
    }
}


public record AddedToCart(string Products, int Qty);
public record UptadedShippingInformation(string Address, string PhoneNumber);
