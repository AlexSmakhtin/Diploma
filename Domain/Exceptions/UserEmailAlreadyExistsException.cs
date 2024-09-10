namespace Domain.Exceptions;

public class UserEmailAlreadyExistsException : Exception
{
    public UserEmailAlreadyExistsException(string message) : base(message)
    {
    }
}