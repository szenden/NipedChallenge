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

## Security Features

- **JWT Authentication**: Bearer token-based authentication system
- **Input Validation**: Comprehensive validation with detailed error messages
- **Authorization**: Role-based access control with Admin and User roles
- **Secure Endpoints**: All endpoints protected with [Authorize] attribute

## API Endpoints

### JWT Authentication
- `POST /api/auth/login` - Authenticate user and receive JWT token

### Clients (Protected)
- `POST /api/clients` - Create a new client
- `GET /api/clients` - Get all clients
- `GET /api/clients/{id}` - Get client by ID

### Health Assessments (Protected)
- `POST /api/clients/{id}/assessments` - Create assessment and generate health report

## Test User Credentials
Available test users:
- `admin` / `password` (Admin role)
- `doctor` / `doctor123` (User role)
- `user` / `user123` (User role)
## Using Swagger UI
1. Go to `https://localhost:5003/swagger`
2. Get a token first
   - Click on `/api/Auth/login` endpoint
   - Click "Try it out"
   - Enter credentials:
     ```json
     {
       "username": "admin",
       "password": "password"
     }
     ```
   - Click "Execute"
   - Copy the token from the response (without quotes)

3. Authorize in Swagger:
   - Click the **"Authorize"** button (🔒 lock icon) at the top-right of Swagger UI
   - In the "Bearer" section, enter: `Bearer YOUR_TOKEN_HERE`
   - Click "Authorize"
   - Click "Close"

## Technology Stack

- **.NET 9**: Latest .NET framework
- **ASP.NET Core**: Web API framework with JWT authentication
- **Entity Framework Core**: Data access with In-Memory database
- **System.IdentityModel.Tokens.Jwt**: JWT token generation and validation
- **Swagger**: API documentation with authentication support
- **Clean Architecture**: Domain-driven design with separated layers
- **xUnit**: Unit and integration testing framework
- **Moq**: Mocking framework for isolated testing

## Project Structure

```
MedicalAssessment/
├── Domain/
│   ├── Entities/          # Core business entities
│   ├── ValueObjects/      # Health metrics value objects
│   └── Services/          # Domain services
├── Application/
│   ├── DTOs/             # Data transfer objects with validation
│   ├── Interfaces/       # Service contracts
│   └── Services/         # Application services & JWT service
├── Infrastructure/
│   ├── Data/             # Entity Framework configuration
│   ├── Filters/          # Input validation filters
│   └── Repositories/     # Data access implementations
├── Presentation/
│   └── Controllers/      # API controllers with authentication
└── Tests/
    ├── Domain/           # Domain layer tests
    ├── Application/      # Application service tests
    ├── Infrastructure/   # Infrastructure layer tests
    ├── Presentation/     # Controller tests
    └── Integration/      # Authentication integration tests
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

Test coverage includes:
- **Domain Tests**: Value objects, entities, and domain services
- **Application Tests**: Business logic and service orchestration
- **Infrastructure Tests**: Data access and validation filters
- **Controller Tests**: API endpoints with authentication
- **Integration Tests**: End-to-end authentication pipeline testing


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