using Ollama_Component.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.Repositories.AIModelRepo;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
        public async Task<IActionResult> AddModel(AIModelDTO model)
        {
            var DbModel = new AIModel 
            {
                Name = model.Name,
                Description = model.Description,
                Version = model.Version,
                Size = model.Size,
                Digest = model.Digest,
                Format = model.Format,
                ParameterSize = model.ParameterSize,
                QuantizationLevel = model.QuantizationLevel,
                CreatedAt = DateTime.Now,
                ReleasedAt = model.ReleasedAt,
                User_Id = model.UserId
            };
            var response = AIModelRepository.AddAsync(DbModel);
            await AIModelRepository.SaveChangesAsync();
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


    public class AIModelDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Size { get; set; }
        public string Digest { get; set; }
        public string Format { get; set; }
        public string ParameterSize { get; set; }
        public string QuantizationLevel { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsArchived { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime ReleasedAt { get; set; }
        public string UserId{ get; set; }
    }
}
