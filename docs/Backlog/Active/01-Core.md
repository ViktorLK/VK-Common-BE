# Core Module Backlog

Tasks related to the foundational `VK.Blocks.Core` library.

## 🔴 High Priority (Technical Debt)

- [ ] **[Rule 12] Seal VKBlockBuilder**: `VKBlockBuilder<TMarker>` is currently not sealed. Review if any modules inherit from it; if not, seal it to comply with Rule 12.
- [ ] **[Standard] VKErrorType Alignment**: Reorder the `VKErrorType` enum members to match HTTP status code logic (4xx -> 5xx) to improve developer cognitive load.

## 🟡 Medium Priority (Refinement)

- [ ] **[Documentation] VKResult.FirstError**: Add missing XML documentation for the `FirstError` property in `VKResult.cs`.
- [ ] **[Architecture] Folder Consistency**: Standardize the split between `Contracts/` and `Results/`. Decide if `IVKResult` belongs in `Contracts` or `Results` and apply consistently across all blocks.
- [ ] **[Optimization] Frozen Collections**: Migrate `VKEntityMetadata` capability cache from `ConcurrentDictionary` to `FrozenDictionary` (Rule 4 / .NET 8+).

## 🔵 Low Priority (Future Optimization)

- [ ] **[Language] Covariant Markers**: Use C# 9 Covariant Return Types for `IVKBlockMarker.Dependencies` to enhance type safety.
- [ ] **[Automation] VKGuard CallerInfo**: Implement a Source Generator to optimize `VKGuard` caller argument expression resolution, reducing runtime string allocations.
- [ ] **[Performance] DI Interceptors**: Explore C# 12 Interceptors to optimize `IsVKBlockRegistered` checks in DI extensions, avoiding linear scans of the service collection.
