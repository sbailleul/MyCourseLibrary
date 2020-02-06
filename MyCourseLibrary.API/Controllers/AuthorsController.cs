using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CourseLibrary.API.ActionConstraints;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers.Extensions;
using CourseLibrary.API.Helpers.Pagination;
using CourseLibrary.API.Models;
using CourseLibrary.API.Models.Authors;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;

        public AuthorsController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper, IPropertyMappingService propertyMappingService, IPropertyCheckerService propertyCheckerService)
        {
            _courseLibraryRepository = courseLibraryRepository ??
                                       throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        }

        // GET
        [HttpGet(Name = "[action]")]
        [HttpHead]
        public IActionResult GetAuthors([FromQuery] AuthorsResourceParameters authorsResourceParameters )
        {
            if (!_propertyMappingService.ValidMappingExistFor<AuthorDto, Author>(authorsResourceParameters.OrderBy) ||
                !_propertyCheckerService.TypeHasProperty<AuthorDto>(authorsResourceParameters.Fields))
            {
                return BadRequest();
            }
            var authors = _courseLibraryRepository.GetAuthors(authorsResourceParameters);
            
            var metadata = PaginationMetadata.Create(authors);
            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));
            
            var links = GetLinksForAuthors(authorsResourceParameters, authors.HasNext, authors.HasPrevious);
            
            var shapedAuthors = _mapper.Map<IEnumerable<AuthorDto>>(authors)
                .ShapeData(authorsResourceParameters.Fields);
            var shapedAuthorsWithLinks = shapedAuthors.Select(sa =>
            {
                var shapedAuthor = sa as IDictionary<string, object>;
                var authorLinks = GetLinksForAuthor((Guid) shapedAuthor["Id"]);
                shapedAuthor.Add("links", authorLinks);
                return shapedAuthor;
            });

            return  Ok(new {value = shapedAuthorsWithLinks, links});
        }
        
        
        [Produces("application/json",
            "application/vnd.marvin.hateoas+json",
            "application/vnd.marvin.author.full+json",
            "application/vnd.marvin.author.full.hateoas+json",
            "application/vnd.marvin.author.friendly+json",
            "application/vnd.marvin.author.friendly.hateoas+json"
        )]
        [HttpGet("{authorId}", Name = "[action]")]
        public IActionResult GetAuthor(Guid authorId, string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out var parsedMediaType) || 
                !_propertyCheckerService.TypeHasProperty<AuthorDto>(fields)) return BadRequest();
            
            var author = _courseLibraryRepository.GetAuthor(authorId);
            
            if (author == null) return NotFound();

            var includeLinks =
                parsedMediaType.SubTypeWithoutSuffix.EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);
            var links = new List<LinkDto>() as IEnumerable<LinkDto>;
            if (includeLinks)
            {
                links = GetLinksForAuthor(authorId, fields);
            }

            var primaryMediaType = includeLinks
                ? parsedMediaType.SubTypeWithoutSuffix.Substring(0,
                    parsedMediaType.SubTypeWithoutSuffix.Length - "hateoas".Length) : parsedMediaType.SubTypeWithoutSuffix;

            var authorToReturn = (primaryMediaType == "vnd.marvin.author.full" ?
                _mapper.Map<AuthorFullDto>(author).ShapeData(fields) : _mapper.Map<AuthorDto>(author).ShapeData(fields)) as IDictionary<string, object>;

            if (includeLinks)
            {
                authorToReturn.Add("links", links);
            }
            
            return Ok(authorToReturn);
        }
        

        [RequestHeaderMatchesMediaType("Content-Type",
            "application/vnd.marvin.authorforcreationwithdateofdeath+json")]
        [Consumes( "application/vnd.marvin.authorforcreationwithdateofdeath+json")]
        [HttpPost(Name = "[action]")]
        public IActionResult CreateAuthorWithDateOfDeath(AuthorCreationWithDateOfDeathDto authorCreationWithDateOfDeathDto)
        {
            return CreateAuthor(authorCreationWithDateOfDeathDto);
        }

        
        [RequestHeaderMatchesMediaType("Content-Type", 
            "application/json","application/vnd.marvin.authorforcreation+json")]
        [Consumes("application/json", "application/vnd.marvin.authorforcreation+json")]
        [HttpPost(Name = "[action]")]
        public IActionResult CreateAuthor(AuthorForCreationDto authorForCreationDto)
        {
            var authorForCreation = _mapper.Map<Author>(authorForCreationDto);
            _courseLibraryRepository.AddAuthor(authorForCreation);
            _courseLibraryRepository.Save();
            var createdAuthor = _mapper.Map<AuthorDto>(authorForCreation);
            var links = GetLinksForAuthor(createdAuthor.Id);
            var linkedResourceToReturn = createdAuthor.ShapeData() as IDictionary<string, object>;
            linkedResourceToReturn.Add("links",links);
            return CreatedAtRoute("GetAuthor",new {authorId = createdAuthor.Id}, linkedResourceToReturn);
        }


       
        
        [HttpDelete("{authorId}", Name = "[action]")]
        public ActionResult<AuthorDto> DeleteAuthor(Guid authorId)
        {
            var authorEntity = _courseLibraryRepository.GetAuthor(authorId);

            if (authorEntity == null) return NotFound();
            
            _courseLibraryRepository.DeleteAuthor(authorEntity);
            _courseLibraryRepository.Save();
            return NoContent();
        }

        
        [HttpOptions]
        public ActionResult GetAuthorsOptions()
        {
            Response.Headers.Add("Allow", "POST,GET,OPTIONS");
            return Ok();
        }

        private string CreateAuthorsResourceUri(AuthorsResourceParameters authorsResourceParameters, ResourceUriType type = ResourceUriType.CurrentPage)
        {
            if (type == ResourceUriType.PreviousPage)
                authorsResourceParameters.PageNumber--;
            else if (type == ResourceUriType.NextPage) authorsResourceParameters.PageNumber++;

            return Url.Link(nameof(GetAuthors), authorsResourceParameters);
        }

        private IEnumerable<LinkDto> GetLinksForAuthor(Guid authorId, string fields = null)
        {
            var links = new List<LinkDto>();
            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                    new LinkDto(
                        Url.Link(nameof(GetAuthor),new {authorId}),
                        "self",
                        "GET"));
            }
            else
            {
                links.Add(
                    new LinkDto(
                        Url.Link(nameof(GetAuthor),new {authorId, fields}),
                        "self",
                        "GET"));
            }
            links.Add(
                new LinkDto(
                    Url.Link(nameof(DeleteAuthor),new {authorId}),
                    "delete_author",
                    "DELETE"));
            links.Add(
                new LinkDto(
                    Url.Link(nameof(CoursesController.CreateCourseForAuthor),new {authorId}),
                    "create_course_for_author",
                    "POST"));
            links.Add(
                new LinkDto(
                    Url.Link(nameof(CoursesController.GetCoursesForAuthor),new {authorId}),
                    "get_courses_for_author",
                    "GET"));
            return links;
        }
        
        private IEnumerable<LinkDto> GetLinksForAuthors(AuthorsResourceParameters authorsResourceParameters, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();
            
            links.Add(new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters), "self", "GET"));
            if (hasNext)
            {
                links.Add(new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.NextPage), "nextPage", "GET" ));
            }
            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage), "previouPage", "GET" ));
            }
            return links;
        }
    }
}