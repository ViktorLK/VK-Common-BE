using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.WorkingHours.Internal;

/// <summary>
/// Marker class for the WorkingHours feature.
/// </summary>
[ExcludeFromCodeCoverage]
[VKFeatureMarker(WorkingHoursConstants.FeatureName, typeof(VKAuthorizationBlock))]
internal sealed partial class WorkingHoursFeature;

