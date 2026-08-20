using VK.Blocks.Core;

namespace VK.Blocks.Workflow;

/// <summary>
/// Feature marker and registration for Workflow Definition slice.
/// </summary>
[VKFeature(typeof(VKWorkflowBlock), OptionsType = typeof(VKDefinitionOptions))]
internal sealed partial class DefinitionFeature;
