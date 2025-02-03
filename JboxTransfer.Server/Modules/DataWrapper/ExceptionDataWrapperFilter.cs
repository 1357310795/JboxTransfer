using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace JboxTransfer.Server.Modules.DataWrapper
{
    internal class ExceptionDataWrapperFilter : IAsyncExceptionFilter
    {
        private readonly IDataWrapperExecutor _wrapperExecutor;
        private readonly IDataWrapperOptions _options;

        public ExceptionDataWrapperFilter(
            IDataWrapperOptions options,
            IDataWrapperExecutor wrapperExecutor)
        {
            _wrapperExecutor = wrapperExecutor;
            _options = options;
        }

        public Task OnExceptionAsync(ExceptionContext context)
        {
            if (context.ExceptionHandled)
                return Task.CompletedTask;

            //httpContext status code is always be 0.

            var wrapContext = new DataWrapperContext(context.Result,
                                                     context.HttpContext,
                                                     _options,
                                                     context.ActionDescriptor);

            var wrappedData = _wrapperExecutor.WrapExceptionResult(context.Result, context.Exception, wrapContext);
            context.ExceptionHandled = true;
            context.Result = new ObjectResult(wrappedData);

            //exceptionContext.Result != null || exceptionContext.Exception == null || exceptionContext.ExceptionHandled
            //Therefore, the exceptionContext.Result is not assigned here and is handled by middleware

            return Task.CompletedTask;
        }
    }
}
