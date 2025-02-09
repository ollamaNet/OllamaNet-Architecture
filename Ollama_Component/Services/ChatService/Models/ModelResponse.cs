using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace Ollama_Component.Services.ChatService.Models
{
    public class ModelResponse
    {

        public AuthorRole Role { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonIgnore]
        public string? Content
        {
            get
            {
                var textContent = Items.OfType<TextContent>().FirstOrDefault();
                return textContent?.Text;
            }
            set
            {
                var textContent = Items.OfType<TextContent>().FirstOrDefault();
                if (textContent is not null)
                {
                    textContent.Text = value;
                }
                else if (value is not null)
                {
                    Items.Add(new TextContent(
                        text: value,
                        modelId: ModelId,
                        innerContent: InnerContent,
                        encoding: Encoding,
                        metadata: Metadata
                    )
                    { MimeType = MimeType });
                }
            }
        }

        [JsonIgnore]
        public Encoding Encoding
        {
            get
            {
                var textContent = Items.OfType<TextContent>().FirstOrDefault();
                if (textContent is not null)
                {
                    return textContent.Encoding;
                }

                return _encoding;
            }
            set
            {
                _encoding = value;

                var textContent = Items.OfType<TextContent>().FirstOrDefault();
                if (textContent is not null)
                {
                    textContent.Encoding = value;
                }
            }
        }

        public ChatMessageContentItemCollection Items
        {
            get => _items ??= [];
            set => _items = value;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ModelId { get; set; }

        [JsonIgnore]
        public object? InnerContent { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? MimeType { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IReadOnlyDictionary<string, object?>? Metadata { get; set; }

        private ChatMessageContentItemCollection? _items;
        private Encoding _encoding;


        public long TotalDuration { get; set; }


        public long LoadDuration { get; set; }


        public int PromptEvalCount { get; set; }


        public long PromptEvalDuration { get; set; }


        public int EvalCount { get; set; }


        [MaxLength(50)]
        public long EvalDuration { get; set; }


        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
