namespace IntegrationServices.Application.Common;

public sealed class ApplicationException : Exception
{
    public ApplicationException(string message) : base(message)
    {
    }
}