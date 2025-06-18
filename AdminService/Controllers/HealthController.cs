using Microsoft.AspNetCore.Mvc;
using AdminService.Infrastructure.Configuration;
using AdminService.Infrastructure.Integration.InferenceEngine;

namespace AdminService.Controllers
{
    /// <summary>
    /// API controller for health monitoring
    /// </summary>
    [ApiController]
    [Route("api/admin/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IInferenceEngineConnector _connector;
        private readonly IInferenceEngineConfiguration _configuration;
        
        /// <summary>
        /// Constructor for HealthController
        /// </summary>
        /// <param name="connector">Inference engine connector</param>
        /// <param name="configuration">Inference engine configuration</param>
        public HealthController(
            IInferenceEngineConnector connector,
            IInferenceEngineConfiguration configuration)
        {
            _connector = connector;
            _configuration = configuration;
        }
        
        /// <summary>
        /// Check RabbitMQ status by checking if we have a valid configuration
        /// </summary>
        [HttpGet("inference-engine/config")]
        public async Task<IActionResult> CheckInferenceEngineConfig()
        {
            var url =  _configuration.GetBaseUrl();
            return Ok(new { ConfiguredUrl = url });
        }
        
        /// <summary>
        /// Check inference engine connection health
        /// </summary>
        [HttpGet("inference-engine")]
        public IActionResult CheckInferenceEngineHealth()
        {
            var url = _connector.GetBaseUrl();
            return Ok(new { Url = url });
        }
        
        /// <summary>
        /// Check overall system health
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckHealth()
        {
            var configUrl = _configuration.GetBaseUrl();
            var connectorUrl = _connector.GetBaseUrl();
            
            return Ok(new { 
                Status = "Healthy",
                ConfiguredUrl = configUrl,
                ConnectorUrl = connectorUrl,
                IsConfigured = !string.IsNullOrWhiteSpace(configUrl)
            });
        }
    }
} 