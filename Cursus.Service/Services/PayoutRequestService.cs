using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.Data.Enums;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Service.Services
{
    public class PayoutRequestService : IPayoutRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly CursusDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IAdminService _adminService;

        public PayoutRequestService(IUnitOfWork unitOfWork, CursusDbContext db, IMapper mapper, IEmailService emailService, IAdminService adminService)
        {
            _unitOfWork = unitOfWork;
            _db = db;
            _mapper = mapper;
            _emailService = emailService;
            _adminService = adminService;

        }

        public async Task<IEnumerable<PayoutRequestDisplayDTO>> GetApprovedPayoutRequest()
        {
            var approvedRequest = await (
                from pr in _db.PayoutRequests
                join tr in _db.Transactions on pr.TransactionId equals tr.TransactionId
                join usr in _db.Users on tr.UserId equals usr.Id
                where pr.PayoutRequestStatus == PayoutRequestStatus.Approved
                select new PayoutRequestDisplayDTO
                {
                    Id = pr.Id,
                    InstructorName = usr.UserName, // Lấy tên User
                    Amount = tr.Amount ?? 0,
                    CreateDate = pr.CreatedDate,
                    TransactionId = pr.TransactionId,
                    Status = pr.PayoutRequestStatus
                }
            ).ToListAsync();

            return approvedRequest;
        }

        public async Task<IEnumerable<PayoutRequestDisplayDTO>> GetPendingPayoutRequest()
        {
            var pendingRequest = await (
                from pr in _db.PayoutRequests
                join tr in _db.Transactions on pr.TransactionId equals tr.TransactionId
                join usr in _db.Users on tr.UserId equals usr.Id
                where pr.PayoutRequestStatus == PayoutRequestStatus.Pending
                select new PayoutRequestDisplayDTO
                {
                    Id = pr.Id,
                    InstructorName = usr.UserName, // Lấy tên User
                    Amount = tr.Amount ?? 0,
                    CreateDate = pr.CreatedDate,
                    TransactionId = pr.TransactionId,
                    Status = pr.PayoutRequestStatus
                }
            ).ToListAsync();

            return pendingRequest;
        }

        public async Task<IEnumerable<PayoutRequestDisplayDTO>> GetRejectPayoutRequest()
        {
            var rejectRequest = await (
                from pr in _db.PayoutRequests
                join tr in _db.Transactions on pr.TransactionId equals tr.TransactionId
                join usr in _db.Users on tr.UserId equals usr.Id
                where pr.PayoutRequestStatus == PayoutRequestStatus.Rejected
                select new PayoutRequestDisplayDTO
                {
                    Id = pr.Id,
                    InstructorName = usr.UserName, // Lấy tên User
                    Amount = tr.Amount ?? 0,
                    CreateDate = pr.CreatedDate,
                    TransactionId = pr.TransactionId,
                    Status = pr.PayoutRequestStatus
                }
            ).ToListAsync();

            return rejectRequest;
        }
        public async Task<PayoutAcceptDTO> AcceptPayout(int id)
        {
            PayoutRequest payoutRequest = await _unitOfWork.PayoutRequestRepository.GetPayoutByID(id);
            if (payoutRequest == null)
            {
                throw new KeyNotFoundException("Payout Request not found");
            }
            var instruct = await _unitOfWork.InstructorInfoRepository.GetAsync(x => x.Id == payoutRequest.InstructorId,includeProperties: "User");
            if (instruct == null)
            {
                throw new KeyNotFoundException("Instructor not found");
            }
            if(payoutRequest.PayoutRequestStatus != PayoutRequestStatus.Pending)
            {
                throw new BadHttpRequestException("Payout Request is not pending");
            }
            instruct.TotalWithdrawn += (double) payoutRequest.Transaction.Amount;
            payoutRequest.PayoutRequestStatus = PayoutRequestStatus.Approved;
            var payoutAccept= _mapper.Map<PayoutAcceptDTO>(payoutRequest);
            payoutAccept.Instructor= instruct;
            await _adminService.AcceptPayout(payoutAccept.TransactionId);
            await _unitOfWork.SaveChanges();
            string emailBody = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Payment Confirmation</title>
    <style>
        body {{
            font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background-color: #f9fafb;
            margin: 0;
            padding: 20px;
            color: #1f2937;
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 16px;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
            overflow: hidden;
        }}
        .email-header {{
            background-color: #7c3aed;
            padding: 32px 24px;
            text-align: center;
        }}
        .email-header h1 {{
            margin: 0;
            color: #ffffff;
            font-size: 24px;
            font-weight: 700;
        }}
        .amount-section {{
            background-color: #f5f3ff;
            padding: 24px;
            text-align: center;
            border-bottom: 1px solid #e5e7eb;
        }}
        .amount {{
            font-size: 36px;
            font-weight: 700;
            color: #7c3aed;
            margin: 8px 0;
        }}
        .email-body {{
            padding: 32px 24px;
            font-size: 16px;
            line-height: 1.6;
        }}
        .payment-details {{
            background-color: #f9fafb;
            border-radius: 12px;
            padding: 16px;
            margin: 24px 0;
        }}
        .payment-row {{
            display: flex;
            justify-content: space-between;
            padding: 12px 0;
            border-bottom: 1px solid #e5e7eb;
        }}
        .payment-row:last-child {{
            border-bottom: none;
        }}
        .label {{
            color: #6b7280;
            padding-right: 24px; /* Add space between label and value */
        }}
        .value {{
            font-weight: 500;
            text-align: right;
            margin-left: auto; /* Push value to the right */
            padding-left: 24px; /* Add space between label and value */
        }}
        .email-footer {{
            text-align: center;
            padding: 24px;
            background-color: #f9fafb;
            font-size: 14px;
            color: #6b7280;
        }}
        .email-footer a {{
            color: #7c3aed;
            text-decoration: none;
        }}
        .support-text {{
            margin-top: 24px;
            padding-top: 24px;
            border-top: 1px solid #e5e7eb;
            font-size: 14px;
            color: #6b7280;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""email-header"">
            <h1>Payment Confirmation</h1>
        </div>
        <div class=""amount-section"">
            <div style=""color: #6b7280;"">Amount Paid</div>
            <div class=""amount"">{payoutRequest.Transaction.Amount:C}</div>
            <div style=""color: #6b7280;"">Successfully transferred to your account</div>
        </div>
        <div class=""email-body"">
            <h2 style=""margin-top: 0;"">Hello {instruct.User.UserName},</h2>
            <p>We're pleased to confirm that your latest payout has been successfully processed and sent to your registered payment account.</p>
            
            <div class=""payment-details"">
                <div class=""payment-row"">
                    <span class=""label"">Transaction ID</span>
                    <span class=""value"">{payoutRequest.TransactionId}</span>
                </div>
                <div class=""payment-row"">
                    <span class=""label"">Payment Date</span>
                    <span class=""value"">{payoutRequest.Transaction.DateCreated}</span>
                </div>
                <div class=""payment-row"">
                    <span class=""label"">Payment Method</span>
                    <span class=""value"">{payoutRequest.Transaction.PaymentMethod}</span>
                </div>
            </div>

            <p>This payment represents your earnings from course sales, including all applicable fees and deductions. You can view detailed earnings reports in your instructor dashboard.</p>

            <div class=""support-text"">
                <p>If you notice any discrepancies or have questions about this payment, please hesitate and DO NOT to reach out to our support team.</p>
            </div>
        </div>
        <div class=""email-footer"">
            <p>Need help? Contact us at <a href=""mailto:cursus.course@gmail.com"">cursus.course@gmail.com</a> and we will no reply</p>
            <p>&copy; 2024 Cursus. All rights reserved. Your money, our choice</p>
        </div>
    </div>
</body>
</html>";
            var emailRequest = new EmailRequestDTO
            {
                toEmail = instruct.User.Email,
                Subject = "Payout Approved",
                Body = emailBody,
            };
            _emailService.SendEmail(emailRequest);
            return payoutAccept;
        }
        public async Task<PayoutDenyDTO> DenyPayout(int id,string reason)
        {
            PayoutRequest payoutRequest = await _unitOfWork.PayoutRequestRepository.GetPayoutByID(id);
            if (payoutRequest == null)
            {
                throw new KeyNotFoundException("Payout Request not found");
            }
            var instruct = await _unitOfWork.InstructorInfoRepository.GetAsync(x => x.Id == payoutRequest.InstructorId, includeProperties: "User");
            if (instruct == null)
            {
                throw new KeyNotFoundException("Instructor not found");
            }
            if (payoutRequest.PayoutRequestStatus != PayoutRequestStatus.Pending)
            {
                throw new BadHttpRequestException("Payout Request is not pending");
            }
            payoutRequest.PayoutRequestStatus = PayoutRequestStatus.Rejected;
            var payoutDeny = _mapper.Map<PayoutDenyDTO>(payoutRequest);
            payoutDeny.Reason = reason;
            await _unitOfWork.SaveChanges();
            string emailBody = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Important: Payment Rejection Notice</title>
    <style>
        body {{
            font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background-color: #f9fafb;
            margin: 0;
            padding: 20px;
            color: #1f2937;
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
            border: 1px solid #e5e7eb;
        }}
        .email-header {{
            background-color: #dc2626;
            padding: 24px;
            text-align: center;
            border-radius: 8px 8px 0 0;
        }}
        .email-header h1 {{
            margin: 0;
            color: #ffffff;
            font-size: 24px;
            font-weight: 700;
            text-transform: uppercase;
            letter-spacing: 0.025em;
        }}
        .email-body {{
            padding: 32px 24px;
            font-size: 16px;
            line-height: 1.6;
        }}
        .important-notice {{
            background-color: #fee2e2;
            border: 1px solid #dc2626;
            border-radius: 4px;
            padding: 16px;
            margin: 24px 0;
        }}
        .reason-box {{
            background-color: #f3f4f6;
            border-left: 4px solid #dc2626;
            padding: 16px;
            margin: 16px 0;
        }}
        .email-footer {{
            text-align: center;
            padding: 24px;
            background-color: #f9fafb;
            font-size: 14px;
            color: #4b5563;
            border-top: 1px solid #e5e7eb;
            border-radius: 0 0 8px 8px;
        }}
        .email-footer a {{
            color: #dc2626;
            text-decoration: none;
            font-weight: 500;
        }}
        .support-text {{
            margin-top: 24px;
            padding-top: 24px;
            border-top: 1px solid #e5e7eb;
            font-size: 14px;
            color: #4b5563;
        }}
        .action-required {{
            color: #dc2626;
            font-weight: 600;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""email-header"">
            <h1>Payment Rejection Notice</h1>
        </div>
        <div class=""email-body"">
            <h2 style=""margin-top: 0;"">Dear {instruct.User.UserName},</h2>
            
            <div class=""important-notice"">
                <strong>IMPORTANT:</strong> Your payment request has been rejected and requires immediate attention.
            </div>

            <p>We are writing to inform you that your recent payment request has been denied due to the following reason:</p>
            
            <div class=""reason-box"">
                <strong>Rejection Reason:</strong><br>
                {reason}
            </div>

            <p class=""action-required"">Action Required:</p>
            <p>Please review the rejection reason carefully and take the necessary corrective actions before submitting a new payment request. Failure to address these issues may result in your money become our money.</p>

            <div class=""support-text"">
                <p>If you believe this rejection was made in error or need assistance resolving this issue, we don't care.</p>
            </div>
        </div>
        <div class=""email-footer"">
            <a href=""mailto:support@cursus.com"">support@cursus.com</a></p>
            <p>Response Time: Who knows LOL</p>
            <p>&copy; 2024 Cursus. All rights reserved. Your money, our choice</p>
        </div>
    </div>
</body>
</html>";
            var emailRequest = new EmailRequestDTO
            {
                toEmail = instruct.User.Email,
                Subject = "Payout Denied",
                Body = emailBody,
            };
            _emailService.SendEmail(emailRequest);
            return payoutDeny;
        }
    }

}
