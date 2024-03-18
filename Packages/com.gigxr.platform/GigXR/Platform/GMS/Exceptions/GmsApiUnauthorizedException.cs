namespace GIGXR.Platform.GMS.Exceptions
{
    using System;

    public class GmsApiUnauthorizedException : GmsApiException
    {
        public GmsApiUnauthorizedException()
        {
        }

        public GmsApiUnauthorizedException(string message) : base(message)
        {
        }

        public GmsApiUnauthorizedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}