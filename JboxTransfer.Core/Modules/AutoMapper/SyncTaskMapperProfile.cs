using AutoMapper;
using JboxTransfer.Core.Helpers;
using JboxTransfer.Core.Models.Db;
using JboxTransfer.Core.Models.Output;

namespace JboxTransfer.Core.Modules.AutoMapper
{
    public class SyncTaskMapperProfile : Profile
    {
        public SyncTaskMapperProfile()
        {
            CreateMap<SyncTaskDbModel, SyncTaskDbModelOutputDto>()
                .ForMember(dest => dest.ParentPath, opt => opt.MapFrom(src => src.FilePath.GetParentPath()))
                ;
        }
    }
}
