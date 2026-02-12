using AutoMapper;
using Basket.API.DTOs;
using Basket.API.Models;

namespace Basket.API.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Basket mappings
        CreateMap<Models.Basket, BasketDto>()
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice));

        CreateMap<UpdateBasketDto, Models.Basket>();

        // BasketItem mappings
        CreateMap<BasketItem, BasketItemDto>()
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Subtotal));

        CreateMap<UpdateBasketItemDto, BasketItem>();
        CreateMap<AddItemDto, BasketItem>();
    }
}
