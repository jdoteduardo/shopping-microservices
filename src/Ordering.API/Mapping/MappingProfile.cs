using AutoMapper;
using Ordering.API.DTOs;
using Ordering.API.Models;

namespace Ordering.API.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Order mappings
        CreateMap<Order, OrderDto>();
        CreateMap<CreateOrderDto, Order>();

        // OrderItem mappings
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Subtotal));
        CreateMap<CreateOrderItemDto, OrderItem>();

        // Address mappings
        CreateMap<Address, AddressDto>();
        CreateMap<AddressDto, Address>();
    }
}
