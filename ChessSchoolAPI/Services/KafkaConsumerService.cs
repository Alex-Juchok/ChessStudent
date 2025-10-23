using Confluent.Kafka;
using Newtonsoft.Json;

namespace ChessSchoolAPI.Services
{
    public class KafkaConsumerService 
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;
        private Task _backgroundTask;

        public KafkaConsumerService(IServiceProvider services, IConfiguration config)
        {
            _services = services;
            _config = config;
        }

        public void Start(CancellationToken token)
        {
            _backgroundTask = Task.Run(() => ConsumeLoop(token), token);
        }

        private async Task ConsumeLoop(CancellationToken token)
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _config["Kafka:Brokers"],
                GroupId = "user-service-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
            consumer.Subscribe("student.conformed");

            Console.WriteLine("✅ Kafka consumer listening...");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(token);
                    if (cr == null) continue;

                    var msg = JsonConvert.DeserializeObject<StudentConfirmationMessage>(cr.Message.Value);
                    Console.WriteLine($"📩 Получено сообщение: {msg?.studentId}");
                    
                    using var scope = _services.CreateScope();
                    var chessStudentService = scope.ServiceProvider.GetRequiredService<ChessStudentService>();

                    chessStudentService.ConfirmObject(msg?.studentId, msg?.confirmationTime);

                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Ошибка Kafka consumer: {ex.Message}");
                }
            }

            consumer.Close();
        }
    }


    public class StudentConfirmationMessage
    {
        public string studentId { get; set; }
        public string confirmationTime { get; set; }
    }
}
