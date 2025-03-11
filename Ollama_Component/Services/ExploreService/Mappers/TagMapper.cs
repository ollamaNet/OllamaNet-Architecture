using Ollama_Component.Services.ExploreService.Models;
using Ollama_DB_layer.Entities;

namespace Ollama_Component.Services.ExploreService.Mappers
{
    public static class TagMapper
    {
        public static List<GetTagsResponse> ToGetTagsResponse(this IEnumerable<Tag> tags)
        {
            List<GetTagsResponse> mappedTags = new();
            foreach (var tag in tags)
            {
                mappedTags.Add(new GetTagsResponse
                {
                    Id = tag.Id,
                    Name = tag.Name
                }
                );
            }
            return mappedTags;
        }
    }
}
