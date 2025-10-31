using AutoMapper;
using NaturalShop.API.DTOs;
using NaturalShop.API.Models;

namespace NaturalShop.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Entity -> DTO
            CreateMap<Product, ProductDto>();

            // DTO -> Entity
            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price ?? 0))
                .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.Stock ?? 0));
            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price ?? 0))
                .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.Stock ?? 0));

            // Optionally DTO <- Entity for update responses if needed
            CreateMap<Product, UpdateProductDto>();
        }
    }
}


