using System;
using System.Collections.Generic;
using System.Linq;
using Communication.JsonConverter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Rewrite.Internal;
using Newtonsoft.Json.Linq;

namespace Communication.Filters
{
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        public Type Pattern = null;
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //var msg = context.ActionArguments.Values.ToList()[0] as JObject;
            //var test = DtoConverter.ConvertToInstance(msg, Pattern);

            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }
    }

    public class ExceptionReader
    {
        public static Dictionary<string, string> ReadInnerExceptions(Exception e)
        {
            var dic = new Dictionary<string, string>();
            dic.Add(e.Source, e.InnerException.Message);
            return dic;
        }
    }
}