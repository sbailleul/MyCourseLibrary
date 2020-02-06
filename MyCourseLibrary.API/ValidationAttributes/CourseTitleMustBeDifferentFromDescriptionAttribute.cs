using System.ComponentModel.DataAnnotations;
using CourseLibrary.API.Models.Courses;

namespace CourseLibrary.API.ValidationAttributes
{
    public class CourseTitleMustBeDifferentFromDescriptionAttribute: ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var courseForCreationDto = (CourseForManipulationDto) validationContext.ObjectInstance;
            if (courseForCreationDto.Title == courseForCreationDto.Description)
            {
                return new ValidationResult(ErrorMessage, new[] {nameof(CourseForManipulationDto)});
            }
            return ValidationResult.Success;
        }
    }
}