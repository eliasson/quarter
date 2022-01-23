namespace Quarter.Core.Exceptions
{
    public class NotFoundException : System.Exception
    {
        public NotFoundException(string reason) : base(reason)
        {
        }
    }
}