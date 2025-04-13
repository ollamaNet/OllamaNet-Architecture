using System.Linq;
using System.Text;
using Azure.Core;
using System.Threading;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using AdminService.DTOs;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using Model = OllamaSharp.Models.Model;
using OpenTelemetry.Trace;
using AdminService.Mappers;
using AdminService.DTOs;

namespace AdminService.Connectors
{
    public class OllamaConnector : IOllamaConnector
    {
        private readonly IOllamaApiClient ollamaApiClient;

        public OllamaConnector(IOllamaApiClient ollamaApiClient)
        {
            this.ollamaApiClient = ollamaApiClient;
        }

        public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();


        public async Task<IEnumerable<Model>> GetInstalledModels()
        {
            var response = await ollamaApiClient.ListLocalModelsAsync();

            return response;
        }

        public async Task<IEnumerable<Model>> GetInstalledModelsPaged(int pageNumber, int pageSize)
        {
            var response = await ollamaApiClient.ListLocalModelsAsync();

            // Ensure pageNumber and pageSize are valid
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            // Calculate the items to skip and take for pagination
            var skip = (pageNumber - 1) * pageSize;
            var pagedResponse = response.Skip(skip).Take(pageSize);

            return pagedResponse;
        }

        public async Task<ShowModelResponse> GetModelInfo(string modelName)
        {
            var modelInfo = await ollamaApiClient.ShowModelAsync(modelName);
            return modelInfo;
        }

        public async Task<string> RemoveModel(string modelName)
        {

            await ollamaApiClient.DeleteModelAsync(modelName);

            var models = await ollamaApiClient.ListLocalModelsAsync();
            var modelExists = models.Any(m => m.Name == modelName);

            return modelExists ? "Model not removed successfully" : "Model removed successfully";
        }


        public async IAsyncEnumerable<InstallProgressInfo> PullModelAsync(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name cannot be empty.", nameof(modelName));

            await foreach (var response in ollamaApiClient.PullModelAsync(modelName))
            {
                yield return new InstallProgressInfo
                {
                    Status = response.Status,
                    Digest = response.Digest,
                    Total = response.Total,
                    Completed = response.Completed,
                    Progress = response.Total > 0
                        ? (double)response.Completed / response.Total * 100
                        : 0
                };
            }
        }



    }
}