using System.ComponentModel.DataAnnotations;
using CourseLibrary.API.ValidationAttributes;

namespace CourseLibrary.API.Models.Courses
{
    [CourseTitleMustBeDifferentFromDescription(ErrorMessage = "Title must be different from description")]
    public abstract class CourseForManipulationDto
    {
        [Required(ErrorMessage = "You should fill out title")]
        [MaxLength(100, ErrorMessage = "You should provide title lesser than 100 characters")]
        public string Title { get; set; } 
        
        [MaxLength(1500, ErrorMessage = "You should provide description lesser than 1500 characters")]
        public virtual string Description { get; set; }
    }
}