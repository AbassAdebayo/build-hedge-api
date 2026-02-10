using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Exceptions.TemplateEngine
{
    public class RazorEngineException : Exception
    {
#pragma warning disable CS0114
        public string Message { get; set; }
#pragma warning restore CS0114

        public RazorEngineException(string message)
            : base(message) { }

        public RazorEngineException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
