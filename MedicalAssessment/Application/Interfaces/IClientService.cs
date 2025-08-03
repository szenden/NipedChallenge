using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedicalAssessment.Application.DTOs;

namespace MedicalAssessment.Application.Interfaces
{
    public interface IClientService
    {
        Task<ClientResponse> CreateClientAsync(CreateClientRequest request);
        Task<ClientResponse?> GetClientByIdAsync(Guid id);
        Task<List<ClientResponse>> GetAllClientsAsync();
        Task<HealthReportResponse?> CreateAssessmentAndGenerateReportAsync(Guid clientId, CreateAssessmentRequest request);
    }
}