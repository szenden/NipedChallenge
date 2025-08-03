using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedicalAssessment.Application.DTOs;
using MedicalAssessment.Application.Interfaces;
using MedicalAssessment.Domain.Entities;
using MedicalAssessment.Domain.Services;
using MedicalAssessment.Domain.ValueObjects;

namespace MedicalAssessment.Application.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IHealthAssessmentService _healthAssessmentService;

        public ClientService(IClientRepository clientRepository, IHealthAssessmentService healthAssessmentService)
        {
            _clientRepository = clientRepository;
            _healthAssessmentService = healthAssessmentService;
        }

        public async Task<ClientResponse> CreateClientAsync(CreateClientRequest request)
        {
            var client = new Client(request.Name, request.DateOfBirth, request.Gender);
            await _clientRepository.AddAsync(client);
            
            return MapToResponse(client);
        }

        public async Task<List<ClientResponse>> GetAllClientsAsync()
        {
            var clients = await _clientRepository.GetAllAsync();
            return clients.Select(MapToResponse).ToList();
        }

        public async Task<ClientResponse?> GetClientByIdAsync(Guid id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            return client != null ? MapToResponse(client) : null;
        }

        public async Task<HealthReportResponse?> CreateAssessmentAndGenerateReportAsync(Guid clientId, CreateAssessmentRequest request)
        {
            var client = await _clientRepository.GetByIdAsync(clientId);
            if (client == null) return null;

            var bloodPressure = new BloodPressure(request.SystolicBP, request.DiastolicBP);
            var exerciseMinutes = new ExerciseMinutes(request.ExerciseWeeklyMinutes);
            var sleepQuality = new SleepQuality(request.SleepQuality);
            var stressLevel = new StressLevel(request.StressLevel);
            var dietQuality = new DietQuality(request.DietQuality);
            
            var assessment = new Assessment(clientId, bloodPressure, request.CholesterolTotal, request.BloodSugar,
                exerciseMinutes, sleepQuality, stressLevel, dietQuality);
            
            // Add assessment directly to the context instead of through the client
            await _clientRepository.AddAssessmentAsync(assessment);

            var report = _healthAssessmentService.GenerateReport(client, assessment);
            return MapToResponse(report);
        }

        private static ClientResponse MapToResponse(Client client)
        {
            return new ClientResponse(
                client.Id,
                client.Name,
                client.DateOfBirth,
                client.Gender,
                client.CalculateAge(),
                client.Assessments.Count
            );
        }

        private static HealthReportResponse MapToResponse(HealthReport report)
        {
            var metrics = report.Metrics.Select(m => new HealthMetricResponse(
                m.Name,
                m.Value,
                m.Status.ToString(),
                m.Recommendation
            )).ToList();

            return new HealthReportResponse(
                report.ClientId,
                report.ClientName,
                report.AssessmentDate,
                metrics,
                report.OverallRisk,
                report.Recommendations
            );
        }
    }
}