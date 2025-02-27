using gRPC_Receiver.Entity;
using System.Threading.Channels;

namespace gRPC_Receiver.Service
{
    public interface IReceiverService
    {
        /// <summary>
        /// Метод для асинхронного получения данных от gRPC сервера.
        /// </summary>
        Task ReceiveEntitiesAsync(CancellationToken cancellationToken);
    }
}
