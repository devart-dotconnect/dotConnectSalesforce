using Microsoft.EntityFrameworkCore;
using Devart.Data.Salesforce.EFCore;

namespace EfCoreSalesforceDemo
{
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