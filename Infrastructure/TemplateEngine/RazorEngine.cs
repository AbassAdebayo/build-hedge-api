using Domain.TemplateEngine;
using Infrastructure.Exceptions.TemplateEngine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.TemplateEngine
{
    public class RazorEngine(IRazorViewEngine viewEngine, 
        ITempDataProvider tempDataProvider, IHttpContextAccessor accessor,
        ILogger<RazorEngine> logger) : IRazorEngine
    {
        private readonly IRazorViewEngine _viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
        private readonly ITempDataProvider _tempDataProvider = tempDataProvider ?? throw new ArgumentNullException(nameof(tempDataProvider));
        private readonly HttpContext _context = accessor.HttpContext ?? throw new ArgumentNullException(nameof(accessor));
        private readonly ILogger<IRazorEngine> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<string> ParseAsync<TModel>(string viewName, TModel model)
        {
            try
            {
                var actionContext = GetActionContext();
                var view = FindView(actionContext, viewName);

                await using var writer = new StringWriter();
                var viewContext = new ViewContext(
                    actionContext,
                    view,
                    new ViewDataDictionary<TModel>(
                        new EmptyModelMetadataProvider(),
                        new ModelStateDictionary())
                    { Model = model },
                    new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                    writer,
                    new HtmlHelperOptions())
                { RouteData = _context.GetRouteData() };

                await view.RenderAsync(viewContext);

                return writer.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new RazorEngineException(ex.Message, ex);
            }
        }

        private IView FindView(ActionContext actionContext, string viewName)
        {
            var getViewResult = _viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
            if (getViewResult.Success)
            {
                return getViewResult.View;
            }

            var findViewResult = _viewEngine.FindView(actionContext, viewName, isMainPage: true);
            if (findViewResult.Success)
            {
                return findViewResult.View;
            }

            var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
            var errorMessage = string.Join(
                Environment.NewLine,
                new[] { $"Unable to find view '{viewName}'. The following locations were searched:" }.Concat(searchedLocations)); ;

            throw new InvalidOperationException(errorMessage);
        }

        private ActionContext GetActionContext()
        {
            return new ActionContext(_context, new RouteData(), new ActionDescriptor());
        }
    }
}
