namespace GIGXR.Platform.GMS.Exceptions
{
    using System;

    public class GmsApiException : Exception
    {
        public GmsApiException()
        {
        }

        public GmsApiException(string message) : base(message)
        {
        }

        public GmsApiException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class GmsServerException : GmsApiException
    {
        public GmsServerException()
        {
        }

        public GmsServerException(string message) : base(message)
        {
        }

        public GmsServerException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}