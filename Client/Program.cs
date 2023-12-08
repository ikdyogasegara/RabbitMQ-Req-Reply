using System.Text;
using Microsoft.VisualBasic;
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

var replyQueue = channel.QueueDeclare(queue: "", exclusive: true);
channel.QueueDeclare(queue: "request-queue", exclusive: false);

var consumer = new EventingBasicConsumer(channel);

consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"<= Mendapatkan Balasan : {message}");
    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
};

channel.BasicConsume(queue: replyQueue.QueueName, autoAck:false, consumer: consumer);

#region send request


for (var i = 0; i < 100; i++)
{
    var message = $"Halooo {DateTime.Now:yyyMMddHHmmss}";
    var body = Encoding.UTF8.GetBytes(message);

    var properties = channel.CreateBasicProperties();
    properties.ReplyTo = replyQueue.QueueName;
    properties.CorrelationId = Guid.NewGuid().ToString();

    channel.BasicPublish("",  "request-queue", properties, body);

    Console.WriteLine($"[{properties.CorrelationId}] Mengirim Permintaan => {message}" );

    await Task.Delay(5000);
}

#endregion

Console.WriteLine("Started Client");

Console.ReadKey();