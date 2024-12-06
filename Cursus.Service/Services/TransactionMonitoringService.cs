using Cursus.RepositoryContract.Interfaces;
using Demo_PayPal.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cursus.Service.Services
{
    public class TransactionMonitoringService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TransactionMonitoringService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30); 

        public TransactionMonitoringService(IServiceScopeFactory scopeFactory, ILogger<TransactionMonitoringService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var payPalClient = scope.ServiceProvider.GetRequiredService<PayPalClient>(); 
                    _logger.LogInformation("Starting to check pending transactions at {Time}", DateTime.Now);

                    try
                    {
                        // Get pending transactions from the repository
                        var pendingTransactions = await unitOfWork.TransactionRepository.GetPendingTransactions();

                        if (!pendingTransactions.Any())
                        {
                            _logger.LogInformation("No pending transactions found at {Time}", DateTime.Now);
                        }

                        foreach (var transaction in pendingTransactions)
                        {
                           
                            if (transaction.DateCreated <= DateTime.Now.AddMinutes(-10))
                            {
                                _logger.LogInformation($"Transaction {transaction.TransactionId} exceeded the allowed time. Marking it as Failed.");

                               
                                await unitOfWork.TransactionRepository.UpdateTransactionStatus(transaction.TransactionId, Data.Enums.TransactionStatus.Failed);

                               
                                var order = await unitOfWork.OrderRepository.GetAsync(o => o.TransactionId == transaction.TransactionId);
                                if (order != null)
                                {
                                    await unitOfWork.OrderRepository.UpdateOrderStatus(order.OrderId, Data.Entities.OrderStatus.Failed);
                                    _logger.LogInformation($"Order {order.OrderId} associated with transaction {transaction.TransactionId} has been marked as Failed.");
                                }

                               
                                await unitOfWork.SaveChanges();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while checking pending transactions at {Time}", DateTime.Now);
                    }
                }

               
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
