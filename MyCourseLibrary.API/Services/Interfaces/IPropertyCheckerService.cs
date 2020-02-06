namespace CourseLibrary.API.Services.Interfaces
{
    public interface IPropertyCheckerService
    {
        bool TypeHasProperty<T>(string fields);
    }
}