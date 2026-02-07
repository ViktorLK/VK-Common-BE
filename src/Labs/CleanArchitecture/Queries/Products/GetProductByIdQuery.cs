using MediatR;
using VK.Lab.CleanArchitecture.DTOs.Products;

namespace VK.Lab.CleanArchitecture.Queries.Products
{
    /// <summary>
    /// 根据ID获取产品查询
    /// </summary>
    public class GetProductByIdQuery : IRequest<ProductDto?>
    {
        /// <summary>
        /// 产品ID
        /// </summary>
        public int Id { get; set; }

        public GetProductByIdQuery(int id)
        {
            Id = id;
        }
    }
}
