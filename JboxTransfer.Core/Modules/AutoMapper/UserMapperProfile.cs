using AutoMapper;
using JboxTransfer.Core.Helpers;
using JboxTransfer.Core.Models.Db;
using JboxTransfer.Core.Models.Output;

namespace JboxTransfer.Core.Modules.AutoMapper
{
    public class UserMapperProfile : Profile
    {
        public UserMapperProfile()
        {
            CreateMap<UserStatistics, UserStatisticsOutputDto>()
                ;
        }
    }
}
