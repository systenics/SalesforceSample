using Salesforce.Common;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesforceSample
{
    class Program
    {
        // 1. Add System.Configuration.dll
        // 2. Add DevloperForce.Force using NuGet Package Manager
        private static readonly string SecurityToken = System.Configuration.ConfigurationManager.AppSettings["SecurityToken"];
        private static readonly string ConsumerKey = System.Configuration.ConfigurationManager.AppSettings["ConsumerKey"];
        private static readonly string ConsumerSecret = System.Configuration.ConfigurationManager.AppSettings["ConsumerSecret"];
        private static readonly string Username = System.Configuration.ConfigurationManager.AppSettings["Username"];
        private static readonly string Password = System.Configuration.ConfigurationManager.AppSettings["Password"];
        private static readonly string IsSandboxUser = System.Configuration.ConfigurationManager.AppSettings["IsSandboxUser"];

        static void Main(string[] args)
        {
            try
            {
                var task = RunSample();
                task.Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                var innerException = e.InnerException;
                while (innerException != null)
                {
                    Console.WriteLine(innerException.Message);
                    Console.WriteLine(innerException.StackTrace);

                    innerException = innerException.InnerException;
                }
            }
        }

        private static async Task RunSample()
        {
            try
            {
                var auth = new AuthenticationClient();

                // Authenticate with Salesforce
                Console.WriteLine("Authenticating with Salesforce...");
                var url = IsSandboxUser.Equals("true", StringComparison.CurrentCultureIgnoreCase)
                    ? "https://login.salesforce.com/services/oauth2/token "
                    : "https://test.salesforce.com/services/oauth2/token";

                await auth.UsernamePasswordAsync(ConsumerKey, ConsumerSecret, Username, Password, url);
                Console.WriteLine("Connected to Salesforce.");
                var client = new ForceClient(auth.InstanceUrl, auth.AccessToken, auth.ApiVersion);
                // Retrieve all accounts
                Console.WriteLine("Get Accounts");
                // Initialization
                PhoneBook phoneBook = new PhoneBook();

                // Create Query
                var addPhoneData = new PhoneBook { Email__c = "john@gmail.com", FirstName__c = "John", LastName__c = "Doe", Phone__c = 9876543210 };
                var result = await client.CreateAsync(PhoneBook.SObjectTypeName, addPhoneData);
                if (result.Id == null)
                {
                    Console.WriteLine("Cannot insert data to Phonebook!");
                    return;
                }
                Console.WriteLine("Successfully added the record.");

                // Update Query
                var success = await client.UpdateAsync(PhoneBook.SObjectTypeName, result.Id, new { FirstName__c = "Tim" });
                if (!string.IsNullOrEmpty(success.Errors.ToString()))
                {
                    Console.WriteLine("Failed to update test record!");
                    return;
                }
                Console.WriteLine("Successfully updated the record.");

                // Delete Query
                Console.WriteLine("Deleting the record by ID.");
                var deleted = await client.DeleteAsync(PhoneBook.SObjectTypeName, result.Id);
                if (!deleted)
                {
                    Console.WriteLine("Failed to delete the record by ID!");
                    return;
                }
                Console.WriteLine("Successfully deleted the record.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

    }


    public class PhoneBook
    {
        public const String SObjectTypeName = "PhoneBook__c";

        public string Email__c { get; set; }
        public string FirstName__c { get; set; }
        public string LastName__c { get; set; }
        public long Phone__c { get; set; }
    }
}
