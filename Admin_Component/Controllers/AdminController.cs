using Admin_Component.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Admin_Component.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        public IAdminService AdminService { get; set; }

        public AdminController(IAdminService adminService)
        {
            AdminService = adminService;
        }

        [HttpGet("InstalledModels")]
        public async Task<IActionResult> InstalledModels()
        {

            var response = await AdminService.InstalledModelsAsync();

            if (response == null)
            {
                return StatusCode(500, "Failed to process the chat request.");
            }

            return Ok(response);
        }


        [HttpPost("AddModel")]
        public async Task<IActionResult> AddModel()
        {

            var response = "here is aconnection with DB context adding a previosly installed model listing it for users to use it";

            if (response == null)
            {
                return StatusCode(500, "Failed to process the chat request.");
            }

            return Ok(response);
        }


        [HttpPost("InstallModel")]
        public async Task<IActionResult> InstallModel()
        {

            var response = "here is a connection with Ollama to pull a model from ollama ";

            if (response == null)
            {
                return StatusCode(500, "Failed to process the chat request.");
            }

            return Ok(response);
        }


        [HttpPost("DeleteModel")]
        public async Task<IActionResult> DeleteModel()
        {

            var response = "here is a connection with Ollama to pull a model from ollama";

            if (response == null)
            {
                return StatusCode(500, "Failed to process the chat request.");
            }

            return Ok(response);
        }



        [HttpPost("RemoveModel")]
        public async Task<IActionResult> RemoveModel()
        {

            var response = "here is a connection with Ollama to REMOVE a model from ollama and the DATABASE";

            if (response == null)
            {
                return StatusCode(500, "Failed to process the chat request.");
            }

            return Ok(response);
        }
    }
}
