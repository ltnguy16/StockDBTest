using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

using System.Text.Json;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.SNSEvents;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace StockDBTest
{
    public class Functions
    {
        // This const is the name of the environment variable that the serverless.template will use to set
        // the name of the DynamoDB table used to store blog posts.
        const string TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP = "BlogTable";

        public const string ID_QUERY_STRING_NAME = "Id";
        public const string ID_QUERY_STRING_NAME_TEST = "ProductId";
        IDynamoDBContext DDBContext { get; set; }

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {
            // Check to see if a table name was passed in through environment variables and if so 
            // add the table mapping.
            var tableName = System.Environment.GetEnvironmentVariable(TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP);
            if(!string.IsNullOrEmpty(tableName))
            {
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(Blog)] = new Amazon.Util.TypeMapping(typeof(Blog), tableName);
            }

            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            this.DDBContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        /// <summary>
        /// Constructor used for testing passing in a preconfigured DynamoDB client.
        /// </summary>
        /// <param name="ddbClient"></param>
        /// <param name="tableName"></param>
        public Functions(IAmazonDynamoDB ddbClient, string tableName)
        {
            if (!string.IsNullOrEmpty(tableName))
            {
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(Blog)] = new Amazon.Util.TypeMapping(typeof(Blog), tableName);
            }

            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            this.DDBContext = new DynamoDBContext(ddbClient, config);
        }

        /*=========================================================================== Loi's Edits ================================================================================================*/
        public async Task<APIGatewayProxyResponse> GetItems(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var headers = new Dictionary<string, string>
            {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
            };

            AmazonDynamoDBClient client = new AmazonDynamoDBClient();
            var query = new QueryRequest
            {
                TableName = "ProductCatalog",
                KeyConditionExpression = "Pk = :v_Pk",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":v_Pk", new AttributeValue{ S = "Product" } }
                }
            };

            List<ProductModel> lst = new List<ProductModel>();
            var items = await client.QueryAsync(query);
            foreach (var item in items.Items)
            {
                lst.Add(Getvalue(item));
            }

            var response = new APIGatewayProxyResponse
            {
                
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonSerializer.Serialize(lst),
                // remove newton soft, use system.text.json
                Headers = headers,
            };

            return response;
        }

        public async Task<APIGatewayProxyResponse> AddItem(APIGatewayProxyRequest request, ILambdaContext context)
        {
            Console.WriteLine("Start Add");
            var newItem = JsonSerializer.Deserialize<ProductModel>(request?.Body);
            AmazonDynamoDBClient client = new AmazonDynamoDBClient();
            string id = Guid.NewGuid().ToString();
            var item = new PutItemRequest
            {
                TableName = "ProductCatalog",
                Item = new Dictionary<string, AttributeValue>()
                {
                    { "Pk", new AttributeValue { S = newItem.Type} },
                    { "Sk", new AttributeValue { S =  newItem.Type + "ID_"+ id } },
                    { "ProductID", new AttributeValue { S = id } },
                    { "ProductName", new AttributeValue { S = newItem.ProductName } },
                    { "DepartmentName", new AttributeValue { S = newItem.DepartmentName } },
                    { "Type", new AttributeValue { S = newItem.Type } },
                    { "Price", new AttributeValue { N = newItem.Price.ToString() } },
                    { "Quantity", new AttributeValue { N = newItem.Quantity.ToString() } },
                }
            };
            await client.PutItemAsync(item);
            
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonSerializer.Serialize(item),
                Headers = new Dictionary<string, string> 
                { 
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                }
            };
            return response;
            
        }

        public async Task<APIGatewayProxyResponse> DeleteItem(APIGatewayProxyRequest request, ILambdaContext context)
        {
            AmazonDynamoDBClient client = new AmazonDynamoDBClient();
            var newItem = JsonSerializer.Deserialize<ProductModel>(request?.Body);

            var item = new DeleteItemRequest
            {
                TableName = "ProductCatalog",
                Key = new Dictionary<string, AttributeValue>()
                {
                    { "Pk", new AttributeValue { S = newItem.Type } },
                    { "Sk", new AttributeValue { S = newItem.Type + "ID_"+ newItem.ProductId } }
                },
            };
            await client.DeleteItemAsync(item);
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonSerializer.Serialize(item),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                }
            };
            return response;
        }

        public async Task<APIGatewayProxyResponse> UpdateItem(APIGatewayProxyRequest request, ILambdaContext context)
        {
            AmazonDynamoDBClient client = new AmazonDynamoDBClient();
            var newItem = JsonSerializer.Deserialize<ProductModel>(request?.Body);

            var item = new UpdateItemRequest
            {
                TableName = "ProductCatalog",
                Key = new Dictionary<string, AttributeValue>()
                {
                    { "Pk", new AttributeValue { S = newItem.Type } },
                    { "Sk", new AttributeValue { S = newItem.Type + "ID_"+ newItem.ProductId } }
                },
                ExpressionAttributeNames = new Dictionary<string, string>()
                {
                    {"#N", "ProductName"},
                    {"#D", "DepartmentName"},
                    {"#P", "Price"},
                    {"#Q", "Quantity" }
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":n", new AttributeValue { S = newItem.ProductName } },
                    {":d", new AttributeValue { S = newItem.DepartmentName } },
                    {":p", new AttributeValue { N = newItem.Price.ToString() } },
                    {":q",  new AttributeValue { N = newItem.Quantity.ToString() } },
                },

                // ADD for Adds a new attribute to the item
                // REMOVE for Removes
                UpdateExpression = "SET #N = :n, #D = :d, #P = :p, #Q = :q"
            };
            await client.UpdateItemAsync(item);
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonSerializer.Serialize(item),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                }
            };
            return response;
        }



        public async Task<APIGatewayProxyResponse> AddTopic(APIGatewayProxyRequest request, ILambdaContext context)
        {
            Console.WriteLine("Start AddTopic");
            IAmazonSimpleNotificationService client = new AmazonSimpleNotificationServiceClient();
            
            var message = (request?.Body.ToString());

            var topic = new PublishRequest
            {
                TopicArn = "arn:aws:sns:us-east-1:495886275655:SNSTopicTest",
                Message = message,
            };
            Console.WriteLine("finish topic");
            await client.PublishAsync(topic);
            //Console.WriteLine($"Successfully published message ID: {test.MessageId}");
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonSerializer.Serialize(topic),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                }
            };
            return response;
        }

        /*
        Description:
            This function will be triggered as an api, in otherword it depend on user interaction.
            Once invoked, the logic will publish an sns event.
            The sns event is basically create something similar to a ripple effect where anys functions exist on aws that subscript the this specific sns event will be automatically invoke
         */

        // sns topic, created from serverless template
        private readonly string _topicArn = "arn:aws:sns:us-east-1:495886275655:Inventory-Test-TestStockSNSTopic-I1ZBBK1T0Q0F";
        
        public async Task<APIGatewayProxyResponse> AddTopicWithSNS_Sample(APIGatewayProxyRequest request, ILambdaContext context)
        {
            // OTHER LOGIC GOES HERE
            ///////////////////////////////
            // set up sns sclient
            IAmazonSimpleNotificationService snsClient = new AmazonSimpleNotificationServiceClient();
            // publish request
            // Topic arn is required, think of it like an sns unique identifier
            // Message, think of a like a parameter in API where data will be used to perform any specific logic
            // Mesasage Attribute, it is a filter policy
                // Exactly like the name implies (FILTER POLICY), meaning once this sns published, only those sns subscription that include this FILTER POLICY wil be invoked
            PublishRequest publishRequest = new PublishRequest()
            {
                TopicArn = _topicArn,
                Message = JsonSerializer.Serialize("My First Sns Messsage"),
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        "MY_FIRST_EVENT",
                        new MessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = "Add_Topic_Event"
                        }
                    }
                }
            };

            try
            {
                // Publishing the SNS
                var publishResponse = await snsClient.PublishAsync(publishRequest).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            ///////////////////////////////
            var response = new APIGatewayProxyResponse
            {

                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonSerializer.Serialize("696969"),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                }
            };
            return response;
        }


        /*
            This is subcription function, and it will be triggered whenever AddTopicWithSNS_Sample was invoked
            Most the setup for this function is being done on the serverless template
         */
        public Task SubcribeAddTopSns_Sample(SNSEvent snsEvent, ILambdaContext context)
        {

            var snsResponse = snsEvent.Records.Select(x => JsonSerializer.Deserialize<string>(x.Sns.Message)).ToList();
            foreach(var item in snsResponse)
            {
                Console.WriteLine(item);
            }

            return Task.CompletedTask;
        }

        /*
            This is another test function, this one is just basically to show how to subscripe manually
            If you pay attention to the function parameter, we utilizes APIGateway. Hence, this one is treated an api and will be invoked by the USER
            
            Note: you will never see this type of manual subscription in the actual work enviroment, as sns subscript tends to be triggered automatically (microservice)
         */
        public async Task<APIGatewayProxyResponse> SubcribeAddTopSns_Sample_TriggerWithApi(APIGatewayProxyRequest request, ILambdaContext context)
        {
            IAmazonSimpleNotificationService client = new AmazonSimpleNotificationServiceClient();

            //sns reponse to subscribe to 
            var snsReponse = new SubscribeRequest
            {
                // sns topic
                TopicArn = _topicArn,
                // arn of a lambda functions
                Endpoint = "arn:aws:lambda:us-east-1:495886275655:function:Inventory-Test-SubcribeItemWithSns-s66OvoZEBHtH",
                Protocol = "Lambda",
            };
            SubscribeResponse sub = await client.SubscribeAsync(snsReponse);
            Console.WriteLine(sub.SubscriptionArn);

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonSerializer.Serialize("SubcribeAddTopSns_Sample_TriggerWithApi"),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                }
            };
            return response;
        }


        private static ProductModel Getvalue(Dictionary<string, AttributeValue> attributeList)
        {
            ProductModel model = new ProductModel();
            foreach (KeyValuePair<string, AttributeValue> attribute in attributeList)
            {
                string attributeName = attribute.Key;
                AttributeValue attributeValue = attribute.Value;
                if (attributeName == "ProductID")
                    model.ProductId = attributeValue.S;
                else if (attributeName == "ProductName")
                    model.ProductName = attributeValue.S;
                else if (attributeName == "DepartmentName")
                    model.DepartmentName = attributeValue.S;
                else if (attributeName == "Price")
                    model.Price = Convert.ToDouble(attributeValue.N);
                else if (attributeName == "Quantity")
                    model.Quantity = Int32.Parse(attributeValue.N);
                else if (attributeName == "Type")
                    model.Type = attributeValue.S;
            }

            return model;
        }
    }
}
