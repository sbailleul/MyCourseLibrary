using System;
using System.Collections.Generic;
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models.Courses;
using CourseLibrary.API.Services.Interfaces;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId}/[controller]")]
    // [ResponseCache(CacheProfileName = "240SecondCachedProfile")]
    [HttpCacheExpiration(CacheLocation = CacheLocation.Public)]
    [HttpCacheValidation(MustRevalidate = true)]
    public class CoursesController: ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public CoursesController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository ??
                                       throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet(Name = "[action]")]
        [HttpCacheExpiration(MaxAge = 1000, CacheLocation = CacheLocation.Public)]
        [HttpCacheValidation(MustRevalidate = false)]
        public ActionResult<IEnumerable<CourseDto>> GetCoursesForAuthor(Guid authorId)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId)) return NotFound();

            var courses = _courseLibraryRepository.GetCourses(authorId);
            
            if (courses == null) return NotFound();

            return Ok(_mapper.Map<IEnumerable<CourseDto>>(courses));
        }
        
        [HttpGet("{courseId}",Name = "[action]")]
        // [ResponseCache(Duration = 120)]
        public ActionResult<IEnumerable<CourseDto>> GetCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId)) return NotFound();

            var course = _courseLibraryRepository.GetCourse(authorId, courseId);
            
            if (course == null) return NotFound();

            return Ok(_mapper.Map<CourseDto>(course));
        }
        
        [HttpPost(Name = "[action]")]
        public ActionResult<IEnumerable<CourseDto>> CreateCourseForAuthor(Guid authorId, CourseForCreationDto courseForCreationDto)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId)) return NotFound();

            
            var courseForCreation = _mapper.Map<Course>(courseForCreationDto);
            _courseLibraryRepository.AddCourse(authorId, courseForCreation);
            _courseLibraryRepository.Save();    
            return CreatedAtRoute("GetCourseForAuthor",new { authorId, courseId = courseForCreation.Id}, _mapper.Map<CourseDto>(courseForCreation));
        }

        [HttpPut("{courseId}")]
        public IActionResult UpdateCourseForAuthor(Guid authorId, Guid courseId, CourseForUpdateDto courseForUpdateDto)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId)) return NotFound();
            var courseEntity = _courseLibraryRepository.GetCourse(authorId, courseId);
            if (courseEntity == null)
            {
                var courseForCreation = _mapper.Map<Course>(courseForUpdateDto);
                courseForCreation.Id = courseId;
                _courseLibraryRepository.AddCourse(authorId, courseForCreation);
                _courseLibraryRepository.Save();
                return CreatedAtRoute("GetCourseForAuthor",new { authorId, courseId = courseForCreation.Id}, _mapper.Map<CourseDto>(courseForCreation));
            }

            _mapper.Map(courseForUpdateDto, courseEntity);
            _courseLibraryRepository.UpdateCourse(courseEntity);
            _courseLibraryRepository.Save();
            return NoContent();
        }
        
        [HttpDelete("{courseId}")]
        public IActionResult DeleteCourseForAuthor(Guid authorId, Guid courseId, CourseForUpdateDto courseForUpdateDto)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId)) return NotFound();
            var courseEntity = _courseLibraryRepository.GetCourse(authorId, courseId);
            if (courseEntity == null) return NotFound();
            _courseLibraryRepository.DeleteCourse(courseEntity);
            _courseLibraryRepository.Save();
            return NoContent();
        }
        
        
        [HttpPatch("{courseId}")]
        public IActionResult PartiallyUpdateCourseForAuthor(Guid authorId, Guid courseId, JsonPatchDocument<CourseForUpdateDto> patchDoc)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId)) return NotFound();
            var courseEntity = _courseLibraryRepository.GetCourse(authorId, courseId);
            if (courseEntity == null)
            {
                var courseDto = new CourseForUpdateDto();
                patchDoc.ApplyTo(courseDto, ModelState);

                if (!TryValidateModel(courseDto))
                {
                    return ValidationProblem();
                }
                courseEntity = _mapper.Map<Course>(courseDto);
                courseEntity.Id = courseId;
                _courseLibraryRepository.AddCourse(authorId,courseEntity);
                _courseLibraryRepository.Save();
                var courseToReturn = _mapper.Map<CourseDto>(courseEntity);
                return CreatedAtRoute("GetCourseForAuthor",new { authorId, courseId = courseToReturn.Id}, _mapper.Map<CourseDto>(courseToReturn));
            }
            
            var courseForPatch = _mapper.Map<CourseForUpdateDto>(courseEntity);

            patchDoc.ApplyTo(courseForPatch,ModelState);

            if (!TryValidateModel(courseForPatch))
            {
                return ValidationProblem();
            }
            _mapper.Map(courseForPatch, courseEntity);
            _courseLibraryRepository.UpdateCourse(courseEntity);
            _courseLibraryRepository.Save();
            return NoContent();
        }

        public override ActionResult ValidationProblem([ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices.GetRequiredService<IOptions<ApiBehaviorOptions>>();
            return (ActionResult) options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }
    }
}