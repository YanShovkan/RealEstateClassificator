using AutoMapper;
using RealEstateClassificator.Core.Dto;
using RealEstateClassificator.Dal.Entities;

namespace RealEstateClassificator.Core.Profiles;

public class CardProfile : Profile
{
    public CardProfile()
    {
        CreateMap<CardDto, Card>()
            .ForMember(_ => _.Price, opt => opt.MapFrom(_ => Convert.ToInt64(_.Price)))
            .ForMember(_ => _.Floor, opt => opt.MapFrom(_ => Convert.ToInt64(_.Floor)))
            .ForMember(_ => _.Price, opt => opt.MapFrom(_ => Convert.ToInt64(_.Price)))
            .ForMember(_ => _.Price, opt => opt.MapFrom(_ => Convert.ToInt64(_.Price)))
            .ForMember(_ => _.Price, opt => opt.MapFrom(_ => Convert.ToInt64(_.Price)))
            .ForMember(_ => _.Price, opt => opt.MapFrom(_ => Convert.ToInt64(_.Price)))
            .ForMember(_ => _.Price, opt => opt.MapFrom(_ => Convert.ToInt64(_.Price)))
            .ForMember(_ => _.Price, opt => opt.MapFrom(_ => Convert.ToInt64(_.Price)))
            .ForMember(_ => _.Price, opt => opt.MapFrom(_ => Convert.ToInt64(_.Price)));
    }
}
