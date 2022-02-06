
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string tableName = "ProductCatalog";
            createTable(tableName);  
            //deleteTable(tableName);
        }

        static async void deleteTable(string tableName)
        {
            AmazonDynamoDBClient client = new AmazonDynamoDBClient();
            //string tableName = "ProductCatalog";

            var request = new DeleteTableRequest { TableName = tableName };
            var response = await client.DeleteTableAsync(request);
            Console.WriteLine("Finish Deleting");
        }

        static async void createTable(string tableName)
        {
            AmazonDynamoDBClient client = new AmazonDynamoDBClient();
            //string tableName = "ProductCatalog";

            var request = new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = new List<AttributeDefinition>()
              {
                new AttributeDefinition
                {
                  AttributeName = "Type",
                  AttributeType = "S"
                }
              },
                KeySchema = new List<KeySchemaElement>()
              {
                new KeySchemaElement
                {
                  AttributeName = "Type",
                  KeyType = "HASH"  //Partition key
                }
              },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 10,
                    WriteCapacityUnits = 5
                }
            };

            var response = await client.CreateTableAsync(request);
            Console.WriteLine("finish creating table");
        }
    }
}

