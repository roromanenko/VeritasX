using AutoMapper;
using VeritasX.Core.DTO;
using VeritasX.Infrastructure.Persistence.Entities;

namespace VeritasX.Application.Mapping;

public class DataCollectionJobProfile : Profile
{
    public DataCollectionJobProfile()
    {
        CreateMap<DataCollectionJob, DataCollectionJobDto>();
    }
}