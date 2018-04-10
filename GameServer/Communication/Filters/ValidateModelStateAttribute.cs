using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Communication.Filters
{
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = new Dictionary<string, string>();

                foreach (var state in context.ModelState)
                {
                      errors.Add(state.Key, state.Value.Errors[0].Exception.GetBaseException().Message);
                }

                context.Result = new BadRequestObjectResult(errors);
            }
        }
    }
}