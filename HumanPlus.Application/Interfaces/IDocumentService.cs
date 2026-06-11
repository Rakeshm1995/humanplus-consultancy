using HumanPlus.Domain.Entities.Candidates;

namespace HumanPlus.Application.Interfaces
{
    public interface IDocumentService
    {
        Task<byte[]> GenerateCandidateRegistrationFormAsync(Candidate candidate, string baseUrl, string webRootPath);
        Task<byte[]> GenerateReceiptAsync(string receiptNumber, string userName, decimal amount, string description);
        Task<byte[]> GenerateInvoiceAsync(string invoiceNumber, string companyName, decimal amount, string description);
    }
}
