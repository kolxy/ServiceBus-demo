using Azure.Messaging.ServiceBus;
using Demo.Config;

Config config = new Config();
// the client that owns the connection and can be used to create senders and receivers
ServiceBusClient client;

// the sender used to publish messages to the queue
ServiceBusSender sender;

// The Service Bus client types are safe to cache and use as a singleton for the lifetime
// of the application, which is best practice when messages are being published or read
// regularly.
//
// set the transport type to AmqpWebSockets so that the ServiceBusClient uses the port 443. 
// If you use the default AmqpTcp, you will need to make sure that the ports 5671 and 5672 are open

// TODO: Replace the <NAMESPACE-CONNECTION-STRING> and <QUEUE-NAME> placeholders
var clientOptions = new ServiceBusClientOptions()
{
    TransportType = ServiceBusTransportType.AmqpWebSockets
};
client = new ServiceBusClient(config.get("SERVICE_BUS_CONNECTION_STRING"), clientOptions);
sender = client.CreateSender(config.get("SERVICE_BUS_TOPIC_NAME"));

// create a batch 
using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
Console.WriteLine($"Topic sending to {config.get("SERVICE_BUS_TOPIC_NAME")}");
try
{
    // Use the producer client to send the batch of messages to the Service Bus queue
    bool end = false;
    while (!end)
    {
        string? input = Console.ReadLine();
        if (input != null && input.ToLower() == "exit")
        {
            end = true;
        }
        else
        {
            await sender.SendMessageAsync(new ServiceBusMessage(input));
            Console.WriteLine($"Content \"{input}\" has been sent to service bus");
        }
    }
}
finally
{
    // Calling DisposeAsync on client types is required to ensure that network
    // resources and other unmanaged objects are properly cleaned up.
    await sender.DisposeAsync();
    await client.DisposeAsync();
}