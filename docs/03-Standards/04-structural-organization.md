# Standard 04: Structural Organization & Naming

## 1. Directory Layout (Vertical Slices)
Prioritize domain-driven vertical slices over technical type grouping.

- `{FeatureName}/`: Domain logic (Interfaces, Handlers).
- `{FeatureName}/Internal/`: Encapsulated implementation (Level 2+).
- `DependencyInjection/`: Registration logic.
- `Diagnostics/`: Logging and metrics.

## 2. Visibility Boundaries (AP.03)
- **Level 1 (Root/Feature Folder)**: Must be `public`. Namespace should be flat (e.g., `VK.Blocks.Auth`).
- **Level 2+ (Subfolders)**: Must be `internal`. Namespace matches the full path.

## 3. Naming Conventions
- **Public Types**: MUST use the `VK` prefix (e.g., `VKAuthOptions`, `IVKAuthService`) to avoid collisions in the flattened namespace.
- **Internal Types**: MUST NOT use the `VK` prefix.

## 4. Args Pattern (AP.05)
Per-request behavior overrides must follow the "Global Default + Local Override" pattern:
- **Global**: `IVKBlockOptions`
- **Local**: `XxxArgs` record (e.g., `VKChatArgs`)
- **Merging**: `args?.Property ?? options.Property`

## 5. Modern C# Semantics
- **Sealed by Default**: All classes are `sealed` unless polymorphism is required.
- **Required Keyword**: Use for non-nullable properties in records/DTOs.
- **Collection Expressions**: Use `[]` syntax for collections.
- **VKGuard**: Mandatory for boundary validation.

