using gRPC_Receiver.Entity;
using AutoMapper;

namespace gRPC_Receiver.Mapper
{
    public class EntityMappingProfile : Profile
    {
        public EntityMappingProfile()
        {
            // Обратное преобразование
            CreateMap<GrpcServices.Entity, AdkuEntity>()
            .ForMember(dest => dest.TagName, opt => opt.MapFrom(src => src.TagName))
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
            .ForMember(dest => dest.DateTime, opt => opt.MapFrom(src => src.DateTime.ToDateTime()))
            .ForMember(dest => dest.DateTimeUTC, opt => opt.MapFrom(src => src.DateTimeUTC.ToDateTime()))
            .ForMember(dest => dest.RegisterType, opt => opt.MapFrom(src => (RegisterType)src.RegisterType));
        }
    }
}
