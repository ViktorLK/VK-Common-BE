using VK.Lab.CleanArchitecture.GraphQL;

namespace VK.Lab.CleanArchitecture.Extensions
{
    /// <summary>
    /// GraphQL サービス登録拡張メソッド
    /// </summary>
    public static class GraphQLServiceExtensions
    {
        /// <summary>
        /// GraphQL サービスを追加
        /// </summary>
        public static IServiceCollection AddGraphQLServices(this IServiceCollection services)
        {
            services
                .AddGraphQLServer()
                .AddQueryType<Query>()
                .AddMutationType<Mutation>()
                .AddAuthorization()
                .AddFiltering()
                .AddSorting();
            
            return services;
        }
    }
}
