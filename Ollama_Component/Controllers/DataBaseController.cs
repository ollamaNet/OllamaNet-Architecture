//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;

//namespace Ollama_Component.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class DataBaseController : ControllerBase
//    {
//        //This will be in the database project cause its a quering endpoint
//        [HttpPost("ModelInfo`DatabaseEndPoint`")]
//        public async Task<IActionResult> ModelInfo([FromBody] ModelInfo request)
//        {
//            if (request == null)
//            {
//                return BadRequest("Request body cannot be null."); // Validate input
//            }

//            // Use the Ollama HTTP Client to send the request
//            var response = "this is the AI model information. *Temporay till Database project*";

//            if (response == null)
//            {
//                return StatusCode(500, "Failed to process the chat request.");
//            }

//            return Ok(response);
//        }

//        //This will be in the database project cause its a quering endpoint
//        [HttpGet("AvailableModels`DatabaseEndPoint`")]
//        public async Task<IActionResult> AvailableModels()
//        {
//            // Use the Ollama HTTP Client to send the request
//            var response = new List<ModelInfo> { };

//            if (response == null)
//            {
//                return StatusCode(500, "Failed to process the chat request.");
//            }

//            return Ok(response);
//        }
//    }
//}
