using AutoMapper;
using VK.Lab.LayeredArchitecture.DTOs;
using VK.Lab.LayeredArchitecture.Models;

namespace VK.Lab.LayeredArchitecture.Mappings
{
    /// <summary>
    /// AutoMapper 映封E�E置
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Product <-> ProductDto
            CreateMap<Product, ProductDto>();
            CreateMap<ProductDto, Product>();

            // CreateProductDto -> Product
            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // UpdateProductDto -> Product
            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        }
    }
}
