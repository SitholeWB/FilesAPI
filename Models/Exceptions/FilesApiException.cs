namespace Models;

public sealed class FilesApiException : Exception
{
    public FilesApiException(string message) : base(message)
    {
    }

    public FilesApiException(string message, Exception innerException) : base(message, innerException)
    {
    }
}