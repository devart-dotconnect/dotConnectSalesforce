# How to connect to Salesforce in .NET with C#

Based on [https://www.devart.com/dotconnect/salesforce/connect-to-salesforce.html](https://www.devart.com/dotconnect/salesforce/connect-to-salesforce.html)

This tutorial demonstrates how to integrate Salesforce with your .NET application using C#. Whether you're performing direct ADO.NET operations or building an object-relational model with EF Core, dotConnect for Salesforce provides a seamless, secure, and high-performance experience. We'll cover basic connections, credential management, and ORM-based development.

## Connect to Salesforce using C#

This section guides you through connecting to Salesforce using dotConnect for Salesforce and ADO.NET. You'll create a connection string, authenticate your session, and use ADO.NET classes like SalesforceConnection and SalesforceCommand to query data from your Salesforce organization.

```cs
using System;
using Devart.Data.Salesforce;

namespace SalesforceConnect
{
    class Program
    {
        static void Main(string[] args)
        {

            string connectionString = "" +
                "Authentication Type=AccessRefreshTokenInteractive;License Key=**********";

            using (var connection = new SalesforceConnection(connectionString))
            {
                connection.Open();

                string soql =
                    "SELECT Id, Name, Industry, BillingCity " +
                    "FROM Account " +
                    "WHERE Name LIKE @pattern " +
                    "ORDER BY Name " +
                    "LIMIT 15";

                using (var cmd = new SalesforceCommand(soql, connection))
                {
                    cmd.Parameters.AddWithValue("pattern", "U%");

                    using (var reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("Id\t\t\t\tName\t\t\tIndustry\tBillingCity");
                        Console.WriteLine("--------------------------------------------------------------------------");

                        while (reader.Read())
                        {
                            string id = reader["Id"].ToString();
                            string name = reader["Name"]?.ToString() ?? "";
                            string industry = reader["Industry"] == DBNull.Value ? "" : reader["Industry"].ToString();
                            string city = reader["BillingCity"] == DBNull.Value ? "" : reader["BillingCity"].ToString();

                            Console.WriteLine($"{id}\t{name,-24}\t{industry,-12}\t{city}");
                        }
                    }
                }
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}

```

## Connect to Salesforce using existing credentials

If you already have Salesforce credentials or an OAuth token, this section shows how to securely reuse them in your connection string. You'll also learn how to manage session expiration and refresh tokens for long-lived integrations.

```cs
using System;
using Devart.Data.Salesforce;

namespace SalesforceConnect
{
    class Program
    {
        static void Main(string[] args)
        {

            var clientId = Environment.GetEnvironmentVariable("SALESFORCE_CLIENT_ID") ?? "";
            var clientSecret = Environment.GetEnvironmentVariable("SALESFORCE_CLIENT_SECRET") ?? "";

            string connectionString =
                "Authentication Type=AccessRefreshTokenInteractive;" +
                "Host=https://test.develop.my.salesforce.com;" +
                "User Id=test@user.com;" +
                $"Client Id={clientId};" +
                $"Client Secret={clientSecret};";

            using (var connection = new SalesforceConnection(connectionString))
            {
                connection.Open();

                string soql =
                    "SELECT Id, Name, Industry, BillingCity " +
                    "FROM Account " +
                    "WHERE Name LIKE @pattern " +
                    "ORDER BY Name " +
                    "LIMIT 15";

                using (var cmd = new SalesforceCommand(soql, connection))
                {
                    cmd.Parameters.AddWithValue("pattern", "U%");

                    using (var reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("Id\t\t\t\tName\t\t\tIndustry\tBillingCity");
                        Console.WriteLine("--------------------------------------------------------------------------");

                        while (reader.Read())
                        {
                            string id = reader["Id"].ToString();
                            string name = reader["Name"]?.ToString() ?? "";
                            string industry = reader["Industry"] == DBNull.Value ? "" : reader["Industry"].ToString();
                            string city = reader["BillingCity"] == DBNull.Value ? "" : reader["BillingCity"].ToString();

                            Console.WriteLine($"{id}\t{name,-24}\t{industry,-12}\t{city}");
                        }
                    }
                }
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}

```

## Connect to Salesforce with EF Core

Leverage EF Core to simplify data access with strongly-typed models. You'll use LINQ to interact with Salesforce data as .NET objects, simplifying queries and CRUD operations.

```cs
using Microsoft.EntityFrameworkCore;
using Devart.Data.Salesforce.EFCore;

namespace EfCoreSalesforceDemo
{
  // Map to Salesforce Account object
  public class Account
  {
      public string Id { get; set; } = "";
      public string Name { get; set; } = "";
      public string? Industry { get; set; }
      public string? BillingCity { get; set; }
  }

  public class SalesforceContext : DbContext
  {
    private readonly string _connectionString;
    public SalesforceContext(string connectionString) => _connectionString = connectionString;
    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSalesforce(_connectionString);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<Account>();
        e.ToTable("Account"); // Salesforce object
        e.HasKey(a => a.Id);  // Primary key
        e.Property(a => a.Id).HasColumnName("Id");
        e.Property(a => a.Name).HasColumnName("Name");
        e.Property(a => a.Industry).HasColumnName("Industry");
        e.Property(a => a.BillingCity).HasColumnName("BillingCity");
    }
  }

  class Program
  {
    static void Main(string[] args)
    {
      // Fill in Password, Security Token, License Key
      var connectionString =
        "Authentication Type=AccessRefreshTokenInteractive;" +
        "License Key=**********";

      using var db = new SalesforceContext(connectionString);

      Console.WriteLine("Fetching first 15 accounts...\n");

      var accounts = db.Accounts
                        .OrderBy(a => a.Name)
                        .Select(a => new { a.Id, a.Name, a.Industry, a.BillingCity })
                        .Take(15)
                        .ToList();

      Console.WriteLine("Id\t\t\t\tName\t\t\tIndustry\tBillingCity");
      Console.WriteLine("------------------------------------------");

      foreach (var a in accounts)
        Console.WriteLine($"{a.Id}\t{a.Name,-24}\t{a.Industry,-12}\t{a.BillingCity}");

      Console.WriteLine("\nDone. Press any key to exit...");
      Console.ReadKey();
    }
  }
}
```