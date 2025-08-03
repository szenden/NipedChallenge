# Medical Assessment API

A comprehensive health assessment system that evaluates client health metrics including blood work, exercise habits, sleep quality, stress levels, and diet quality.

## Features

- **Client Management**: Create and manage client profiles
- **Health Assessments**: Comprehensive health evaluations including:
  - Blood pressure monitoring
  - Cholesterol tracking
  - Blood sugar analysis
  - Exercise minutes per week
  - Sleep quality assessment
  - Stress level evaluation
  - Diet quality assessment
- **Health Reports**: Automated report generation with personalized recommendations
- **Risk Assessment**: Overall health risk calculation based on multiple factors

## Health Metrics

### Blood Work
- **Blood Pressure**: Systolic/Diastolic measurements with optimal (<120/<80), needs attention (120-129/<80), and serious issue (≥130/≥80) thresholds
- **Cholesterol**: Total cholesterol tracking with guidelines (<200 optimal, 200-239 needs attention, ≥240 serious)
- **Blood Sugar**: Glucose monitoring (70-99 optimal, 100-125 needs attention, ≥126 serious)

### Lifestyle Factors
- **Exercise**: Weekly minutes tracking (≥150 optimal, 75-149 needs attention, <75 serious)
- **Sleep Quality**: Descriptive assessment of sleep patterns and quality
- **Stress Level**: Self-reported stress evaluation from low to high chronic stress
- **Diet Quality**: Nutritional assessment from balanced nutrient-rich to poor nutrition

## API Endpoints

### Clients
- `POST /api/clients` - Create a new client
- `GET /api/clients` - Get all clients
- `GET /api/clients/{id}` - Get client by ID

### Health Assessments
- `POST /api/clients/{id}/assessments` - Create assessment and generate health report

## Technology Stack

- **.NET 9**: Latest .NET framework
- **ASP.NET Core**: Web API framework
- **Entity Framework Core**: Data access with In-Memory database
- **Swagger**: API documentation
- **Clean Architecture**: Domain-driven design with separated layers

## Project Structure

```
MedicalAssessment/
├── Domain/
│   ├── Entities/          # Core business entities
│   ├── ValueObjects/      # Health metrics value objects
│   └── Services/          # Domain services
├── Application/
│   ├── DTOs/             # Data transfer objects
│   ├── Interfaces/       # Service contracts
│   └── Services/         # Application services
├── Infrastructure/
│   ├── Data/             # Entity Framework configuration
│   └── Repositories/     # Data access implementations
└── Presentation/
    └── Controllers/      # API controllers
```

## Getting Started

### Prerequisites
- .NET 9 SDK
- Visual Studio 2022 or VS Code

### Running the Application

1. Clone the repository
2. Navigate to the MedicalAssessment directory
3. Build the project:
   ```bash
   dotnet build
   ```
4. Run the application:
   ```bash
   dotnet run
   ```
5. Access the API at `https://localhost:5003` or `http://localhost:5002`
6. View Swagger documentation at `https://localhost:5003/swagger`

### Running Tests

```bash
dotnet test
```

## Sample Assessment Request

```json
{
  "systolicBP": 130,
  "diastolicBP": 85,
  "cholesterolTotal": 210,
  "bloodSugar": 95,
  "exerciseWeeklyMinutes": 90,
  "sleepQuality": "6 hours, frequent disturbances",
  "stressLevel": "Moderate self-reported stress",
  "dietQuality": "Processed or high-sugar diet"
}
```

## Health Status Categories

- **Optimal**: Healthy ranges requiring maintenance
- **Needs Attention**: Borderline values requiring monitoring and lifestyle changes
- **Serious Issue**: Values requiring immediate medical consultation

## Development

The application uses clean architecture principles with:
- Domain layer containing business logic and entities
- Application layer handling use cases and orchestration
- Infrastructure layer managing data persistence
- Presentation layer exposing REST API endpoints

Each health metric is implemented as a value object with built-in validation and status assessment methods.