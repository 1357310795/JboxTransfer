using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace JboxTransfer.Server.Modules.DataWrapper
{
    /// <summary>
    /// Default implementation for <see cref="IDataWrapperExecutor"/>
    /// </summary>
    internal class DefaultWrapperExecutor : IDataWrapperExecutor
    {
        public DefaultWrapperExecutor()
        {
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual object WrapExceptionResult(object originalData, Exception exception, DataWrapperContext wrapperContext)
        {
            var httpContext = wrapperContext.HttpContext;
            var options = wrapperContext.WrapperOptions;

            //if (exception is Exception)
            //{
            //    //Given Ok Code for this exception.
            //    httpContext.Response.StatusCode = StatusCodes.Status200OK;
            //    //Given this exception to context.
            //    wrapperContext.Exception = exception as Exception;

            //    return WrapSuccesfullysResult(originalData ?? exception.Message, wrapperContext, true);
            //}

            var micakeException = exception as Exception;
            var result = new ApiResponse(StatusCodes.Status500InternalServerError,
                                           "InternalServerError",
                                           exception.Message);

            return result;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual object WrapSuccesfullysResult(object orignalData, DataWrapperContext wrapperContext)
        {
            if (orignalData is IResultDataWrapper)
                return orignalData;

            var options = wrapperContext.WrapperOptions;
            var statuCode = (wrapperContext.ResultData as ObjectResult)?.StatusCode ?? wrapperContext.HttpContext.Response.StatusCode;

            if (orignalData is ProblemDetails problemDetails)
            {
                return options.WrapProblemDetails ? new ApiResponse(problemDetails.Title)
                {
                    Result = problemDetails.Detail,
                    StatusCode = statuCode,
                    IsError = true,
                }
                : orignalData;
            }

            return new ApiResponse(orignalData) { StatusCode = statuCode };
        }
    }
}
