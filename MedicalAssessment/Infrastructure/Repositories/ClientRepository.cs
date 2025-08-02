using MedicalAssessment.Application.Interfaces;
using MedicalAssessment.Domain.Entities;
using MedicalAssessment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicalAssessment.Infrastructure.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly MedicalAssessmentDbContext _context;

        public ClientRepository(MedicalAssessmentDbContext context)
        {
            _context = context;
        }

        public async Task<Client?> GetByIdAsync(Guid id)
        {
            return await _context.Clients
                .Include(c => c.Assessments)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Client>> GetAllAsync()
        {
            return await _context.Clients
                .Include(c => c.Assessments)
                .ToListAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Client> AddAsync(Client client)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return client;
        }

        public async Task UpdateAsync(Client client)
        {
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
        }
    }
}