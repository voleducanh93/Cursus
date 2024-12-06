using Cursus.Common.Helper;
using Cursus.Data.Entities;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class CertificateController : ControllerBase
{
    private readonly ICertificateService _certificateService;

    public CertificateController(ICertificateService certificateService)
    {
        _certificateService = certificateService;
    }

    [HttpPost("create-certificate")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "User,Admin")]
    public async Task<IActionResult> CreateCertificate(string userId, int courseId)
    {
        var apiResponse = new APIResponse();
         await _certificateService.CreateCertificate(courseId, userId);

        apiResponse.StatusCode = HttpStatusCode.OK;
        apiResponse.IsSuccess = true;
        apiResponse.Result = "Create certificate successull,Check your email";
        return StatusCode((int)apiResponse.StatusCode, apiResponse);
    }

    [HttpGet("download-certificate")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadCertificate(string userId, int courseId)
    {
        var apiResponse = new APIResponse();
        var pdfData = await _certificateService.GetCertificatePdfByUserAndCourseAsync(courseId, userId);
      
        if (pdfData == null)
        {
            apiResponse.StatusCode = HttpStatusCode.NotFound;
            apiResponse.IsSuccess = false;
            apiResponse.ErrorMessages.Add("Certificate not found for the specified user and course.");
            return StatusCode((int)apiResponse.StatusCode, apiResponse);
        }
        apiResponse.StatusCode = HttpStatusCode.OK;
        apiResponse.IsSuccess = true;
        apiResponse.Result = "Download your certificate.";
        Response.Headers.Add("ApiResponse", JsonSerializer.Serialize(apiResponse));
        return File(pdfData, "application/pdf", "Certificate.pdf");
    }


    [HttpGet("download-certificate/{certificateId}")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "User,Admin")]
    public async Task<IActionResult> DownloadCertificateById(int certificateId)
    {
        var apiResponse = new APIResponse();

        var pdfData = await _certificateService.GetCertificatePdfByIdAsync(certificateId);
        apiResponse.StatusCode = HttpStatusCode.OK;
        apiResponse.IsSuccess = true;
        apiResponse.Result = " your certificate successful";
        Response.Headers.Add("ApiResponse", JsonSerializer.Serialize(apiResponse));
        return File(pdfData, "application/pdf", "Certificate.pdf");
    }

    [HttpGet("export-excel")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "User,Admin")]
    public async Task<IActionResult> ExportCertificatesToExcel()
    {
        var excelData = await _certificateService.ExportCertificatesToExcel();

        if (excelData == null || excelData.Length == 0)
        {
            var apiResponse = new APIResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                IsSuccess = false,
                ErrorMessages = new List<string> { "No certificate data available." }
            };
            return StatusCode((int)apiResponse.StatusCode, apiResponse);
        }

       
        return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Certificates.xlsx");
    }


    [HttpGet("export-excel-by-user/{userId}")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "User,Admin")]
    public async Task<IActionResult> ExportCertificatesToExcelByUserId(string userId)
    {
        var excelData = await _certificateService.ExportCertificatesUserToExcel(userId);

        if (excelData == null || excelData.Length == 0)
        {
            var apiResponse = new APIResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                IsSuccess = false,
                ErrorMessages = new List<string> { "No certificate data available for this user." }
            };
            return StatusCode((int)apiResponse.StatusCode, apiResponse);
        }

      
        return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Certificates_{userId}.xlsx");
    }

}
