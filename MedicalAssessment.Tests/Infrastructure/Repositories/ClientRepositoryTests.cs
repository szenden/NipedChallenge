using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MedicalAssessment.Domain.Entities;
using MedicalAssessment.Domain.ValueObjects;
using MedicalAssessment.Infrastructure.Data;
using MedicalAssessment.Infrastructure.Repositories;

namespace MedicalAssessment.Tests.Infrastructure.Repositories;

public class ClientRepositoryTests : IDisposable
{
    private readonly MedicalAssessmentDbContext _context;
    private readonly ClientRepository _repository;

    public ClientRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<MedicalAssessmentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MedicalAssessmentDbContext(options);
        _repository = new ClientRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeSuccessfully_WithValidContext()
    {
        // Arrange & Act
        var repository = new ClientRepository(_context);

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public void Constructor_ShouldAcceptNullContext_WithoutThrowing()
    {
        // The current implementation doesn't validate constructor parameters
        var repository = new ClientRepository(null!);
        Assert.NotNull(repository);
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddClientToDatabase_WhenClientIsValid()
    {
        // Arrange
        var client = new Client("John Doe", new DateTime(1990, 5, 15), Gender.Male);

        // Act
        var result = await _repository.AddAsync(client);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(client.Id, result.Id);
        Assert.Equal("John Doe", result.Name);

        var dbClient = await _context.Clients.FindAsync(client.Id);
        Assert.NotNull(dbClient);
        Assert.Equal("John Doe", dbClient.Name);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnSameClient_AfterSaving()
    {
        // Arrange
        var client = new Client("Test Client", new DateTime(1985, 8, 22), Gender.Female);

        // Act
        var result = await _repository.AddAsync(client);

        // Assert
        Assert.Same(client, result);
        Assert.Equal(client.Id, result.Id);
        Assert.Equal(client.Name, result.Name);
        Assert.Equal(client.Gender, result.Gender);
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    [InlineData(Gender.Other)]
    public async Task AddAsync_ShouldHandleAllGenderTypes(Gender gender)
    {
        // Arrange
        var client = new Client("Test User", new DateTime(1990, 1, 1), gender);

        // Act
        var result = await _repository.AddAsync(client);

        // Assert
        var dbClient = await _context.Clients.FindAsync(client.Id);
        Assert.NotNull(dbClient);
        Assert.Equal(gender, dbClient.Gender);
    }

    [Fact]
    public async Task AddAsync_ShouldGenerateUniqueIds_ForMultipleClients()
    {
        // Arrange
        var client1 = new Client("Client 1", new DateTime(1990, 1, 1), Gender.Male);
        var client2 = new Client("Client 2", new DateTime(1990, 1, 1), Gender.Female);

        // Act
        await _repository.AddAsync(client1);
        await _repository.AddAsync(client2);

        // Assert
        Assert.NotEqual(client1.Id, client2.Id);
        
        var dbClients = await _context.Clients.ToListAsync();
        Assert.Equal(2, dbClients.Count);
        Assert.Contains(dbClients, c => c.Id == client1.Id);
        Assert.Contains(dbClients, c => c.Id == client2.Id);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ShouldReturnClient_WhenClientExists()
    {
        // Arrange
        var client = new Client("John Doe", new DateTime(1990, 5, 15), Gender.Male);
        await _repository.AddAsync(client);

        // Act
        var result = await _repository.GetByIdAsync(client.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(client.Id, result.Id);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal(new DateTime(1990, 5, 15), result.DateOfBirth);
        Assert.Equal(Gender.Male, result.Gender);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenClientDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldIncludeAssessments_WhenClientHasAssessments()
    {
        // Arrange
        var client = new Client("Test Client", new DateTime(1990, 1, 1), Gender.Male);
        var bloodPressure = new BloodPressure(120, 80);
        var exerciseMinutes = new ExerciseMinutes(150);
        var sleepQuality = new SleepQuality("7 hours, restful sleep");
        var stressLevel = new StressLevel("Low self-reported stress");
        var dietQuality = new DietQuality("Balanced, nutrient-rich diet");

        client.AddAssessment(bloodPressure, 180, 85, exerciseMinutes, sleepQuality, stressLevel, dietQuality);
        await _repository.AddAsync(client);

        // Act
        var result = await _repository.GetByIdAsync(client.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Assessments);
        
        var assessment = result.Assessments.First();
        Assert.Equal(120, assessment.BloodPressure.Systolic);
        Assert.Equal(80, assessment.BloodPressure.Diastolic);
        Assert.Equal(180, assessment.CholesterolTotal);
        Assert.Equal(85, assessment.BloodSugar);
        Assert.Equal(150, assessment.ExerciseMinutes.WeeklyMinutes);
        Assert.Equal("7 hours, restful sleep", assessment.SleepQuality.Description);
        Assert.Equal("Low self-reported stress", assessment.StressLevel.Assessment);
        Assert.Equal("Balanced, nutrient-rich diet", assessment.DietQuality.Assessment);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldHandleEmptyGuid()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.Empty);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoClientsExist()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllClients_WhenClientsExist()
    {
        // Arrange
        var client1 = new Client("John Doe", new DateTime(1990, 5, 15), Gender.Male);
        var client2 = new Client("Jane Smith", new DateTime(1985, 8, 22), Gender.Female);
        var client3 = new Client("Alex Johnson", new DateTime(1995, 12, 3), Gender.Other);

        await _repository.AddAsync(client1);
        await _repository.AddAsync(client2);
        await _repository.AddAsync(client3);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, c => c.Name == "John Doe");
        Assert.Contains(result, c => c.Name == "Jane Smith");
        Assert.Contains(result, c => c.Name == "Alex Johnson");
    }

    [Fact]
    public async Task GetAllAsync_ShouldIncludeAssessments_ForAllClients()
    {
        // Arrange
        var client1 = new Client("Client 1", new DateTime(1990, 1, 1), Gender.Male);
        var client2 = new Client("Client 2", new DateTime(1990, 1, 1), Gender.Female);
        
        await _repository.AddAsync(client1);
        await _repository.AddAsync(client2);

        // Add assessments with unique value objects to avoid EF tracking issues
        var bloodPressure1 = new BloodPressure(120, 80);
        var bloodPressure2 = new BloodPressure(125, 82);
        var bloodPressure3 = new BloodPressure(130, 85);
        var exerciseMinutes1 = new ExerciseMinutes(150);
        var exerciseMinutes2 = new ExerciseMinutes(160);
        var exerciseMinutes3 = new ExerciseMinutes(170);
        var sleepQuality1 = new SleepQuality("7 hours, restful sleep");
        var sleepQuality2 = new SleepQuality("7.5 hours, good sleep");
        var sleepQuality3 = new SleepQuality("8 hours, excellent sleep");
        var stressLevel1 = new StressLevel("Low self-reported stress");
        var stressLevel2 = new StressLevel("Moderate stress levels");
        var stressLevel3 = new StressLevel("Low stress with relaxation");
        var dietQuality1 = new DietQuality("Balanced, nutrient-rich diet");
        var dietQuality2 = new DietQuality("Healthy Mediterranean diet");
        var dietQuality3 = new DietQuality("Excellent nutritional plan");

        var assessment1 = new Assessment(client1.Id, bloodPressure1, 180, 85, exerciseMinutes1, sleepQuality1, stressLevel1, dietQuality1);
        var assessment2 = new Assessment(client2.Id, bloodPressure2, 190, 90, exerciseMinutes2, sleepQuality2, stressLevel2, dietQuality2);
        var assessment3 = new Assessment(client2.Id, bloodPressure3, 200, 95, exerciseMinutes3, sleepQuality3, stressLevel3, dietQuality3);

        await _repository.AddAssessmentAsync(assessment1);
        await _repository.AddAssessmentAsync(assessment2);
        await _repository.AddAssessmentAsync(assessment3);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
        
        var resultClient1 = result.First(c => c.Name == "Client 1");
        var resultClient2 = result.First(c => c.Name == "Client 2");
        
        Assert.Single(resultClient1.Assessments);
        Assert.Equal(2, resultClient2.Assessments.Count);
    }

    [Fact]
    public async Task GetAllAsync_ShouldHandleLargeNumberOfClients()
    {
        // Arrange
        var clients = new List<Client>();
        for (int i = 0; i < 100; i++)
        {
            var client = new Client($"Client {i}", new DateTime(1990, 1, 1).AddDays(i), 
                i % 3 == 0 ? Gender.Male : i % 3 == 1 ? Gender.Female : Gender.Other);
            clients.Add(client);
            await _repository.AddAsync(client);
        }

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(100, result.Count);
        for (int i = 0; i < 100; i++)
        {
            Assert.Contains(result, c => c.Name == $"Client {i}");
        }
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ShouldRemoveClient_WhenClientExists()
    {
        // Arrange
        var client = new Client("To Delete", new DateTime(1990, 1, 1), Gender.Male);
        await _repository.AddAsync(client);

        // Verify client exists
        var existingClient = await _repository.GetByIdAsync(client.Id);
        Assert.NotNull(existingClient);

        // Act
        await _repository.DeleteAsync(client.Id);

        // Assert
        var deletedClient = await _repository.GetByIdAsync(client.Id);
        Assert.Null(deletedClient);
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotThrow_WhenClientDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert - Should not throw
        await _repository.DeleteAsync(nonExistentId);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteClientAndAssessments_WhenClientHasAssessments()
    {
        // Arrange
        var client = new Client("Client with Assessments", new DateTime(1990, 1, 1), Gender.Male);
        var bloodPressure = new BloodPressure(120, 80);
        var exerciseMinutes = new ExerciseMinutes(150);
        var sleepQuality = new SleepQuality("7 hours, restful sleep");
        var stressLevel = new StressLevel("Low self-reported stress");
        var dietQuality = new DietQuality("Balanced, nutrient-rich diet");

        client.AddAssessment(bloodPressure, 180, 85, exerciseMinutes, sleepQuality, stressLevel, dietQuality);
        await _repository.AddAsync(client);

        var assessmentId = client.Assessments.First().Id;

        // Act
        await _repository.DeleteAsync(client.Id);

        // Assert
        var deletedClient = await _repository.GetByIdAsync(client.Id);
        Assert.Null(deletedClient);

        // Assessment should also be deleted due to cascade delete
        var deletedAssessment = await _context.Assessments.FindAsync(assessmentId);
        Assert.Null(deletedAssessment);
    }

    [Fact]
    public async Task DeleteAsync_ShouldOnlyDeleteSpecifiedClient_WhenMultipleClientsExist()
    {
        // Arrange
        var client1 = new Client("Client 1", new DateTime(1990, 1, 1), Gender.Male);
        var client2 = new Client("Client 2", new DateTime(1990, 1, 1), Gender.Female);
        var client3 = new Client("Client 3", new DateTime(1990, 1, 1), Gender.Other);

        await _repository.AddAsync(client1);
        await _repository.AddAsync(client2);
        await _repository.AddAsync(client3);

        // Act
        await _repository.DeleteAsync(client2.Id);

        // Assert
        var remainingClients = await _repository.GetAllAsync();
        Assert.Equal(2, remainingClients.Count);
        Assert.Contains(remainingClients, c => c.Id == client1.Id);
        Assert.Contains(remainingClients, c => c.Id == client3.Id);
        Assert.DoesNotContain(remainingClients, c => c.Id == client2.Id);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ShouldUpdateClientProperties_WhenClientExists()
    {
        // Arrange
        var client = new Client("Original Name", new DateTime(1990, 1, 1), Gender.Male);
        await _repository.AddAsync(client);

        // Create an updated version (note: in practice, you'd modify the existing entity)
        var updatedClient = new Client("Updated Name", new DateTime(1985, 5, 15), Gender.Female);
        // Set the same ID to simulate an update
        var idField = typeof(Client).GetProperty("Id");
        idField?.SetValue(updatedClient, client.Id);

        // Act
        await _repository.UpdateAsync(updatedClient);

        // Assert
        var result = await _repository.GetByIdAsync(client.Id);
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal(new DateTime(1985, 5, 15), result.DateOfBirth);
        Assert.Equal(Gender.Female, result.Gender);
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotThrow_WhenClientDoesNotExist()
    {
        // Arrange
        var nonExistentClient = new Client("Non Existent", new DateTime(1990, 1, 1), Gender.Male);

        // Act & Assert - Should not throw
        await _repository.UpdateAsync(nonExistentClient);
    }

    [Fact]
    public async Task UpdateAsync_ShouldHandleClientUpdate_WhenClientExists()
    {
        // Arrange
        var client = new Client("Test Client", new DateTime(1990, 1, 1), Gender.Male);
        await _repository.AddAsync(client);

        // Create updated client with same ID
        var updatedClient = new Client("Updated Client", new DateTime(1985, 5, 15), Gender.Female);
        var idField = typeof(Client).GetProperty("Id");
        idField?.SetValue(updatedClient, client.Id);

        // Act
        await _repository.UpdateAsync(updatedClient);

        // Assert
        var result = await _repository.GetByIdAsync(client.Id);
        Assert.NotNull(result);
        Assert.Equal("Updated Client", result.Name);
        Assert.Equal(new DateTime(1985, 5, 15), result.DateOfBirth);
        Assert.Equal(Gender.Female, result.Gender);
    }

    #endregion

    #region AddAssessmentAsync Tests

    [Fact]
    public async Task AddAssessmentAsync_ShouldAddAssessmentToDatabase()
    {
        // Arrange
        var client = new Client("Test Client", new DateTime(1990, 1, 1), Gender.Male);
        await _repository.AddAsync(client);

        var bloodPressure = new BloodPressure(130, 85);
        var exerciseMinutes = new ExerciseMinutes(90);
        var sleepQuality = new SleepQuality("6 hours, frequent disturbances");
        var stressLevel = new StressLevel("Moderate self-reported stress");
        var dietQuality = new DietQuality("Processed or high-sugar diet");

        var assessment = new Assessment(client.Id, bloodPressure, 210, 95,
            exerciseMinutes, sleepQuality, stressLevel, dietQuality);

        // Act
        await _repository.AddAssessmentAsync(assessment);

        // Assert
        var dbAssessment = await _context.Assessments.FindAsync(assessment.Id);
        Assert.NotNull(dbAssessment);
        Assert.Equal(client.Id, dbAssessment.ClientId);
        Assert.Equal(130, dbAssessment.BloodPressure.Systolic);
        Assert.Equal(85, dbAssessment.BloodPressure.Diastolic);
        Assert.Equal(210, dbAssessment.CholesterolTotal);
        Assert.Equal(95, dbAssessment.BloodSugar);
        Assert.Equal(90, dbAssessment.ExerciseMinutes.WeeklyMinutes);
        Assert.Equal("6 hours, frequent disturbances", dbAssessment.SleepQuality.Description);
        Assert.Equal("Moderate self-reported stress", dbAssessment.StressLevel.Assessment);
        Assert.Equal("Processed or high-sugar diet", dbAssessment.DietQuality.Assessment);
    }

    [Fact]
    public async Task AddAssessmentAsync_ShouldHandleMultipleAssessments_ForSameClient()
    {
        // Arrange
        var client = new Client("Test Client", new DateTime(1990, 1, 1), Gender.Male);
        await _repository.AddAsync(client);

        var bloodPressure1 = new BloodPressure(120, 80);
        var bloodPressure2 = new BloodPressure(140, 90);
        var exerciseMinutes1 = new ExerciseMinutes(150);
        var exerciseMinutes2 = new ExerciseMinutes(180);
        var sleepQuality1 = new SleepQuality("7 hours, restful sleep");
        var sleepQuality2 = new SleepQuality("8 hours, excellent sleep");
        var stressLevel1 = new StressLevel("Low self-reported stress");
        var stressLevel2 = new StressLevel("Very low stress levels");
        var dietQuality1 = new DietQuality("Balanced, nutrient-rich diet");
        var dietQuality2 = new DietQuality("Excellent nutrition plan");

        var assessment1 = new Assessment(client.Id, bloodPressure1, 180, 85,
            exerciseMinutes1, sleepQuality1, stressLevel1, dietQuality1);
        var assessment2 = new Assessment(client.Id, bloodPressure2, 200, 95,
            exerciseMinutes2, sleepQuality2, stressLevel2, dietQuality2);

        // Act
        await _repository.AddAssessmentAsync(assessment1);
        await _repository.AddAssessmentAsync(assessment2);

        // Assert
        var assessments = await _context.Assessments
            .Where(a => a.ClientId == client.Id)
            .ToListAsync();

        Assert.Equal(2, assessments.Count);
        Assert.Contains(assessments, a => a.BloodPressure.Systolic == 120);
        Assert.Contains(assessments, a => a.BloodPressure.Systolic == 140);
    }

    [Fact]
    public async Task AddAssessmentAsync_ShouldHandleAllNewHealthFactors()
    {
        // Arrange
        var client = new Client("Health Factors Test", new DateTime(1990, 1, 1), Gender.Male);
        await _repository.AddAsync(client);

        var bloodPressure = new BloodPressure(140, 90);
        var exerciseMinutes = new ExerciseMinutes(50);
        var sleepQuality = new SleepQuality("4 hours, severe sleep issues");
        var stressLevel = new StressLevel("High chronic stress affecting well-being");
        var dietQuality = new DietQuality("Poor nutrition with deficiencies");

        var assessment = new Assessment(client.Id, bloodPressure, 240, 126,
            exerciseMinutes, sleepQuality, stressLevel, dietQuality);

        // Act
        await _repository.AddAssessmentAsync(assessment);

        // Assert
        var dbAssessment = await _context.Assessments.FindAsync(assessment.Id);
        Assert.NotNull(dbAssessment);
        
        // Verify all new health factors are stored correctly
        Assert.Equal(50, dbAssessment.ExerciseMinutes.WeeklyMinutes);
        Assert.Equal("4 hours, severe sleep issues", dbAssessment.SleepQuality.Description);
        Assert.Equal("High chronic stress affecting well-being", dbAssessment.StressLevel.Assessment);
        Assert.Equal("Poor nutrition with deficiencies", dbAssessment.DietQuality.Assessment);
        
        // Verify existing factors still work
        Assert.Equal(140, dbAssessment.BloodPressure.Systolic);
        Assert.Equal(90, dbAssessment.BloodPressure.Diastolic);
        Assert.Equal(240, dbAssessment.CholesterolTotal);
        Assert.Equal(126, dbAssessment.BloodSugar);
    }

    #endregion

    #region SaveChangesAsync Tests

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChanges_WhenCalled()
    {
        // Arrange
        var client = new Client("Save Changes Test", new DateTime(1990, 1, 1), Gender.Male);
        _context.Clients.Add(client);

        // Act
        await _repository.SaveChangesAsync();

        // Assert
        var savedClient = await _context.Clients.FindAsync(client.Id);
        Assert.NotNull(savedClient);
        Assert.Equal("Save Changes Test", savedClient.Name);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldHandleNoChanges_WithoutError()
    {
        // Act & Assert - Should not throw
        await _repository.SaveChangesAsync();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task Repository_ShouldHandleCompleteWorkflow_AddGetUpdateDelete()
    {
        // Arrange
        var client = new Client("Workflow Test", new DateTime(1990, 1, 1), Gender.Male);

        // Act & Assert - Add
        var addedClient = await _repository.AddAsync(client);
        Assert.NotNull(addedClient);
        Assert.Equal("Workflow Test", addedClient.Name);

        // Act & Assert - Get
        var retrievedClient = await _repository.GetByIdAsync(client.Id);
        Assert.NotNull(retrievedClient);
        Assert.Equal("Workflow Test", retrievedClient.Name);

        // Act & Assert - Update
        var updatedClient = new Client("Updated Workflow Test", new DateTime(1985, 1, 1), Gender.Female);
        var idField = typeof(Client).GetProperty("Id");
        idField?.SetValue(updatedClient, client.Id);
        
        await _repository.UpdateAsync(updatedClient);
        var updatedRetrievedClient = await _repository.GetByIdAsync(client.Id);
        Assert.NotNull(updatedRetrievedClient);
        Assert.Equal("Updated Workflow Test", updatedRetrievedClient.Name);

        // Act & Assert - Delete
        await _repository.DeleteAsync(client.Id);
        var deletedClient = await _repository.GetByIdAsync(client.Id);
        Assert.Null(deletedClient);
    }

    [Fact]
    public async Task Repository_ShouldHandleAssessmentWorkflow_WithAllHealthFactors()
    {
        // Arrange
        var client = new Client("Assessment Workflow", new DateTime(1990, 1, 1), Gender.Male);
        await _repository.AddAsync(client);

        // Create assessment with all health factors
        var bloodPressure = new BloodPressure(125, 85);
        var exerciseMinutes = new ExerciseMinutes(120);
        var sleepQuality = new SleepQuality("6.5 hours, mild disturbances");
        var stressLevel = new StressLevel("Moderate self-reported stress");
        var dietQuality = new DietQuality("Processed or high-sugar diet");

        var assessment = new Assessment(client.Id, bloodPressure, 220, 105,
            exerciseMinutes, sleepQuality, stressLevel, dietQuality);

        // Act
        await _repository.AddAssessmentAsync(assessment);

        // Assert - Verify client with assessment
        var clientWithAssessment = await _repository.GetByIdAsync(client.Id);
        Assert.NotNull(clientWithAssessment);
        
        // The assessment added directly won't show up in client.Assessments 
        // because it wasn't added through the client entity
        // But we can verify it exists in the database
        var dbAssessment = await _context.Assessments
            .Where(a => a.ClientId == client.Id)
            .FirstOrDefaultAsync();
        
        Assert.NotNull(dbAssessment);
        Assert.Equal(125, dbAssessment.BloodPressure.Systolic);
        Assert.Equal(120, dbAssessment.ExerciseMinutes.WeeklyMinutes);
        Assert.Equal("6.5 hours, mild disturbances", dbAssessment.SleepQuality.Description);
        Assert.Equal("Moderate self-reported stress", dbAssessment.StressLevel.Assessment);
        Assert.Equal("Processed or high-sugar diet", dbAssessment.DietQuality.Assessment);
    }

    #endregion
}