using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedicalAssessment.Domain.Entities;

namespace MedicalAssessment.Application.Interfaces
{
    public interface IClientRepository
    {
        Task<Client?> GetByIdAsync(Guid id);
        Task<List<Client>> GetAllAsync();
        Task<Client> AddAsync(Client client);
        Task UpdateAsync(Client client);
    }
}