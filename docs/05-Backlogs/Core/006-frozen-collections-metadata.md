# Task: Optimization: Frozen Collections
**ID**: CORE-006
**Status**: 🟡 Medium | #Performance
**Target**: `VK.Blocks.Core` -> `VKTypeMetadataCache`
**Ref**: Rule-4

## 📝 Description
Migrate `VKEntityMetadata` capability cache from `ConcurrentDictionary` to `FrozenDictionary` (.NET 8+). Since this cache is mostly read-heavy and stabilized after startup, `FrozenDictionary` provides better read performance.

## ✅ DoD (Definition of Done)
- [ ] Implement `FrozenDictionary` for metadata cache
- [ ] Ensure thread-safe initialization (build once)
- [ ] Run benchmarks to verify improvement
