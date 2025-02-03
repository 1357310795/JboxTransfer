using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace JboxTransfer.Server.Modules.DataWrapper
{
    /// <summary>
    /// The options of wrap reponse data.
    /// </summary>
    public class DataWrapperOptions : IDataWrapperOptions
    {
        /// <summary>
        /// Shows the stack trace information in the responseException details.
        /// </summary>
        public bool IsDebug { get; set; } = false;

        /// <summary>
        /// When the http status code in this list, the result is not wrapped.
        /// Defatult:201,202,404
        /// 
        /// <see cref="StatusCodes"/>
        /// </summary>
        public List<int> NoWrapStatusCode { get; set; } = new List<int>() { 201, 202, 404 };

        /// <summary>
        /// <see cref="ProblemDetails"/> has a separate format in asp net core.
        /// If this values is true,ProblemDetails will be wrapped.So you will lost some error info.
        /// <para>
        ///     Default :false.
        /// </para>
        /// </summary>
        public bool WrapProblemDetails { get; set; } = false;
    }
}
