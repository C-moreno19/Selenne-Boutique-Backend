namespace SelenneApi.Exceptions;
public class UnauthorizedException : Exception
{ public UnauthorizedException(string msg = "No autorizado") : base(msg) {} }
