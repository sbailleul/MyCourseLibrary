using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers.Extensions;
using CourseLibrary.API.Models.Authors;

namespace CourseLibrary.API.Profiles
{
    public class AuthorsProfile: Profile
    {
        public AuthorsProfile()
        {
            CreateMap<Author, AuthorDto>().ForMember(
                dst => dst.Name,
                opt => opt.MapFrom(
                    src => $"{src.FirstName} {src.LastName}")).ForMember(
                dst => dst.Age, 
                opt => opt.MapFrom(
                    src => src.DateOfBirth.GetCurrentAge(src.DateOfDeath)));

            CreateMap<AuthorForCreationDto, Author>();
            CreateMap<AuthorCreationWithDateOfDeathDto, Author>();
            CreateMap<Author, AuthorFullDto>();
        }
    }
}