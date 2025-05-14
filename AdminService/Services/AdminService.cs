//using AdminService.Connectors;
//using Ollama_DB_layer.Entities;
//using Ollama_DB_layer.UOW;
//using AdminService.Mappers;

//namespace AdminService
//{
//    public class AdminService : IAdminService
//    {
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly IOllamaConnector _ollamaConnector;

//        public AdminService(IOllamaConnector connector, IUnitOfWork unitOfWork)
//        {
//            _ollamaConnector = connector;
//            _unitOfWork = unitOfWork;
//        }








//        public async Task<AIModel?> AddModelAsync(AddModelRequest model, string userId)
//        {
//            if (model == null || string.IsNullOrWhiteSpace(model.Name))
//                throw new ArgumentException("Invalid model data. Model name is required.");

//            AIModel? dbModel;

//            if (!model.FromOllama)
//                dbModel = AIModelMapper.FromRequestToAIModel(model, userId);
//            else
//            {
//                // We need to get model info from Ollama
//                var ollamaModelInfo = await _ollamaConnector.GetModelInfo(model.Name);
//                if (ollamaModelInfo == null)
//                    throw new InvalidOperationException("Model not installed in Ollama.");

//                dbModel = AIModelMapper.FromOllamaToAIModel(model, ollamaModelInfo, userId);
//            }

//            if (dbModel == null)
//                throw new InvalidOperationException("Failed to create AI model.");

//            if (model.Tags != null)
//            {
//                foreach (var tag in model.Tags)
//                {
//                    var dbTag = await _unitOfWork.TagRepo.GetByIdAsync(tag.TagId);
//                    if (dbTag == null)
//                        throw new InvalidOperationException("Tag not found.");

//                    await _unitOfWork.ModelTagRepo.AddAsync(new ModelTag { AIModel_Id = dbModel.Name, Tag_Id = tag.TagId });
//                }
//            }

//            await _unitOfWork.AIModelRepo.AddAsync(dbModel);
//            await _unitOfWork.SaveChangesAsync();

//            return await _unitOfWork.AIModelRepo.GetByIdAsync(model.Name);
//        }








//        public async Task<string> SoftDeleteAIModelAsync(string modelName)
//        {
//            if (string.IsNullOrWhiteSpace(modelName))
//                throw new ArgumentException("Model name is required.", nameof(modelName));

//            var model = await _unitOfWork.AIModelRepo.GetByIdAsync(modelName);
//            if (model == null)
//                return "Model not found";

//            await _unitOfWork.AIModelRepo.SoftDeleteAsync(modelName);
//            await _unitOfWork.SaveChangesAsync();

//            return "Model soft deleted successfully";
//        }

//        public async Task<List<Tag>> AddTags(List<string> tags)
//        {
//            var dbTags = tags.Select(tag => new Tag { Name = tag }).ToList();
//            foreach (var tag in dbTags)
//            {
//                await _unitOfWork.TagRepo.AddAsync(tag);
//            }
//            await _unitOfWork.SaveChangesAsync();
//            return dbTags;
//        }

//        public async Task<string> AddTagsToModel(string modelId, ICollection<AddTagToModelRequest> tags)
//        {
//            var model = await _unitOfWork.AIModelRepo.GetByIdAsync(modelId);
//            if (model == null)
//                throw new InvalidOperationException("Model not found.");

//            foreach (var tag in tags)
//            {
//                var dbTag = await _unitOfWork.TagRepo.GetByIdAsync(tag.TagId);
//                if (dbTag == null)
//                    throw new InvalidOperationException("Tag not found.");

//                await _unitOfWork.ModelTagRepo.AddAsync(new ModelTag { AIModel_Id = model.Name, Tag_Id = tag.TagId });
//            }

//            await _unitOfWork.SaveChangesAsync();
//            return "Tags added successfully";
//        }

//        public async Task<IEnumerable<ApplicationUser>> GetAllUsers()
//        {
//            var users = await _unitOfWork.ApplicationUserRepo.GetAllAsync();
//            return users;
//        }

//        public async Task<string> UpdateModel(UpdateModelRequest model)
//        {
//            var dbModel = await _unitOfWork.AIModelRepo.GetByIdAsync(model.Name)
//                         ?? throw new InvalidOperationException("Model not found.");

//            // Update properties if provided
//            if (model.Description != null) dbModel.Description = model.Description;
//            if (model.ReleasedAt != null) dbModel.ReleasedAt = (DateTime)model.ReleasedAt;
//            if (model.License != null) dbModel.License = model.License;
//            if (model.Template != null) dbModel.Template = model.Template;
//            if (model.ModelFile != null) dbModel.ModelFile = model.ModelFile;
//            if (model.ReferenceLink != null) dbModel.ReferenceLink = model.ReferenceLink;

//            await _unitOfWork.AIModelRepo.UpdateAsync(dbModel);
//            await _unitOfWork.SaveChangesAsync();

//            return "Model updated successfully";
//        }
//    }
//}
