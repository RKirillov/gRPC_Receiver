using gRPC_Receiver.Entity;
using AutoMapper;

namespace gRPC_Receiver.Mapper
{
    public class EntityMappingProfile : Profile
    {
        public EntityMappingProfile()
        {
            CreateMap<AdkuEntity, GrpcServices.Entity>().ReverseMap()
                .ForMember(dest => dest.TagName, opt => opt.MapFrom(src => src.TagName))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));
        }
    }
}
