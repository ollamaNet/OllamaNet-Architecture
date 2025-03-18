using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ollama_Component.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class secure : ControllerBase
    {

        [HttpGet]
        public IActionResult GetHello()
        {
            return Ok("Hello from secured controller");
        }
    }
}
