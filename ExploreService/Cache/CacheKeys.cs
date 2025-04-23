namespace ExploreService.Cache;

public static class CacheKeys
{
    public const string ModelList = "models:list:page:{0}:size:{1}";
    public const string ModelInfo = "models:info:{0}"; // {0} = modelId
    public const string ModelTags = "models:tags";
    public const string TagModels = "models:tag:{0}"; // {0} = tagId
    public const string SearchResults = "models:search:{0}:page:{1}:size:{2}"; // {0} = searchTerm, {1} = page, {2} = size
} 