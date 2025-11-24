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