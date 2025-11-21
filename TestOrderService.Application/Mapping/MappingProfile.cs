using AutoMapper;
using TestOrderService.Application.DTOs;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Comment, CommentDto>();
        }
    }
}
