using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.TemplateEngine
{
    public interface IRazorEngine
    {
        Task<string> ParseAsync<TModel>(string viewName, TModel model);
    }
}
}
