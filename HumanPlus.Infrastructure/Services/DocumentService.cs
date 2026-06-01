using HumanPlus.Application.Interfaces;
using HumanPlus.Domain.Entities.Candidates;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HumanPlus.Infrastructure.Services
{
    public class DocumentService : IDocumentService
    {
        static DocumentService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public Task<byte[]> GenerateCandidateRegistrationFormAsync(Candidate candidate)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().AlignCenter().Text("HumanPlus Manpower Consulting").Bold().FontSize(18);
                    page.Content().Column(col =>
                    {
                        col.Item().AlignCenter().Text("Candidate Registration Form").Bold().FontSize(14);
                        col.Item().PaddingVertical(3).LineHorizontal(1);

                        col.Item().Text($"Full Name: {candidate.User?.FullName ?? "N/A"}");
                        col.Item().Text($"Father's Name: {candidate.FatherName ?? "N/A"}");
                        col.Item().Text($"Mother's Name: {candidate.MotherName ?? "N/A"}");
                        col.Item().Text($"Gender: {candidate.Gender}");
                        col.Item().Text($"Date of Birth: {candidate.DateOfBirth:dd-MMM-yyyy}");
                        col.Item().Text($"Aadhaar: {candidate.AadhaarNumber ?? "N/A"}");
                        col.Item().Text($"Mobile: {candidate.User?.PhoneNumber ?? "N/A"}");
                        col.Item().Text($"Email: {candidate.User?.Email ?? "N/A"}");
                        col.Item().Text($"Address: {candidate.CurrentAddress ?? "N/A"}");
                        col.Item().PaddingVertical(3).LineHorizontal(1);
                        col.Item().Text($"Experience: {(candidate.IsFresher ? "Fresher" : $"{candidate.TotalExperienceYears} years")}");
                        col.Item().Text($"Previous Employer: {candidate.PreviousEmployer ?? "N/A"}");
                        col.Item().Text($"Expected Salary: {candidate.ExpectedSalary?.ToString("N0") ?? "N/A"}");
                        col.Item().Text($"Preferred Location: {candidate.PreferredLocation ?? "N/A"}");
                        col.Item().Text($"Status: {candidate.Status}");
                    });
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generated on: ");
                        x.Span($"{DateTime.Now:dd-MMM-yyyy HH:mm}");
                    });
                });
            });

            return Task.FromResult(document.GeneratePdf());
        }

        public Task<byte[]> GenerateReceiptAsync(string receiptNumber, string userName, decimal amount, string description)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(20);

                    page.Header().AlignCenter().Text("HumanPlus Manpower Consulting").Bold().FontSize(14);
                    page.Content().Column(col =>
                    {
                        col.Item().AlignCenter().Text("RECEIPT").Bold().FontSize(12);
                        col.Item().PaddingVertical(3).LineHorizontal(1);
                        col.Item().Text($"Receipt No: {receiptNumber}");
                        col.Item().Text($"Date: {DateTime.Now:dd-MMM-yyyy}");
                        col.Item().Text($"Received from: {userName}");
                        col.Item().Text($"Amount: Rs. {amount:N2}");
                        col.Item().Text($"Description: {description}");
                    });
                    page.Footer().AlignCenter().Text("Thank you for using HumanPlus!");
                });
            });

            return Task.FromResult(document.GeneratePdf());
        }

        public Task<byte[]> GenerateInvoiceAsync(string invoiceNumber, string companyName, decimal amount, string description)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(20);

                    page.Header().AlignCenter().Text("HumanPlus Manpower Consulting").Bold().FontSize(14);
                    page.Content().Column(col =>
                    {
                        col.Item().AlignCenter().Text("INVOICE").Bold().FontSize(12);
                        col.Item().PaddingVertical(3).LineHorizontal(1);
                        col.Item().Text($"Invoice No: {invoiceNumber}");
                        col.Item().Text($"Date: {DateTime.Now:dd-MMM-yyyy}");
                        col.Item().Text($"Bill To: {companyName}");
                        col.Item().Text($"Amount: Rs. {amount:N2}");
                        col.Item().Text($"Description: {description}");
                    });
                    page.Footer().AlignCenter().Text("Thank you for your business!");
                });
            });

            return Task.FromResult(document.GeneratePdf());
        }
    }
}
