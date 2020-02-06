using System;
using System.Collections.Generic;
using CourseLibrary.API.Models.Courses;

namespace CourseLibrary.API.Models.Authors
{
    public class AuthorForCreationDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTimeOffset DateOfBirth { get; set; }          

        public string MainCategory { get; set; }

        public ICollection<CourseForCreationDto> Course { get; set; } = new List<CourseForCreationDto>();

    }
}