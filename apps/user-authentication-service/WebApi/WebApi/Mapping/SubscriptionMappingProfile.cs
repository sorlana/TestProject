using AutoMapper;
using Domain.Entities.Entities;
using Services.Contracts.DTOs;

namespace WebApi.Mapping;

/// <summary>
/// Профиль AutoMapper для маппинга сущностей подписок
/// </summary>
public class SubscriptionMappingProfile : Profile
{
    public SubscriptionMappingProfile()
    {
        // Маппинг UserSubscription -> UserSubscriptionDto
        CreateMap<UserSubscription, UserSubscriptionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.SubscriptionPlanId, opt => opt.MapFrom(src => src.SubscriptionPlanId))
            .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.PlanName))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.AutoRenew, opt => opt.MapFrom(src => src.AutoRenew))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.CancelledAt, opt => opt.MapFrom(src => src.CancelledAt));

        // Обратный маппинг UserSubscriptionDto -> UserSubscription (для создания новой подписки)
        CreateMap<UserSubscriptionDto, UserSubscription>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // ID генерируется БД
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.SubscriptionPlanId, opt => opt.MapFrom(src => src.SubscriptionPlanId))
            .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.PlanName))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.AutoRenew, opt => opt.MapFrom(src => src.AutoRenew))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.CancelledAt, opt => opt.MapFrom(src => src.CancelledAt))
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.NopCommerceOrderId, opt => opt.Ignore());
    }
}
