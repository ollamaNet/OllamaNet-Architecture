using Ollama_Component.Connectors;
using OllamaSharp.Models;
using Ollama_DB_layer.Entities;
using Ollama_Component.Services.AdminServices.Models;
using Model = OllamaSharp.Models.Model;
using Ollama_Component.Mappers.DbMappers;
using Ollama_DB_layer.UOW;


namespace Ollama_Component.Services.AdminServices
{
    public class AdminService : IAdminService
    {
        public readonly IOllamaConnector _ollamaConnector;
        private readonly IUnitOfWork _unitOfWork;

        public AdminService(IOllamaConnector connector,  IUnitOfWork unitOfWork)
        {
            _ollamaConnector = connector;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Model>> InstalledModelsAsync(int pageNumber, int PageSize)
        {
            var models = await _ollamaConnector.GetInstalledModelsPaged(pageNumber, PageSize)
                         ?? throw new InvalidOperationException("Failed to retrieve installed models.");

            return models.Any() ? models : throw new InvalidOperationException("No models are installed.");
        }


        public async Task<ShowModelResponse?> ModelInfoAsync(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name cannot be empty.", nameof(modelName));

            return await _ollamaConnector.GetModelInfo(modelName)
                   ?? throw new InvalidOperationException($"Model '{modelName}' not found.");
        }


        public async Task<AIModel?> AddModelAsync(AddModelRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Name))
                throw new ArgumentException("Invalid model data. Model name is required.");

            AIModel? dbModel = null;

            if (!model.FromOllama)
                dbModel = AIModelMapper.FromRequestToAIModel(model);
            else
            {
                var ollamaModelInfo = await ModelInfoAsync(model.Name);
                if (ollamaModelInfo == null)
                    throw new InvalidOperationException("Model not installed in Ollama.");

                dbModel = AIModelMapper.FromOllamaToAIModel(model, ollamaModelInfo);
            }

            if (dbModel == null)
                throw new InvalidOperationException("Failed to create AI model.");

            if(model.Tags != null)
            {
                foreach (var tag in model.Tags)
                {
                    var dbTag = await _unitOfWork.TagRepo.GetByIdAsync(tag.TagId);
                    if (dbTag == null)
                        throw new InvalidOperationException("Tag not found.");

                    await _unitOfWork.ModelTagRepo.AddAsync(new ModelTag { AIModel_Id = dbModel.Name, Tag_Id = tag.TagId });
                }
            }

            await _unitOfWork.AIModelRepo.AddAsync(dbModel);
            await _unitOfWork.SaveChangesAsync();

            return await _unitOfWork.AIModelRepo.GetByIdAsync(model.Name);
        }


        public async Task<string> UninstllModelAsync(RemoveModelRequest model)
        {
            if (model is null || string.IsNullOrWhiteSpace(model.ModelName))
                throw new ArgumentException("Invalid model request.", nameof(model));

            await _ollamaConnector.RemoveModel(model.ModelName);

            if (!model.DeleteFromDB)
            {
                await SoftDeleteAIModelAsync(model.ModelName);
                return "Model removed from Ollama";
            }
               
            return await _unitOfWork.AIModelRepo.GetByIdAsync(model.ModelName) == null
                ? "Model removed from Ollama and DB"
                : "Model removed from Ollama but not from DB";
        }


        public async Task<string> SoftDeleteAIModelAsync(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name is required.", nameof(modelName));

            var model = await _unitOfWork.AIModelRepo.GetByIdAsync(modelName);
            if (model == null)
                return "Model not found";

            await _unitOfWork.AIModelRepo.SoftDeleteAsync(modelName);
            await _unitOfWork.SaveChangesAsync();

            return "Model soft deleted successfully";
        }


        public async Task<InstallProgressInfo> InstallModelAsync(string modelName, IProgress<InstallProgressInfo>? progress = null)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name cannot be empty.", nameof(modelName));

            // Check if the model is already installed
            var installedModels = await _ollamaConnector.GetInstalledModels();
            if (installedModels.Any(model => model.Name == modelName))
                return new InstallProgressInfo
                {
                    Completed = 100,
                    Total = 100,
                    Status = $"{modelName}already installed",
                    Digest = installedModels.Where(installedModels => installedModels.Name == modelName).FirstOrDefault()?.Digest

                };

            else
            {
                InstallProgressInfo? lastProgress = null;

                await foreach (var response in _ollamaConnector.PullModelAsync(modelName))
                {
                    lastProgress = response; // Store the latest progress

                    // Report progress to the caller (e.g., the controller)
                    progress?.Report(response);
                }
                return lastProgress ?? throw new InvalidOperationException($"Failed to install model '{modelName}'.");
            }
        }

        public async Task<List<Tag>> AddTags(List<string> tags)
        {
            var dbTags = tags.Select(tag => new Tag { Name = tag }).ToList();
            foreach (var tag in dbTags)
            {
                await _unitOfWork.TagRepo.AddAsync(tag);
            }
            await _unitOfWork.SaveChangesAsync();
            return dbTags;
        }

        public async Task<string> AddTagsToModel(string modelId, ICollection<AddTagToModelRequest> tags)
        {
            var model = await _unitOfWork.AIModelRepo.GetByIdAsync(modelId);
            if(model == null)
                throw new InvalidOperationException("Model not found.");
            foreach (var tag in tags)
            {
                var dbTag = await _unitOfWork.TagRepo.GetByIdAsync(tag.TagId);
                if (dbTag == null)
                    throw new InvalidOperationException("Tag not found.");
                
                await _unitOfWork.ModelTagRepo.AddAsync(new ModelTag { AIModel_Id = model.Name, Tag_Id = tag.TagId });
            }

            await _unitOfWork.SaveChangesAsync();
            return "Tags added successfully";
        }


        public async Task<IEnumerable<ApplicationUser>> GetUsers()
        {
            var users = await _unitOfWork.ApplicationUserRepo.GetAllAsync();
            return users;
        }

        public async Task<string> UpdateModel(UpdateModelRequest model)
        {
            var dbModel = await _unitOfWork.AIModelRepo.GetByIdAsync(model.Name)
                          ?? throw new InvalidOperationException("Model not found.");

            // Update properties if provided
            if (model.Description != null) dbModel.Description = model.Description;
            if (model.ReleasedAt != null) dbModel.ReleasedAt = (DateTime)model.ReleasedAt;
            if (model.License != null) dbModel.License = model.License;
            if (model.Template != null) dbModel.Template = model.Template;
            if (model.ModelFile != null) dbModel.ModelFile = model.ModelFile;
            if (model.ReferenceLink != null) dbModel.ReferenceLink = model.ReferenceLink;

            /*// Update tags if provided
            if (model.Tags != null)
            {
                var tagDict = model.Tags.ToDictionary(tag => tag.Id, tag => tag.Name);

                var updateTasks = dbModel.ModelTags
                    .Where(t => tagDict.ContainsKey(t.Tag.Id))
                    .Select(async t =>
                    {
                        var dbTag = await _unitOfWork.TagRepo.GetByIdAsync(t.Tag.Id)
                                   ?? throw new InvalidOperationException($"Tag {t.Tag.Id} not found.");
                        dbTag.Name = tagDict[t.Tag.Id];
                    });

                await Task.WhenAll(updateTasks);
            }*/

            await _unitOfWork.AIModelRepo.UpdateAsync(dbModel);
            await _unitOfWork.SaveChangesAsync();

            return "Model updated successfully";
        }
    }
}
