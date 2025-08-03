using Microsoft.AspNetCore.Mvc;
using MedicalAssessment.Application.DTOs;
using MedicalAssessment.Application.Services;

namespace MedicalAssessment.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        
        public AuthController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }
        
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Simple hardcoded authentication for assessment purposes
            // In production, replace with real user authentication
            if (IsValidUser(request.Username, request.Password))
            {
                var role = request.Username == "admin" ? "Admin" : "User";
                var token = _jwtService.GenerateToken(request.Username, role);
                
                return Ok(new { 
                    Token = token,
                    Username = request.Username,
                    Role = role,
                    ExpiresIn = "24 hours"
                });
            }
            
            return Unauthorized(new { Message = "Invalid username or password" });
        }
        
        private static bool IsValidUser(string username, string password)
        {
            // Simple hardcoded users for assessment
            return (username == "admin" && password == "password") ||
                   (username == "doctor" && password == "doctor123") ||
                   (username == "user" && password == "user123");
        }
    }
}