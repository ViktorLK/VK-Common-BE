using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VK.Lab.LayeredArchitecture.Constants;
using VK.Lab.LayeredArchitecture.DTOs;
using VK.Lab.LayeredArchitecture.Models;
using VK.Lab.LayeredArchitecture.Services;

namespace VK.Lab.LayeredArchitecture.Controllers
{
    /// <summary>
    /// 製品管琁EPIコントローラー
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public ProductsController(IProductService productService, IMapper mapper)
        {
            _productService = productService;
            _mapper = mapper;
        }

        /// <summary>
        /// すべての製品を取得（�E開アクセス�E�E
        /// </summary>
        /// <returns>製品リスチE/returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
            return Ok(productDtos);
        }

        /// <summary>
        /// IDで単一の製品を取得（認証が忁E��E��E
        /// </summary>
        /// <param name="id">製品ID</param>
        /// <returns>製品詳細</returns>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound(new { message = string.Format(MessageConstants.Errors.ProductNotFound, id) });
            }

            var productDto = _mapper.Map<ProductDto>(product);
            return Ok(productDto);
        }

        /// <summary>
        /// 新しい製品を作�E�E�ユーザーロールが忁E��E��E
        /// </summary>
        /// <param name="createDto">製品情報</param>
        /// <returns>作�Eされた製品E/returns>
        [HttpPost]
        [Authorize(Policy = AuthenticationConstants.Policies.ApiOrB2C)]
        public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createDto)
        {
            var product = _mapper.Map<Product>(createDto);
            var createdProduct = await _productService.CreateProductAsync(product);
            var productDto = _mapper.Map<ProductDto>(createdProduct);

            return CreatedAtAction(nameof(GetProduct), new { id = productDto.Id }, productDto);
        }

        /// <summary>
        /// 製品情報を更新�E�管琁E��E��ールが忁E��E��E
        /// </summary>
        /// <param name="id">製品ID</param>
        /// <param name="updateDto">更新する製品情報</param>
        /// <returns>なぁE/returns>
        [HttpPut("{id}")]
        [Authorize(Policy = AuthenticationConstants.Policies.ApiOrB2C)]
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            var product = _mapper.Map<Product>(updateDto);
            var result = await _productService.UpdateProductAsync(id, product);

            if (!result)
            {
                return NotFound(new { message = string.Format(MessageConstants.Errors.ProductNotFound, id) });
            }

            return NoContent();
        }

        /// <summary>
        /// 製品を削除�E�管琁E��E��ールが忁E��E��E
        /// </summary>
        /// <param name="id">製品ID</param>
        /// <returns>なぁE/returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = AuthenticationConstants.Policies.ApiOrB2C)]
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
