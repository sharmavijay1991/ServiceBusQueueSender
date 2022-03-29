
using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace ServiceBusQueueSender
{
    public class URLData { public int id { get; set; } public string url { get; set; } public int epoch { get; set; } public int status_code { get; set; }
        public URLData(int p_seq, string p_url, int p_epoch, int p_status_code)
        {
            id = p_seq;
            url = p_url;
            epoch = p_epoch;
            status_code = p_status_code;
        }
    }
    /*{
        int seq;
        string url;
        public URLData(int p_seq, string p_url)
        {
            url = p_url;
            seq = p_seq;
        } 
    }*/

    class Program
    {
        static string connectionString = "<Service Bus Connection String>";

        // name of your Service Bus queue
        static string queueName = "<QueueName>";

        // the client that owns the connection and can be used to create senders and receivers
        static ServiceBusClient client;

        // the sender used to publish messages to the queue
        static ServiceBusSender sender;

        // number of messages to be sent to the queue
        private const int run_duration = 180;

        //Vijay Objects
        static URLData msg;

        static async Task Main(string[] args)
        {
            // The Service Bus client types are safe to cache and use as a singleton for the lifetime
            // of the application, which is best practice when messages are being published or read
            // regularly.
            //
            // Create the clients that we'll use for sending and processing messages.
            client = new ServiceBusClient(connectionString);
            sender = client.CreateSender(queueName);

            // create a batch 
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
            TimeSpan epoch_base_time = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int starting_epoch = (int)epoch_base_time.TotalSeconds;
            int end_epoc = starting_epoch + run_duration;
            try
            {
                for (int i = 1; starting_epoch < end_epoc; i++)
                {
                    epoch_base_time = DateTime.UtcNow - new DateTime(1970, 1, 1);
                    starting_epoch = (int)epoch_base_time.TotalSeconds;

                    msg = new URLData(i, $"www.myurl-test.com/page/{i}",starting_epoch, 999);
                    string str_msg = JsonConvert.SerializeObject(msg);
                    Console.WriteLine("Sending string data json:" + str_msg);
                    ServiceBusMessage msg_to_send = new ServiceBusMessage(str_msg);
                    await sender.SendMessageAsync(msg_to_send);
                    Thread.Sleep(1000);
                }
            }

            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }

            Console.WriteLine("Press any key to end the application");
            Console.ReadKey();
        }
    }
}
