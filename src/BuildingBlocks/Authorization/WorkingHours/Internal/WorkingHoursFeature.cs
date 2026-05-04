using VK.Blocks.Core;

namespace VK.Blocks.Authorization.WorkingHours.Internal;

/// <summary>
/// Marker class for the WorkingHours feature.
/// </summary>
[VKFeatureMarker(WorkingHoursConstants.FeatureName, typeof(VKAuthorizationBlock))]
internal sealed partial class WorkingHoursFeature;
