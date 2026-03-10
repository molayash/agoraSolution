namespace CRM.Application.Common.Result;

public class ServiceResult
{
    public bool Success { get; private set; }
    public string Message { get; private set; }

    private ServiceResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public static ServiceResult Ok(string message = "Success") => new(true, message);
    public static ServiceResult Fail(string message) => new(false, message);
    public static ServiceResult NotFound(string message = "Record not found.") => new(false, message);
    public static ServiceResult Duplicate(string message = "Record already exists.") => new(false, message);
}
