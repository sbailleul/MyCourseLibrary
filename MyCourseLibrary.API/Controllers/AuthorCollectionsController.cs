using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models.Authors;
using CourseLibrary.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorCollectionsController: ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public AuthorCollectionsController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository ??
                                       throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        
        [HttpPost]
        public ActionResult<IEnumerable<AuthorDto>> CreateAuthorCollection(
            IEnumerable<AuthorForCreationDto> authorCollection)
        {
            var authorEntities = _mapper.Map<IEnumerable<Author>>(authorCollection);
            foreach (var authorEntity in authorEntities)
            {
                _courseLibraryRepository.AddAuthor(authorEntity);
            }

            _courseLibraryRepository.Save();
            var authorsDto = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            var ids = string.Join(',',authorEntities.Select(a => a.Id).ToArray());

            return CreatedAtRoute("GetAuthorCollection", new {ids = ids}, authorsDto);
        }

        [HttpGet("({ids})", Name = "[action]")]
        public ActionResult<IEnumerable<AuthorDto>> GetAuthorCollection([FromRoute] [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null) return BadRequest();

            var authorEntities = _courseLibraryRepository.GetAuthors(ids);

            if (ids.Count() != authorEntities.Count()) return NotFound();

            var authorsDto = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);

            return Ok(authorsDto);
        }
        
    }
}