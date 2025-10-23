using Confluent.Kafka;
using Newtonsoft.Json;

namespace UserService.Services
{
    public class KafkaProducerService
    {
        private readonly IProducer<Null, string> _producer;

        public KafkaProducerService(IConfiguration config)
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = config["Kafka:Brokers"] ?? "localhost:9092"
            };

            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        }

        public async Task SendMessageAsync(string topic, object message)
        {
            string json = JsonConvert.SerializeObject(message);
            await _producer.ProduceAsync(topic, new Message<Null, string> { Value = json });
        }
    }
}
