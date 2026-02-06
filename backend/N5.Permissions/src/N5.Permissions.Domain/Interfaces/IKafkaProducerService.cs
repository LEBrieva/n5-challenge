namespace N5.Permissions.Domain.Interfaces;

public interface IKafkaProducerService
{
    Task PublishOperationAsync(string operationName);
}
