using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace ChessSchoolAPI.Services
{
    public class KafkaConsumerHostedService : IHostedService
    {
        private readonly KafkaConsumerService _consumer;
        private CancellationTokenSource _cts;

        public KafkaConsumerHostedService(KafkaConsumerService consumer)
        {
            _consumer = consumer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _consumer.Start(_cts.Token);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            return Task.CompletedTask;
        }
    }

}
