using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace VK.Lab.CleanArchitecture.Models
{
    /// <summary>
    /// CosmosDBドキュメントエンティティ
    /// </summary>
    public class CosmosDocument
    {
        /// <summary>
        /// ドキュメントID（CosmosDB必須）
        /// </summary>
        [JsonProperty("id")]
        [Required(ErrorMessage = "Document ID is required")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// パーティションキー
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
        /// 説明
        /// </summary>
        [JsonProperty("description")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新日時
        /// </summary>
        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
