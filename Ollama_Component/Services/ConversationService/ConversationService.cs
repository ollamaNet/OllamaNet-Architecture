using Ollama_Component.Services.ConversationService.Models;
using Ollama_DB_layer.DataBaseHelpers;
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
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.ConversationRepo.AddAsync(conversation);
            await _unitOfWork.SaveChangesAsync();

            var conv = await _unitOfWork.ConversationRepo.GetByIdAsync(conversation.Id);

            return new OpenConversationResponse
            {
                ConversationId = conv.Id,
                Modelname = conv.AI_Id
            };
        }



        public async Task<PagedResult<Conversation>> GetConversationsAsync(string UserId)
        {
            ArgumentNullException.ThrowIfNull(UserId);

            var ConversationList = await _unitOfWork.ConversationRepo.ConversationPagination(UserId, 1, 15);

            return ConversationList;
        }


        public async Task<GetConversationInfoResponse> GetConversationInfoAsync(string ConversationId)
        {
            if (ConversationId == null)
                throw new ArgumentNullException(nameof(ConversationId));

            var ConversationList = await _unitOfWork.ConversationRepo.GetByIdAsync(ConversationId);

            return new GetConversationInfoResponse
            {
                ConversationId = ConversationList.Id,
                ModelName = ConversationList.AI_Id,
                SystemMessage = ConversationList.SystemMessage,
                CreatedAt = ConversationList.CreatedAt,
                TokenUsage = ConversationList.TokensUsed.ToString()
            };
        }

        public async Task<List<MessageHistory>> GetConversationMessagesAsync(string conversationId)
        {
            var ConversationMessages = await _unitOfWork.GetHistoryRepo.GetHistoryForUserAsync(conversationId);
            return ConversationMessages;
        }
    }
}
