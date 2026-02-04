using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VK_Common_BE.Constants;
using VK_Common_BE.Models;
using VK_Common_BE.Services;

namespace VK_Common_BE.Controllers
{
    /// <summary>
    /// 製品管理APIコントローラー
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// すべての製品を取得（公開アクセス）
        /// </summary>
        /// <returns>製品リスト</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        /// <summary>
        /// IDで単一の製品を取得（認証が必要）
        /// </summary>
        /// <param name="id">製品ID</param>
        /// <returns>製品詳細</returns>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound(new { message = string.Format(MessageConstants.Errors.ProductNotFound, id) });
            }

            return Ok(product);
        }

        /// <summary>
        /// 新しい製品を作成（ユーザーロールが必要）
        /// </summary>
        /// <param name="product">製品情報</param>
        /// <returns>作成された製品</returns>
        [HttpPost]
        [Authorize(Policy = AuthenticationConstants.Policies.ApiOrB2C)]  // APIキーはGETのみ、Azure B2Cは全て許可
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdProduct = await _productService.CreateProductAsync(product);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }

        /// <summary>
        /// 製品情報を更新（管理者ロールが必要）
        /// </summary>
        /// <param name="id">製品ID</param>
        /// <param name="product">更新する製品情報</param>
        /// <returns>なし</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = AuthenticationConstants.Policies.ApiOrB2C)]  // APIキーはGETのみ、Azure B2Cは全て許可
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _productService.UpdateProductAsync(id, product);

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
            var result = await _productService.DeleteProductAsync(id);

            if (!result)
            {
                return NotFound(new { message = string.Format(MessageConstants.Errors.ProductNotFound, id) });
            }

            return NoContent();
        }
    }
}
