using System;

namespace CourseLibrary.API.Models.Authors
{
    public class AuthorCreationWithDateOfDeathDto: AuthorForCreationDto
    {
        public DateTimeOffset DateOfDeath { get; set; }
    }
}