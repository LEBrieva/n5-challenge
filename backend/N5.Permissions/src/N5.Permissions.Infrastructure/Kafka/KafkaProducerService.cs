using System.Text.Json;
using Confluent.Kafka;
using N5.Permissions.Domain.Interfaces;

namespace N5.Permissions.Infrastructure.Kafka;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<string, string> _producer;
    private const string TopicName = "permissions-operations";

    public KafkaProducerService(IProducer<string, string> producer)
    {
        _producer = producer;
    }

    public async Task PublishOperationAsync(string operationName)
    {
        var message = new
        {
            Id = Guid.NewGuid(),
            NameOperation = operationName
        };

        await _producer.ProduceAsync(TopicName, new Message<string, string>
        {
            Key = message.Id.ToString(),
            Value = JsonSerializer.Serialize(message)
        });
    }
}
