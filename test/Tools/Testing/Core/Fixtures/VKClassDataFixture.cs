namespace VK.Blocks.Testing;

/// <summary>
/// Specialized class fixture that automatically executes data setup and teardown defined by <typeparamref name="TData"/>.
/// Supports both <see cref="IVKClassDeclaredEntities"/> (yield return) and <see cref="IVKClassTestData"/> (async methods).
/// </summary>
/// <typeparam name="TData">The test data contract provider type.</typeparam>
public class VKClassDataFixture<TData> : VKClassFixture
{
    private List<object>? _cachedEntities;

    /// <inheritdoc />
    protected override async Task OnInitializeCoreAsync()
    {
        if (typeof(IVKClassDeclaredEntities).IsAssignableFrom(typeof(TData)))
        {
            var entities = TDataInvoker.GetSeedEntities<TData>().ToList();
            if (entities.Count > 0)
            {
                _cachedEntities = entities;
                if (this is IVKDeclaredEntityPersistenceStrategy persistence)
                {
                    await persistence.SaveEntitiesAsync(Services, entities).ConfigureAwait(false);
                }
            }
        }

        if (typeof(IVKClassTestData).IsAssignableFrom(typeof(TData)))
        {
            await TDataInvoker.InitializeDataAsync<TData>(Services).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    protected override async Task OnDisposeCoreAsync()
    {
        if (typeof(IVKClassTestData).IsAssignableFrom(typeof(TData)))
        {
            await TDataInvoker.CleanupDataAsync<TData>(Services).ConfigureAwait(false);
        }

        if (_cachedEntities is { Count: > 0 } entities && this is IVKDeclaredEntityPersistenceStrategy persistence)
        {
            await persistence.DeleteEntitiesAsync(Services, entities).ConfigureAwait(false);
        }
    }

    private static class TDataInvoker
    {
        public static IEnumerable<object> GetSeedEntities<T>()
            => typeof(T).IsAssignableTo(typeof(IVKClassDeclaredEntities))
                ? CallDeclaredEntities<T>()
                : [];

        public static Task InitializeDataAsync<T>(IServiceProvider services)
            => typeof(T).IsAssignableTo(typeof(IVKClassTestData))
                ? CallInitializeData<T>(services)
                : Task.CompletedTask;

        public static Task CleanupDataAsync<T>(IServiceProvider services)
            => typeof(T).IsAssignableTo(typeof(IVKClassTestData))
                ? CallCleanupData<T>(services)
                : Task.CompletedTask;

        private static IEnumerable<object> CallDeclaredEntities<T>()
        {
            if (typeof(IVKClassDeclaredEntities).IsAssignableFrom(typeof(T)))
            {
                return InvokeGetSeedEntities<T>();
            }

            return [];
        }

        private static IEnumerable<object> InvokeGetSeedEntities<T>() =>
            (typeof(T).GetInterfaceMap(typeof(IVKClassDeclaredEntities)))
                is { TargetMethods: { Length: > 0 } methods }
                ? (IEnumerable<object>)methods[0].Invoke(null, null)!
                : [];

        private static Task CallInitializeData<T>(IServiceProvider services)
        {
            var method = typeof(T).GetMethod(nameof(IVKClassTestData.InitializeDataAsync),
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            return method is not null ? (Task)method.Invoke(null, [services, CancellationToken.None])! : Task.CompletedTask;
        }

        private static Task CallCleanupData<T>(IServiceProvider services)
        {
            var method = typeof(T).GetMethod(nameof(IVKClassTestData.CleanupDataAsync),
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            return method is not null ? (Task)method.Invoke(null, [services, CancellationToken.None])! : Task.CompletedTask;
        }
    }
}
