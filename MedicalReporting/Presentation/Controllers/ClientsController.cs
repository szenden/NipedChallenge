using Microsoft.AspNetCore.Mvc;

namespace MedicalAssessment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ClientResponse>>> GetAllClients()
        {
            var clients = await _clientService.GetAllClientsAsync();
            return Ok(clients);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ClientResponse>> GetClient(Guid id)
        {
            var client = await _clientService.GetClientByIdAsync(id);
            return client != null ? Ok(client) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<ClientResponse>> CreateClient(CreateClientRequest request)
        {
            try
            {
                var client = await _clientService.CreateClientAsync(request);
                return CreatedAtAction(nameof(GetClient), new { id = client.Id }, client);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id:guid}/assessments")]
        public async Task<ActionResult<HealthReportResponse>> CreateAssessment(Guid id, CreateAssessmentRequest request)
        {
            try
            {
                var report = await _clientService.CreateAssessmentAndGenerateReportAsync(id, request);
                return report != null ? Ok(report) : NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}