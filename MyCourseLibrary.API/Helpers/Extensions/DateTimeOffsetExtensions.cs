using System;

namespace CourseLibrary.API.Helpers.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        public static int GetCurrentAge(this DateTimeOffset date, DateTimeOffset? dateOfDeath)
        {

            var dateToCalculate = dateOfDeath?.UtcDateTime ?? DateTime.Now;
            var age = dateToCalculate.Year - date.Year;

            if (dateToCalculate < date.AddYears(age))
            {
                age--;
            }
            return age;
        }
    }
}