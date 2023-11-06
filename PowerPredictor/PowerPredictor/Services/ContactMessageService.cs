using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using PowerPredictor.Models;
using PowerPredictor.Services.Interfaces;

namespace PowerPredictor.Services
{
    public class ContactMessageService : IContactMessageService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        public ContactMessageService(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _contextFactory = dbContextFactory;
        }
        public async Task<bool> AddMessageAsync(ContactMessage message)
        {
            using(var context = _contextFactory.CreateDbContext())
            {
                message.Date = DateTime.Now;
                context.ContactMessages.Add(message);
                if (await context.SaveChangesAsync() == 1) return true;
                return false;
            }
        }

        public async Task<bool> DeleteMessageAsync(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var message = await context.ContactMessages.FindAsync(id);
                if (message == null) return false;
                context.ContactMessages.Remove(message);

                if (await context.SaveChangesAsync() == 1) return true;
                return false;
            }
        }

        public async Task<List<ContactMessage>> GetAllMessagesAsync()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return await context.ContactMessages.ToListAsync();
            }
        }
    }
}
