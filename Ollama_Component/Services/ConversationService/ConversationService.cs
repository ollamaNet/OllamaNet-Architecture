using Ollama_Component.Services.ConversationService.Models;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.UOW;

namespace Ollama_Component.Services.ConversationService
{
    public class ConversationService : IConversationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ConversationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OpenConversationResponse> CreateConversationAsync(OpenConversationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            var conversation = new Conversation
            {
                User_Id = request.UserId,
                AI_Id = request.ModelName,
                SystemMessage = request.SystemMessage,
                CreatedAt = request.CreatedAt
            };
            _unitOfWork.ConversationRepo.AddAsync(conversation);
            await _unitOfWork.SaveChangesAsync();
            var conv= await _unitOfWork.ConversationRepo.GetByIdAsync(conversation.Id);

            return new OpenConversationResponse
            {
                ConversationId = conv.Id,
                Modelname = conv.AI_Id
            };
        }

    }
}
