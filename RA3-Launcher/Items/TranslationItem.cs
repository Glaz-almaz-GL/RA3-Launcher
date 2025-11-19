using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RA3_Launcher.Items
{
    public class TranslationItem
    {
        [JsonIgnore]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonIgnore]
        [JsonPropertyName("creationDate")]
        public DateTime CreationDate { get; set; } = DateTime.MinValue;

        [JsonIgnore]
        [JsonPropertyName("language")]
        public string Language { get; set; } = string.Empty;

        [JsonIgnore]
        [JsonPropertyName("downloadUrl")]
        public string DownloadUrl { get; set; } = string.Empty;

        [JsonIgnore]
        [JsonPropertyName("isDownlodable")]
        public bool IsDownlodable { get; set; } = true;

        public TranslationItem(string name, string description, DateTime creationDate, string language, string downloadUrl, bool isDownlodable)
        {
            Name = name;
            Description = description;
            CreationDate = creationDate;
            Language = language;
            DownloadUrl = downloadUrl;
            IsDownlodable = isDownlodable;
        }

        public TranslationItem() { }
    }
}
