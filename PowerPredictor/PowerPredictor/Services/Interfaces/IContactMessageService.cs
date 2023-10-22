using PowerPredictor.Models;

namespace PowerPredictor.Services.Interfaces
{
    public interface IContactMessageService
    {
        ContactMessage? GetMessageById(int id);
        bool AddMessage(ContactMessage message);
        bool DeleteMessage(int id);
        List<ContactMessage> GetAllMessages();
    }
}
