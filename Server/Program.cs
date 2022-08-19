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

Console.Clear();


using var connection = factory.CreateConnection();

using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "request-queue", exclusive: false);

var consumer = new EventingBasicConsumer(channel);

consumer.Received += (model, ea) =>
{
    Console.WriteLine($"Received Request : {ea.BasicProperties.CorrelationId}");
    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

    var replyMessage = $"This is your reply : {ea.BasicProperties.CorrelationId}";

    var body = Encoding.UTF8.GetBytes(replyMessage);

    channel.BasicPublish("", ea.BasicProperties.ReplyTo, null, body);
    Console.WriteLine($"=> Send Reply : {ea.BasicProperties.CorrelationId}");
};

channel.BasicConsume(queue: "request-queue", autoAck: false, consumer: consumer);

Console.ReadKey();

