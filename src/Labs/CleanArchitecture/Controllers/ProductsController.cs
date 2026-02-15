using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VK.Lab.CleanArchitecture.Commands.Products;
using VK.Lab.CleanArchitecture.Constants;
using VK.Lab.CleanArchitecture.DTOs.Products;
using VK.Lab.CleanArchitecture.Queries.Products;

namespace VK.Lab.CleanArchitecture.Controllers
{
    /// <summary>
    /// 製品管理APIコントローラー
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// すべての製品を取得（公開アクセス）
        /// </summary>
        /// <returns>製品リスト</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var query = new GetAllProductsQuery();
            var products = await _mediator.Send(query);
            return Ok(products);
        }

        /// <summary>
        /// IDで単一の製品を取得（認証が必要）
        /// </summary>
        /// <param name="id">製品ID</param>
        /// <returns>製品詳細</returns>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var query = new GetProductByIdQuery(id);
            var product = await _mediator.Send(query);

            if (product is null)
            {
                return NotFound(new { message = string.Format(MessageConstants.Errors.ProductNotFound, id) });
            }

            return Ok(product);
        }

        /// <summary>
        /// 新しい製品を作成（ユーザーロールが必要）
        /// </summary>
        /// <param name="command">製品情報</param>
        /// <returns>作成された製品</returns>
        [HttpPost]
        [Authorize(Policy = AuthenticationConstants.Policies.ApiOrB2C)]  // APIキーはGETのみ、Azure B2Cは全て許可
        public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductCommand command)
        {
            var createdProduct = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }

        /// <summary>
        /// 製品情報を更新（管理者ロールが必要）
        /// </summary>
        /// <param name="id">製品ID</param>
        /// <param name="command">更新する製品情報</param>
        /// <returns>なし</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = AuthenticationConstants.Policies.ApiOrB2C)]  // APIキーはGETのみ、Azure B2Cは全て許可
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound(new { message = string.Format(MessageConstants.Errors.ProductNotFound, id) });
            }

            return NoContent();
        }

        /// <summary>
        /// 製品を削除（管理者ロールが必要）
        /// </summary>
        /// <param name="id">製品ID</param>
        /// <returns>なし</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = AuthenticationConstants.Policies.ApiOrB2C)]  // APIキーはGETのみ、Azure B2Cは全て許可
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var command = new DeleteProductCommand(id);
            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound(new { message = string.Format(MessageConstants.Errors.ProductNotFound, id) });
            }

            return NoContent();
        }
    }
}
