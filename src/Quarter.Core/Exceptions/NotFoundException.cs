namespace Quarter.Core.Exceptions;

public class NotFoundException(string reason) : System.Exception(reason);
