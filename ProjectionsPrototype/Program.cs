using Marten;
using Marten.Events.Aggregation;
using Marten.Events.Projections;
using Microsoft.Extensions.Configuration;
using ProjectionsPrototype;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
        using var query = store.QuerySession();

        
        while (true)
        {
            string choose = View.Menu();
            Console.Clear();


            if (choose.Equals("1"))
            {
                string productName = View.SetQuestion("Add produtc"); Console.Clear();
                int quantity = View.SetQuantity("Add quantity"); Console.Clear();
                string address = View.SetQuestion("Add address"); Console.Clear();
                string pNumber = View.SetQuestion("Add phoneNumber"); Console.Clear();
                View.Loading(); Console.Clear();

                var cartResult = await CreateCartAsync(session, productName, quantity, address, pNumber);
                Console.WriteLine(cartResult);

                var appendResult = await AppendToCartAsync(session, productName, quantity);
                Console.WriteLine(appendResult);

                var p1Result = await GetProjection1Async(session);
                Console.WriteLine(p1Result);

                var p2Result = await GetProjection2Async(session);
                Console.WriteLine(p2Result);
            }
            else if (choose.Equals("2"))
            {
                var projection1Results = await session.Query<Projection1>().ToListAsync();
                Console.WriteLine("Projection1 Results:");
                foreach (var result in projection1Results)
                {
                    Console.WriteLine($"Id: {result.Id}, Products: {string.Join(", ", result.Products)}");
                }

                var projection2Results = await session.Query<Projection2>().ToListAsync();
                Console.WriteLine("\nProjection2 Results:");
                foreach (var result in projection2Results)
                {
                    Console.WriteLine($"Id: {result.Id}, Total: {result.Total}");
                }
            }
            else
            {
                return;
            }

        }
        
    }

    static async Task<string> CreateCartAsync(IDocumentSession session, string productName, int quantity, string address, string phoneNumber)
    {
        session.Events.StartStream(
            new AddedToCart(productName, quantity),
            new UptadedShippingInformation(address, phoneNumber)
            );

        await session.SaveChangesAsync();
        return "Cart created!";
    }

    static async Task<string> AppendToCartAsync(IDocumentSession session, string productName, int quantity)
    {
        session.Events.Append(
            new Guid(" 018d83c1-eeb7-42a3-a7ba-24bfcb056798"),
            new AddedToCart(productName, quantity)
            );

        await session.SaveChangesAsync();
        return "Item adicionado ao carrinho!";
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
