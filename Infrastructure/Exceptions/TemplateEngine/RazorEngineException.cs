using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Exceptions.TemplateEngine
{
    public class RazorEngineException : Exception
    {
        public string Message { get; set; }

        public RazorEngineException(string message)
            : base(message) { }

        public RazorEngineException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
