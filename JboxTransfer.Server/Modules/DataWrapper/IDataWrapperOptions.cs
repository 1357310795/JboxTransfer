namespace JboxTransfer.Server.Modules.DataWrapper
{
    public interface IDataWrapperOptions
    {
        /// <summary>
        /// Shows the stack trace information in the responseException details.
        /// </summary>
        public bool IsDebug { get; set; }

        /// <summary>
        /// When the http status code in this list, the result is not wrapped.
        /// Defatult:201,202,404
        /// 
        /// <see cref="StatusCodes"/>
        /// </summary>
        public List<int> NoWrapStatusCode { get; set; }

        /// <summary>
        /// <see cref="ProblemDetails"/> has a separate format in asp net core.
        /// If this values is true,ProblemDetails will be wrapped.So you will lost some error info.
        /// <para>
        ///     Default :false.
        /// </para>
        /// </summary>
        public bool WrapProblemDetails { get; set; }
    }
}
