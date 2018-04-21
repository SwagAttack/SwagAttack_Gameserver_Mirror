using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace Communication.ModelBinders
{
    /// <summary>
    ///     Dto to Model converter and binder. The model will bind to the "value" part of a
    ///     "auth"/"val" request as in accordance with SwagAttack Standards
    /// </summary>
    public class FromSwagDtoModelBinder : IModelBinder
    {
        private const string AuthenticationDelimeter = "auth";
        private const string ValueDelimeter = "val";

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            bindingContext.ActionContext.ActionDescriptor.Properties.Clear();

            var provider = bindingContext.ValueProvider;
            var modelState = bindingContext.ModelState;

            // If the request doesnt contain a value
            if (!provider.ContainsPrefix(ValueDelimeter))
            {
                modelState.AddModelError(ValueDelimeter, "Not a valid format");
                return;
            }

            var resultTask = Task.Run(() =>
            {
                // Get value as raw json format
                var value = provider.GetValue(ValueDelimeter).FirstValue;

                // Construct object
                var result = JsonConvert.DeserializeObject(value, bindingContext.ModelType, new JsonSerializerSettings
                {
                    Error = (s, a) =>
                    {
                        var memberInfo = a.ErrorContext.Member.ToString();
                        var errorMsg = a.ErrorContext.Error.GetBaseException().Message;
                        bindingContext.ModelState.AddModelError(memberInfo, errorMsg);
                        a.ErrorContext.Handled = true;
                    },
                    DefaultValueHandling = DefaultValueHandling.Populate /* Makes sure we throw OUR exceptions */
                });

                return result;
            });

            // Add authenticantion to action-descriptor if request contains one
            if (provider.ContainsPrefix(AuthenticationDelimeter))
            {
                // The token is raw json format containing authentication information
                var authenticationToken = provider.GetValue(AuthenticationDelimeter).FirstValue;

                var error = false;
                var authenticationDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    authenticationToken, new JsonSerializerSettings
                    {
                        Error = (s, a) =>
                        {
                            error = true;
                            a.ErrorContext.Handled = true;
                        }
                    });

                if (!error)
                    bindingContext.ActionContext.ActionDescriptor.Properties.Add(AuthenticationDelimeter,
                        authenticationDictionary);
            }

            var convertedResult = await resultTask;

            // No errors == succes
            if (bindingContext.ModelState.ErrorCount == 0)
                bindingContext.Result = ModelBindingResult.Success(convertedResult);
        }
    }
}