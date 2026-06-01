namespace HumanPlus.Application.Interfaces
{
    public interface ISmsService
    {
        Task SendSmsAsync(string mobileNumber, string message);
    }
}
