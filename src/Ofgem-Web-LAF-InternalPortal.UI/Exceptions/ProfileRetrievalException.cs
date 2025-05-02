namespace Ofgem.API.LAF.UserManagement.Application.Exceptions;

public class ProfileRetrievalException : Exception
{
    public ProfileRetrievalException(): base(BuildMessage) { }
    public ProfileRetrievalException(string message) : base(BuildMessage) { }
    public ProfileRetrievalException(string message, Exception innerException) : base(BuildMessage, innerException) { }
    private static string BuildMessage => "An issue occurred retrieving the data.";
}

