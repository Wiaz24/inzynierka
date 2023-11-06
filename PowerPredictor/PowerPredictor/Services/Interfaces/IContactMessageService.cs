using PowerPredictor.Models;

namespace PowerPredictor.Services.Interfaces
{
    public interface IContactMessageService
    {
        Task<bool> AddMessageAsync(ContactMessage message);
        Task<bool> DeleteMessageAsync(int id);
        Task<List<ContactMessage>> GetAllMessagesAsync();
    }
}
