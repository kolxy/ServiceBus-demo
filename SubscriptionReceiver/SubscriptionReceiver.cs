using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Demo.Config;

Config config = new Config();

// the client that owns the connection and can be used to create senders and receivers
ServiceBusClient client;

// the processor that reads and processes messages from the subscription
ServiceBusProcessor processor;

// handle received messages
async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();
    Console.WriteLine($"Received: {body}");

    // complete the message. messages is deleted from the subscription. 
    await args.CompleteMessageAsync(args.Message);
}

// handle any errors when receiving messages
Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
}

// The Service Bus client types are safe to cache and use as a singleton for the lifetime
// of the application, which is best practice when messages are being published or read
// regularly.
//
// Create the clients that we'll use for sending and processing messages.
// TODO: Replace the <NAMESPACE-CONNECTION-STRING> placeholder
client = new ServiceBusClient(config.get("SERVICE_BUS_CONNECTION_STRING"));

// create a processor that we can use to process the messages
// TODO: Replace the <TOPIC-NAME> and <SUBSCRIPTION-NAME> placeholders
processor = client.CreateProcessor(config.get("SERVICE_BUS_TOPIC_NAME"), config.get("SERVICE_BUS_SUBSCRIPTION_NAME"), new ServiceBusProcessorOptions());

try
{
    // add handler to process messages
    processor.ProcessMessageAsync += MessageHandler;

    // add handler to process any errors
    processor.ProcessErrorAsync += ErrorHandler;

    // start processing 
    await processor.StartProcessingAsync();

    Console.WriteLine($"Listening to Subscription {config.get("SERVICE_BUS_SUBSCRIPTION_NAME")}. Press any key to exit");
    Console.ReadKey();

    // stop processing 
    Console.WriteLine("\nStopping the receiver...");
    await processor.StopProcessingAsync();
    Console.WriteLine("Stopped receiving messages");
}
finally
{
    // Calling DisposeAsync on client types is required to ensure that network
    // resources and other unmanaged objects are properly cleaned up.
    await processor.DisposeAsync();
    await client.DisposeAsync();
}