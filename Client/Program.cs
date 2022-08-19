using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory()
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest",
    Port = 5672,
    AutomaticRecoveryEnabled = true,
    VirtualHost = "DemoApp"
};


using var connection = factory.CreateConnection();

using var channel = connection.CreateModel();

var replyQueue = channel.QueueDeclare(queue: "", exclusive: true);
channel.QueueDeclare(queue: "request-queue", exclusive: false);

var consumer = new EventingBasicConsumer(channel);

consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"<= Reply recieved : {message}");
    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
};

channel.BasicConsume(queue: replyQueue.QueueName, autoAck:false, consumer: consumer);

#region send request


for (int i = 0; i < 100; i++)
{
    var message = $"Can I request a reply";
    var body = Encoding.UTF8.GetBytes(message);

    var properties = channel.CreateBasicProperties();
    properties.ReplyTo = replyQueue.QueueName;
    properties.CorrelationId = Guid.NewGuid().ToString();

    channel.BasicPublish("",  "request-queue", properties, body);

    Console.WriteLine($"=>Sending Request : {properties.CorrelationId}" );

    await Task.Delay(5000);
}

#endregion

Console.WriteLine("Started Client");

Console.ReadKey();