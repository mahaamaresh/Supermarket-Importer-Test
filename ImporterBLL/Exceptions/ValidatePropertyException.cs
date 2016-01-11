using System;

namespace ImporterBLL.Exceptions
{
    public class ValidatePropertyException: Exception
    {
        public ValidatePropertyException(string message) : base(message) { }
    }
}
