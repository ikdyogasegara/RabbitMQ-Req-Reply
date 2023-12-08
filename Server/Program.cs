using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory()
{
    HostName = "localhost",
    UserName = "sipintarv5",
    Password = "sipintarv5",
    Port = 5672,
    AutomaticRecoveryEnabled = true,
    VirtualHost = "pegasusv2"
};

using var connection = factory.CreateConnection();

using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "request-queue", exclusive: false);

var consumer = new EventingBasicConsumer(channel);

consumer.Received += (model, ea) =>
{
    var bodyClient = ea.Body.ToArray();
    var clientMessage = Encoding.UTF8.GetString(bodyClient);

    Console.WriteLine($"[{ea.BasicProperties.CorrelationId}] Permintaan Masuk : {clientMessage}");
    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

    var replyMessage = $"Halo Juga untuk {clientMessage}";
    var body = Encoding.UTF8.GetBytes(replyMessage);

    channel.BasicPublish("", ea.BasicProperties.ReplyTo, null, body);
    Console.WriteLine($"=> [{ea.BasicProperties.CorrelationId}] Membalas Permintaan : {replyMessage}");
};

channel.BasicConsume(queue: "request-queue", autoAck: false, consumer: consumer);

Console.ReadKey();

