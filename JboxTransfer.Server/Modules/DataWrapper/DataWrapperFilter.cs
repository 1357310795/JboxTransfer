using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using JboxTransfer.Server.Modules.DataWrapper;

namespace JboxTransfer.Server.Modules.DataWrapper
{
    public class DataWrapperFilter : IAsyncResultFilter
    {
        private readonly IDataWrapperExecutor _wrapperExecutor;
        private readonly IDataWrapperOptions _options;

        public DataWrapperFilter(
            IDataWrapperOptions options,
            IDataWrapperExecutor wrapperExecutor)
        {
            _wrapperExecutor = wrapperExecutor;
            _options = options;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult objectResult)
            {
                var statusCode = objectResult.StatusCode ?? context.HttpContext.Response.StatusCode;

                if (objectResult.DeclaredType != null && objectResult.DeclaredType.IsGenericType && objectResult.DeclaredType?.GetGenericTypeDefinition() == typeof(ApiResponse))
                {
                    return;
                }
                if (!_options.NoWrapStatusCode.Any(s => s == statusCode))
                {
                    var wrappContext = new DataWrapperContext(context.Result,
                                                              context.HttpContext,
                                                              _options,
                                                              context.ActionDescriptor);

                    var wrappedData = _wrapperExecutor.WrapSuccesfullysResult(objectResult.Value, wrappContext);
                    objectResult.Value = wrappedData;
                    objectResult.DeclaredType = wrappedData.GetType();
                }
            }

            await next();
        }
    }
}
