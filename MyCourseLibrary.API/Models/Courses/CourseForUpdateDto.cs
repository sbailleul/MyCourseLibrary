using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models.Courses
{
    public class CourseForUpdateDto:CourseForManipulationDto
    {
        [Required(ErrorMessage = "You should fill out description")]
        public override string Description { get; set; }
    }
} 