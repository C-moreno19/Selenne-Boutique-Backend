namespace SelenneApi.Exceptions;
public class ForbiddenException : Exception
{ public ForbiddenException(string msg = "Permisos insuficientes") : base(msg) {} }
