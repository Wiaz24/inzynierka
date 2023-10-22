using PowerPredictor.Models;
using PowerPredictor.Services.Interfaces;

namespace PowerPredictor.Services
{
    public class ContactMessageService : IContactMessageService
    {
        private readonly AppDbContext _context;
        public ContactMessageService(AppDbContext dbContext)
        {
            _context = dbContext;
        }
        public bool AddMessage(ContactMessage message)
        {
            message.Date = DateTime.Now;
            _context.ContactMessages.Add(message);
            if (_context.SaveChanges() == 1) return true;
            return false;
        }

        public bool DeleteMessage(int id)
        {
            var message = GetMessageById(id);
            if (message == null) return false;
            _context.ContactMessages.Remove(message);
            if (_context.SaveChanges() == 1) return true;
            return false;
        }

        public List<ContactMessage> GetAllMessages()
        {
            return _context.ContactMessages.ToList();
        }

        public ContactMessage? GetMessageById(int id)
        {
            var result = _context.ContactMessages.Find(id);
            return result;
        }
    }
}
