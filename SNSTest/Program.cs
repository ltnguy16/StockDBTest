
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

using Amazon.SQS;
using Amazon.SQS.Model;


using System.Text.Json;

namespace SNSTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string topicName = "SNSTopicTest";
            string queueName = "QueueTest";
            IAmazonSimpleNotificationService client = new AmazonSimpleNotificationServiceClient();
            IAmazonSQS sqsClient = new AmazonSQSClient();
            
            string topicArn = "arn:aws:sns:us-east-1:495886275655:SNSTopicTest";
            string queueURL = "https://sqs.us-east-1.amazonaws.com/495886275655/QueueTest";
            string queueArn = "arn:aws:sqs:us-east-1:495886275655:QueueTest";
            string message = "This is an example message to publish to the ExampleSNSTopic.";

            //var topicArn = await CreateSNSTopicAsync(client, topicName);
            //Console.WriteLine($"New topic ARN: {topicArn}");

            //await PublishToTopicAsync(client, topicArn, message);

            //var attributes = await GetTopicAttributesAsync(client, tempTopicName);
            //DisplayTopicAttributes(attributes);

            //var queueURL = await CreateQueueAsync(sqsClient, queueName);
            //Console.WriteLine($"New topic ARN: {queueURL}");
            //await SubscribeQueueToSNS(client, topicArn, queueArn);

            await SubscribeLambdaToSNS(client, topicArn);

        }

        public static async Task SubscribeLambdaToSNS(IAmazonSimpleNotificationService client, string topicArn)
        {
            var request = new SubscribeRequest
            {
                TopicArn = topicArn,
                Endpoint = "arn:aws:execute-api:us-east-1:495886275655:t6i6w79qca/*/PUT/topic/add",
                Protocol = "Lambda",
            };
            await client.SubscribeAsync(request);
        }

        public static async Task SubscribeQueueToSNS(IAmazonSimpleNotificationService client, string topicArn, string queueArn)
        {
            var request = new SubscribeRequest
            {
                TopicArn = topicArn,
                Endpoint = queueArn,
                Protocol = "sqs",
            };

            await client.SubscribeAsync(request);
        }

        public static async Task<string> CreateQueueAsync(IAmazonSQS sqsClient, string queueName)
        {
            var request = new CreateQueueRequest
            {
                QueueName = queueName,
            };
            var response = await sqsClient.CreateQueueAsync(request);

            return response.QueueUrl;
        }

        public static async Task<string> CreateSNSTopicAsync(IAmazonSimpleNotificationService client, string topicName)
        {
            var request = new CreateTopicRequest
            {
                Name = topicName,
            };

            var response  = await client.CreateTopicAsync(request);

            return response.TopicArn;
        }

        public static async Task PublishToTopicAsync(IAmazonSimpleNotificationService client, string topicArn, string messageText)
        {
            var request = new PublishRequest
            {
                TopicArn = topicArn,
                Message = messageText,
            };

            var response = await client.PublishAsync(request);

            Console.WriteLine($"Successfully published message ID: {response.MessageId}");

        }

        public static async Task<Dictionary<string, string>> GetTopicAttributesAsync(IAmazonSimpleNotificationService client, string topicArn)
        {
            var response = await client.GetTopicAttributesAsync(topicArn);

            return response.Attributes;
        }

        public static void DisplayTopicAttributes(Dictionary<string, string> topicAttributes)
        {
            foreach (KeyValuePair<string, string> entry in topicAttributes)
            {
                Console.WriteLine($"{entry.Key}: {entry.Value}\n");
            }
        }
    }
}

