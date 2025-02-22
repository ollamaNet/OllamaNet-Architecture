//using Ollama_Component.Services.AdminServices.Models;
//using Ollama_Component.Services.ExploreService.Models;
//using Ollama_DB_layer.DataBaseHelpers;
//using Ollama_DB_layer.Entities;
//using OllamaSharp.Models;

//namespace Ollama_Component.Mappers
//{
//    public static class ModelCardsMapper
//    {
//        public static ModelCardsPaged ToModelCardsPaged(this PagedResult<AIModel> modeList)
//        {
//            if (modeList == null) throw new ArgumentNullException(nameof(modeList));

//           ModelCardsPaged cards = new()
//            {
//               ModelCards = modeList.Items.Select(x => new ModelCard
//               {
//                   ModelName = x.Name,
//                   Description = x.Description,
//                   Version = x.Version,
//                   Size = x.Size,
//                   ModelImageURL = "TEMP URL till adding the image url in the database",
//               }).ToList(),
//               PageSize = modeList.PageSize,
//               CurrentPage = modeList.CurrentPage,
//               TotalPages = modeList.TotalPages,
//           };

//            return cards;
//        }
//    }
//}
