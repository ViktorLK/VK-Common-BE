using System;
using System.ComponentModel.DataAnnotations;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore;

/// <summary>
/// Database entity representing a match key associated with a Knowledge Entry.
/// Follows AP.01, AP.03 (One file, one type).
/// </summary>
[VKPersistEntity(
    typeof(VKKnowledgeKey),
    TableName = "VK_AI_Psyche_Knowledge_Key",
    GenerateRepositoryAlias = false,
    GenerateQueriesAndSpecs = false)]
public sealed class VKPsycheKnowledgeKeyEntity
{
    /// <summary>
    /// Gets or sets the parent knowledge identifier foreign key.
    /// First component of composite primary key (KnowledgeId, Text).
    /// </summary>
    [VKPersistKey(Order = 1)]
    public VKKnowledgeId KnowledgeId { get; set; }

    /// <summary>
    /// Gets or sets the target keyword or substring to match.
    /// Second component of composite primary key (KnowledgeId, Text).
    /// </summary>
    [VKPersistKey(Order = 2)]
    [Required]
    [MaxLength(256)]
    public required string Text { get; set; }

    /// <summary>
    /// Gets or sets the match evaluation strategy (Contains, Exact, Regex, etc.).
    /// </summary>
    public VKKnowledgeMatchType MatchType { get; set; } = VKKnowledgeMatchType.Contains;

    /// <summary>
    /// Gets or sets a value indicating whether string matching is case-sensitive.
    /// </summary>
    public bool CaseSensitive { get; set; }

    /// <summary>
    /// Gets or sets the parent knowledge entity navigation reference.
    /// </summary>
    public VKPsycheKnowledgeEntity? Knowledge { get; set; }
}
