namespace Engage360Suite.Infrastructure.Exceptions
{
    /// <summary>
    /// Thrown when sending a message to WhatsApp via Pingerbot fails.
    /// </summary>
    public class WhatsAppException : Exception
    {
        public WhatsAppException()
        { }

        public WhatsAppException(string message)
            : base(message)
        { }

        public WhatsAppException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
