using Ollama_Component.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.Repositories.AIModelRepo;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ollama_Component.Services.AdminServices.Models;

namespace Ollama_Component.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        public IAdminService AdminService { get; set; }
        public IAIModelRepository AIModelRepository { get; set; }   

        public AdminController(IAdminService adminService, IAIModelRepository AIModelRepo)
        {
            AdminService = adminService;
            AIModelRepository = AIModelRepo;
        }


        [HttpPost("AddModel")]
        public async Task<IActionResult> AddModel(AddModelRequest model)
        {
          
            var response = await AdminService.AddModelAsync(model);
            if (response == null)
            {
                return StatusCode(500, "Failed to process the chat request.");
            }
            return Ok(response);
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


        [HttpPost("ModelInfo")]
        public async Task<IActionResult> ModelInfo(string modelName, bool storeModel)
        {

            var response = await AdminService.GetModelInfo(modelName);

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
