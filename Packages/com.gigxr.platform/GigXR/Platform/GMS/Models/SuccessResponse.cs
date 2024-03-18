using System;

namespace GIGXR.GMS.Models
{
    [Serializable]
    public class SuccessResponse<T> where T : class
    {
        public T data;
    }
}