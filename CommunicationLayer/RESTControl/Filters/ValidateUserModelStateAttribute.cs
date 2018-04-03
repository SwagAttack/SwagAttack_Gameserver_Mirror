using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RESTControl.Filters
{
    public class ValidateUserModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = new Dictionary<string, string>();

                foreach (var state in context.ModelState)
                {
                    foreach (var err in state.Value.Errors)
                    {
                        errors.Add(state.Key, err.Exception.GetBaseException().Message);
                    }
                }

                context.Result = new BadRequestObjectResult(errors);
            }
        }
    }
}