
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Text.Json;

namespace TableTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string id = Guid.NewGuid().ToString();
            string deleteid = "ee588811-efa1-438c-bf94-e5c2df6375b0";
            string findID = "ProductID_" + deleteid;
            string tableName = "ProductCatalog";

            //await createTable(tableName);
            //await addingItem(tableName, "Product", id);
            await deletingItem(tableName, "Product", findID);
            //await updatingItem(tableName, "Product", findID);

            //await getItems(tableName, "Product", findID);

            //await deleteTable(tableName);
        }

        static async Task deleteTable(string tableName)
        {
            Console.WriteLine("Starting Delete");
            AmazonDynamoDBClient client = new AmazonDynamoDBClient();
            

            var request = new DeleteTableRequest { TableName = tableName };
            //Console.WriteLine("check");
            var response = await client.DeleteTableAsync(request);
            Console.WriteLine("Finish Deleting");
        }

        static async Task createTable(string tableName)
        {
            Console.WriteLine("Start Create");
            AmazonDynamoDBClient client = new AmazonDynamoDBClient();
            
            var request = new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = new List<AttributeDefinition>()
              {
                new AttributeDefinition
                {
                    AttributeName = "Pk",
                    AttributeType = "S"
                },
                new AttributeDefinition
                {
                    AttributeName = "Sk",
                    AttributeType = "S"
                },
                /*
                new AttributeDefinition
                {
                    AttributeName = "Type",
                    AttributeType = "S"
                },
                new AttributeDefinition
                {
                    AttributeName = "ProductName",
                    AttributeType = "S"
                },
                new AttributeDefinition
                {
                    AttributeName = "DepartmentName",
                    AttributeType = "S"
                },
                new AttributeDefinition
                {
                    AttributeName = "Search",
                    AttributeType = "S"
                },*/
              },
                KeySchema = new List<KeySchemaElement>()
              {
                new KeySchemaElement
                {
                    AttributeName = "Pk",
                    KeyType = "HASH"  //Partition key
                },
                new KeySchemaElement
                {
                    AttributeName = "Sk",
                    KeyType = "RANGE"
                }
              },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 10,
                    WriteCapacityUnits = 5
                }
            };
            //Console.WriteLine("check");
            var response = await client.CreateTableAsync(request);
            Console.WriteLine("finish creating table");
        }

        static async Task addingItem(string tableName, string testType, string testID)
        {
            Console.WriteLine("Start Add");
            AmazonDynamoDBClient client = new AmazonDynamoDBClient();

            var request = new PutItemRequest
            {
                TableName = tableName,
                Item = new Dictionary<string, AttributeValue>()
                {
                    //assume that all Attribute Value will be pull from the addingItem fucntion parameter.
                    //Product ID will be generate from a "SortkeyFunction" that will then added to the
                      //addItem function parameter.
                    { "Pk", new AttributeValue { S = testType} },
                    { "Sk", new AttributeValue { S =  testType + "ID_"+ testID } },
                    { "ProductID", new AttributeValue { S = testID } },
                    { "ProductName", new AttributeValue { S = "Cabbage" } },
                    { "DepartmentName", new AttributeValue { S = "Produce" } },
                    { "Type", new AttributeValue { S = testType } },
                    { "Price", new AttributeValue { N = "4.76" } },
                    { "Quantity", new AttributeValue { N = "21"} },
                }
            };
            await client.PutItemAsync(request);
            Console.WriteLine("End Add");
        }

        static async Task deletingItem(string tableName, string testType, string testID)
        {
            Console.WriteLine("Start Delete Item");
            AmazonDynamoDBClient client = new AmazonDynamoDBClient();
            
            var request = new DeleteItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>() 
                {
                    { "Pk", new AttributeValue { S = testType } },
                    { "Sk", new AttributeValue { S = testID } } 
                },
            };

            var response = await client.DeleteItemAsync(request);
            Console.WriteLine("End Delete Item");
        }

        static async Task updatingItem(string tableName, string testType, string testID)
        {
            Console.WriteLine("Starting Update");
            AmazonDynamoDBClient client = new AmazonDynamoDBClient();

            var request = new UpdateItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>() 
                {
                    { "Pk", new AttributeValue { S = testType } },
                    { "Sk", new AttributeValue { S = testID } }
                },
                ExpressionAttributeNames = new Dictionary<string, string>()
                {
                    {"#N", "ProductName"},
                    {"#P", "Price"},
                    {"#D", "DepartmentName"},
                    {"#PD", "Department" }
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":n",new AttributeValue { S = "chair" } },
                    {":p",new AttributeValue { N = "5" } },
                    {":d",new AttributeValue { S = "Furniture" } },
                },

                // ADD for Adds a new attribute to the item
                // REMOVE for Removes
                UpdateExpression = "SET #P = #P + :p, #N = :n, #D = :d REMOVE #PD"
            };
            var response = await client.UpdateItemAsync(request);
            Console.WriteLine("End Update");
        }

        static async Task getItems(string tableName, string testType, string testID)
        {
            Console.WriteLine("start getting items");
            AmazonDynamoDBClient client = new AmazonDynamoDBClient();

            var request = new QueryRequest
            {
                TableName = tableName,
                KeyConditionExpression = "Pk = :v_Pk",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":v_Pk", new AttributeValue{ S = testType } }
                }
            };

            var response = await client.QueryAsync(request);
            
            /**
            Console.WriteLine(JsonSerializer.Serialize(response.Items));
            /**/
            foreach (Dictionary<string, AttributeValue> item in response.Items)
            {
                PrintItem(item);
            }
            /**/
            

            Console.WriteLine("End getting");
        }

        private static void PrintItem(Dictionary<string, AttributeValue> attributeList)
        {
            foreach (KeyValuePair<string, AttributeValue> kvp in attributeList)
            {
                string attributeName = kvp.Key;
                AttributeValue value = kvp.Value;

                Console.WriteLine(
                    attributeName + " " +
                    (value.S == null ? "" : "S=[" + value.S + "]") +
                    (value.N == null ? "" : "N=[" + value.N + "]") 
                    );
            }
            Console.WriteLine("************************************************");
        }
    }
}

