
using Microsoft.Extensions.Logging;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Data.Enums;
using Cursus.Data.Entities;
using Cursus.ServiceContract.Interfaces;
using PayPalCheckoutSdk.Orders;
using Microsoft.AspNetCore.Http;
using Cursus.Data.DTO;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;
using Cursus.Repository.Repository;


namespace Demo_PayPal.Service
{
    public class PaymentService: IPaymentService
    {
        private readonly PayPalClient _payPalClient ;
        private readonly IUnitOfWork _unitOfWork ;
        private readonly IMapper _mapper  ;
        private readonly IConfiguration _configuration  ;
        private readonly ILogger<PaymentService> _logger;
        private readonly IStatisticsNotificationService _notificationService;

        public PaymentService(PayPalClient payPalClient, IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration, ILogger<PaymentService> logger, IStatisticsNotificationService notificationService)
        {
            _payPalClient = payPalClient;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<Transaction> CreateTransaction(string userId, string paymentMethod, string description)
        {
            var transaction = new Transaction()
            {
                TransactionId = await _unitOfWork.TransactionRepository.GetNextTransactionId(),
                UserId = userId,
                PaymentMethod = paymentMethod,
                DateCreated = DateTime.Now,
                Status = TransactionStatus.Pending,   
                Description = description
            };

            await _unitOfWork.TransactionRepository.AddAsync(transaction);

            await _unitOfWork.SaveChanges();

            return transaction;
        }

        public async Task<string> CreatePaymentOrder(int orderId)
        {
            string returnUrl = _configuration["PayPalSettings:ReturnUrl"];
            string cancelUrl = _configuration["PayPalSettings:CancelUrl"];

            var order = await _unitOfWork.OrderRepository.GetAsync(o => o.OrderId == orderId, includeProperties: "Cart,Cart.CartItems,Transaction");
           



            if (order == null)
            {
                throw new KeyNotFoundException("The order does not exist.");
            }
            if (order.Transaction.Status != TransactionStatus.Pending)
            {
                throw new BadHttpRequestException("Transaction was used with orther order");
            }

            if (order.Cart.IsPurchased)
            {
                throw new BadHttpRequestException("The cart has already been purchased. You cannot proceed with the payment.");
            }


            if (order.Status != Cursus.Data.Entities.OrderStatus.PendingPayment)
            {
                throw new BadHttpRequestException("Order is not in a pending payment state.");
            }

            var amount = order.PaidAmount;
            if (amount <= 0)
            {

                throw new ArgumentException("The payment amount must be greater than 0.");
            }

            var request = new OrdersCreateRequest();
            request.Prefer("return=representation");
            request.RequestBody(new OrderRequest()
            {
                CheckoutPaymentIntent = "CAPTURE",
                PurchaseUnits = new List<PurchaseUnitRequest>
        {
            new PurchaseUnitRequest
            {
                AmountWithBreakdown = new AmountWithBreakdown
                {
                    CurrencyCode = "USD",
                    Value = amount.ToString("F2")
                }
            }
        },
                ApplicationContext = new ApplicationContext
                {
                    CancelUrl = cancelUrl,
                    ReturnUrl = returnUrl
                }
            });


            var response = await _payPalClient.Client().Execute(request);
            var result = response.Result<PayPalCheckoutSdk.Orders.Order>();


            if (result == null || result.Links == null || !result.Links.Any())
            {

                throw new InvalidOperationException("Invalid response from PayPal. No approval link found.");
            }

            var approvalUrl = result.Links.FirstOrDefault(link => link.Rel == "approve")?.Href;
            if (string.IsNullOrEmpty(approvalUrl))
            {

                throw new InvalidOperationException("Unable to generate PayPal payment link.");
            }

            var token = result.Id;


            var transaction = await _unitOfWork.TransactionRepository.GetAsync(t => t.TransactionId == order.TransactionId);

            transaction.Token = token;

            transaction.Amount = amount;

            await _unitOfWork.SaveChanges();

            string paymentUrl = $"{approvalUrl}";
            return paymentUrl;
        }


        public async Task<TransactionDTO> CapturePayment(string token, string payerId)
        {
            var transaction = await _unitOfWork.TransactionRepository.GetAsync(t => t.Token == token);

            

            if (transaction == null)
            {
                throw new BadHttpRequestException("Transaction not found.");
            }


            var request = new OrdersGetRequest(transaction.Token);
            var response = await _payPalClient.Client().Execute(request);
            var result = response.Result<PayPalCheckoutSdk.Orders.Order>();




            if (result.Status == "APPROVED" || result.Status == "COMPLETED")
            {

                await UpdateTransactionToCompleted(transaction);
                await _notificationService.NotifySalesAndRevenueUpdate();
                await _notificationService.NotifyOrderStatisticsUpdate();
                return _mapper.Map<TransactionDTO>(transaction);
            }
            else if (result.Status == "CREATED")
            {


                await _unitOfWork.TransactionRepository.UpdateTransactionStatus(transaction.TransactionId, TransactionStatus.Failed);
                var order = await GetOrderByTransactionId(transaction.TransactionId);
                if (order != null)
                {
                    await _unitOfWork.OrderRepository.UpdateOrderStatus(order.OrderId, OrderStatus.Failed);
                    await _unitOfWork.SaveChanges();
                }
                return _mapper.Map<TransactionDTO>(transaction);
            }
            else
            {
                throw new BadHttpRequestException($"Payment failed or incomplete on PayPal. Status: {result.Status}");
            }
        }


        private async Task UpdateTransactionToCompleted(Transaction transaction)
        {
           
            await _unitOfWork.TransactionRepository.UpdateTransactionStatus(transaction.TransactionId, TransactionStatus.Completed);

           
            var order = await GetOrderByTransactionId(transaction.TransactionId);
            if (order != null)
            {
               
                await _unitOfWork.OrderRepository.UpdateOrderStatus(order.OrderId, Cursus.Data.Entities.OrderStatus.Paid);

               
                await _unitOfWork.CartRepository.UpdateIsPurchased(order.CartId, true);
            }

            
            await _unitOfWork.SaveChanges();
        }

        private async Task<Cursus.Data.Entities.Order?> GetOrderByTransactionId(int transactionId)
        {
            
            return await _unitOfWork.OrderRepository.GetAsync(o => o.Transaction.TransactionId == transactionId);
        }





    }
}
