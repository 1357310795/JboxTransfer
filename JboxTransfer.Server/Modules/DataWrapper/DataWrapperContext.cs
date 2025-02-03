﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JboxTransfer.Server.Modules.DataWrapper
{
    public class DataWrapperContext
    {
        /// <summary>
        ///  Gets <see cref="ActionDescriptor"/> for the selected action.
        ///  
        ///  It's maybe null
        /// </summary>
        public ActionDescriptor ActionDescriptor { get; set; }

        /// <summary>
        /// The data returned after the action is executed.
        /// <see cref="IActionResult"/>
        /// </summary>
        public IActionResult ResultData { get; set; }

        /// <summary>
        /// <see cref="HttpContext"/>
        /// </summary>
        public HttpContext HttpContext { get; set; }

        /// <summary>
        /// <see cref="ModelStateDictionary"/>
        /// 
        /// It's maybe null
        /// </summary>
        public ModelStateDictionary ModelState { get; set; }

        /// <summary>
        /// <see cref="DataWrapperOptions"/>
        /// </summary>
        public IDataWrapperOptions WrapperOptions { get; set; }

        /// <summary>
        /// <see cref="SoftlyMiCakeException"/> will not be handled as 500 errors and will be wrapped with successful results.
        /// It's maybe null.
        /// </summary>
        public Exception Exception { get; set; }

        public DataWrapperContext(IActionResult resultData,
                                  HttpContext httpContext,
                                  IDataWrapperOptions options,
                                  ActionDescriptor actionDescriptor = null,
                                  ModelStateDictionary modelstate = null,
                                  Exception exception = null)
        {
            ResultData = resultData;
            WrapperOptions = options;
            HttpContext = httpContext;
            ActionDescriptor = actionDescriptor;
            ModelState = modelstate;
            Exception = exception;
        }
    }
}
