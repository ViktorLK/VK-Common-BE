using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace VK.Lab.LayeredArchitecture.Models
{
    /// <summary>
    /// CosmosDBドキュメントエンチE��チE��
    /// </summary>
    public class CosmosDocument
    {
        /// <summary>
        /// ドキュメンチED�E�EosmosDB忁E��！E
        /// </summary>
        [JsonProperty("id")]
        [Required(ErrorMessage = "Document ID is required")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// パ�EチE��ションキー
        /// </summary>
        [JsonProperty("partitionKey")]
        [Required(ErrorMessage = "Partition key is required")]
        public string PartitionKey { get; set; } = string.Empty;

        /// <summary>
        /// ドキュメント名
        /// </summary>
        [JsonProperty("name")]
        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 説昁E
        /// </summary>
        [JsonProperty("description")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        /// <summary>
        /// 作�E日晁E
        /// </summary>
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新日晁E
        /// </summary>
        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
