namespace SelenneApi.Exceptions;
public class AppException : Exception
{ public int StatusCode { get; } = 400;
  public AppException(string msg, int code = 400) : base(msg) { StatusCode = code; } }
