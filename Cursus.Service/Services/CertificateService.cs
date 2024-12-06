using AutoMapper;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class CertificateService : ICertificateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CertificateService> _logger;

    public CertificateService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IEmailService emailSender,
        IConfiguration configuration,
        ILogger<CertificateService> logger)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _emailSender = emailSender;
        _configuration = configuration;
        _logger = logger;
    }

    private string GetDownloadLink(string userId, int courseId)
    {
       
        var baseUrl = _configuration["BaseUrls:CertificateService"];
        var downloadCertificateEndpoint = _configuration["BaseUrls:DownloadCertificateEndpoint"];

        return $"{baseUrl}{downloadCertificateEndpoint}?userId={userId}&courseId={courseId}";
    }
    //Function Private
    public byte[] GenerateCertificatePDF(string name, string courseName, string date, string completionTitle)
    {
       
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.Background(QuestPDF.Helpers.Colors.White);

                page.Content().Padding(20).Column(column =>
                {
                    
                    column.Item().Border(5).BorderColor("#000066").Padding(10).Column(outerColumn =>
                    {
                       
                        outerColumn.Item().Border(2).BorderColor("#777777").Padding(10).Column(innerColumn =>
                        {
                           
                            innerColumn.Item().AlignCenter().Text("CERTIFICATE")
                                .FontSize(32).Bold().FontColor("#111111");

                            innerColumn.Item().PaddingTop(5).AlignCenter().Text("OF ACHIEVEMENT")
                                .FontSize(14).FontColor("#555555");  

                            innerColumn.Item().PaddingTop(13).AlignCenter().Text("THIS CERTIFICATE IS PROUDLY PRESENTED TO")
     .FontSize(10).FontColor("#FF9900").Bold();

                            innerColumn.Item().PaddingTop(10).AlignCenter().Text(name)
     .FontSize(32).FontFamily("Lobster").FontColor("#000066").Italic();




                            
                            innerColumn.Item().PaddingTop(5).AlignCenter().Text(courseName)
                                .FontSize(20).Italic().FontColor("#333333");

                           
                            innerColumn.Item().PaddingTop(10).AlignCenter().Text($"{date} for successfully completing the course requirements in:")
                                .FontSize(12).FontColor("#666666");

                            innerColumn.Item().AlignCenter().Text(completionTitle)
                                .FontSize(16).Bold().FontColor("#333333");
                         
                            innerColumn.Item().PaddingTop(30).Row(row =>
                            {
                               
                                row.RelativeItem().Column(signatureColumn =>
                                {
                                    signatureColumn.Item().Text("Cursus").AlignCenter().FontSize(10).FontColor("#666666");
                                    signatureColumn.Item().Text("_________________________").AlignCenter();
                                    signatureColumn.Item().Text("Signature").AlignCenter().FontSize(10).FontColor("#666666");
                                });

                                
                                row.RelativeItem();

                               
                                row.RelativeItem().Column(dateColumn =>
                                {
                                    dateColumn.Item().Text(DateTime.Now.ToString("dd/MM/yyyy")).AlignCenter().FontSize(10).FontColor("#666666");
                                    dateColumn.Item().Text("_________________________").AlignCenter();
                                    dateColumn.Item().Text("Date").AlignCenter().FontSize(10).FontColor("#666666");
                                });
                            });
                        });
                    });
                
                    
                    });
            });
        });

        using var memoryStream = new MemoryStream();
        document.GeneratePdf(memoryStream);
        return memoryStream.ToArray();
    }
    private async Task<byte[]> ExportExcel(IEnumerable<Certificate> list)
    {

        if (list == null )
        {
            throw new KeyNotFoundException("Data not found with user");
        }
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Certificates");


            worksheet.Cell(1, 1).Value = "User Name";
            worksheet.Cell(1, 2).Value = "Course Name";
            worksheet.Cell(1, 3).Value = "Issue Date";
            worksheet.Cell(1, 4).Value = "Download Link";

            int row = 2;


            foreach (var cert in list)
            {
                var user = await _unitOfWork.UserRepository.GetAsync(u => u.Id == cert.UserId);
                var course = await _unitOfWork.CourseRepository.GetAsync(c => c.Id == cert.CourseId);

                if (user != null && course != null)
                {
                    worksheet.Cell(row, 1).Value = user.UserName;
                    worksheet.Cell(row, 2).Value = course.Name;
                    worksheet.Cell(row, 3).Value = cert.CreateDate.ToString("dd/MM/yyyy");

                   
                   
                    worksheet.Cell(row, 4).SetHyperlink(new XLHyperlink(GetDownloadLink(user.Id,course.Id)));
                    worksheet.Cell(row, 4).Value = "Download PDF";

                    row++;
                }
            }


            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }

    private async   Task<Boolean> CheckExitCourse(string userId, int courseId)
    {        
        var certificate = await  _unitOfWork.CertificateRepository.GetAsync(p => p.CourseId == courseId && p.UserId == userId);
        if(certificate != null)
        {
            _logger.LogInformation("Exit");
        }
        _logger.LogInformation($"{certificate!=null}");
        return certificate != null;
    }
    //Function Private


    public async Task<byte[]> CreateCertificate(int courseId, string userId)
    {
        if (userId == null || courseId == null)
        {
            throw new ArgumentException("UserId or CourseId is not null");
        }

        Course course = await _unitOfWork.CourseRepository.GetAsync(p => p.Id == courseId);
        ApplicationUser user = await _unitOfWork.UserRepository.GetAsync(p => p.Id == userId);

        if (course == null || user == null)
        {
            throw new KeyNotFoundException("Course or user not found.");
        }
        if ( await CheckExitCourse(userId,courseId))
        {
            throw new BadHttpRequestException("Certificate already exists for this user and course.");
           
        }
        string date = DateTime.Now.ToString("dd/MM/yyyy");
        string   completionTitle = "Outstanding achievement in completing the course!";
        
        byte[] pdfData = GenerateCertificatePDF(user.UserName, course.Name, date, completionTitle);
        Task.Run(() => _emailSender.SendEmailCertificateCompletion(user, GetDownloadLink(userId, courseId)));

        var certificate = new Certificate()
        {
            CourseId = course.Id,
            UserId = user.Id,
            PdfData = pdfData,
            CreateDate = DateTime.Now
        };

       
        await _unitOfWork.CertificateRepository.AddAsync(certificate);
        await _unitOfWork.SaveChanges();
        return pdfData;
    }

    public async Task<byte[]> GetCertificatePdfByIdAsync(int certificateId)
    {
        if (certificateId == null )
        {
            throw new ArgumentException("certificateId is not null");
        }
        var certificate = await _unitOfWork.CertificateRepository.GetAsync(c => c.Id == certificateId);
        if (certificate == null || certificate.PdfData == null)
        {
            throw new KeyNotFoundException("Certificate not found or has no PDF data.");
        }
        return certificate.PdfData;
    }
    public async Task<byte[]> GetCertificatePdfByUserAndCourseAsync(int courseId, string userId)
    {

        var certificate = await _unitOfWork.CertificateRepository.GetAsync(c => c.UserId == userId && c.CourseId == courseId);
        if (certificate == null || certificate.PdfData == null)
        {
            throw new KeyNotFoundException("Certificate not found or has no PDF data.");
        }
        return certificate.PdfData;
    }

    public async Task<byte[]> ExportCertificatesToExcel()
    {       
        var certificates = await _unitOfWork.CertificateRepository.GetAllAsync();
        if(certificates.Count() == 0)
        {

            throw new KeyNotFoundException("Certificate not found ");
        }

        return await ExportExcel(certificates);
    }

    public async Task<byte[]> ExportCertificatesUserToExcel(string Id)
    {
        var user = await _unitOfWork.UserRepository.GetAsync(p => p.Id == Id);
        if (user == null) {
            throw new KeyNotFoundException("User not found");
        }

        var certificates = await _unitOfWork.CertificateRepository.GetAllAsync(p => p.UserId == Id);
       
        if (certificates.Count() ==  0)
        {

            throw new KeyNotFoundException("Certificate not found with userId ");
        }

        return await ExportExcel(certificates);
    }

}
