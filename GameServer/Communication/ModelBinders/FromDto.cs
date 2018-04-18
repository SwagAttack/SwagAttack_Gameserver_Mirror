using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Communication.ModelBinders
{
    /// <summary>
    /// Dto to Model converter and binder. The model will bind to the "value" part of a
    /// "auth"/"val" request as in accordance with SwagAttack Standards
    /// </summary>
    public class FromDto : IModelBinder
    {
        private Type _binderType;

        private const string AuthenticationDelimeter = "auth";
        private const string ValueDelimeter = "val";

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            bindingContext.ValueProvider = new JObjectValueProvider(bindingContext.ActionContext);
            bindingContext.ActionContext.ActionDescriptor.Properties.Clear();

            var provider = bindingContext.ValueProvider;
            var modelState = bindingContext.ModelState;

            // If the request doesnt contain a value
            if (!provider.ContainsPrefix(ValueDelimeter))
            {
                modelState.AddModelError(ValueDelimeter, "Not a valid format");
                return Task.CompletedTask;
            }

            _binderType = bindingContext.ModelType;

            var resultTask = Task.Run(delegate
            {
                // Get value as raw json format
                var value = provider.GetValue(ValueDelimeter).FirstValue;

                // Construct object
                var result = JsonConvert.DeserializeObject(value, _binderType, new JsonSerializerSettings
                {
                    Error = (s, a) =>
                    {
                        var memberInfo = a.ErrorContext.Member.ToString();
                        var errorMsg = a.ErrorContext.Error.GetBaseException().Message;
                        bindingContext.ModelState.AddModelError(memberInfo, errorMsg);
                        a.ErrorContext.Handled = true;
                    }
                });

                return result;
            });

            // Add authenticantion to action-descriptor if request contains one
            if (provider.ContainsPrefix(AuthenticationDelimeter))
            {
                // The token is raw json format containing authentication information
                var authenticationToken = provider.GetValue(AuthenticationDelimeter).FirstValue;

                bool error = false;
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
                {
                    bindingContext.ActionContext.ActionDescriptor.Properties.Add(AuthenticationDelimeter, authenticationDictionary);
                }
            }

            var convertedResult = resultTask.Result;

            // No errors == succes
            if (bindingContext.ModelState.ErrorCount == 0)
                bindingContext.Result = ModelBindingResult.Success(convertedResult);

            return Task.CompletedTask;
        }
    }

    public class JObjectValueProvider : IValueProvider
    {
        private readonly Dictionary<string, string> _values;

        public JObjectValueProvider(ActionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _values = new Dictionary<string, string>();

            var inputStream = context.HttpContext.Request.Body;

            try
            {
                string inputString;
                using (var sr = new StreamReader(inputStream))
                {
                    inputString = sr.ReadToEnd();
                }

                var jsonObj = JObject.Parse(inputString);
                foreach (var entry in jsonObj)
                {
                    _values[entry.Key.ToLower()] = entry.Value.ToString();
                }

            }
            catch (Exception)
            {

            }
        }
        public bool ContainsPrefix(string prefix)
        {
            return _values.Keys.Contains(prefix);
        }

        public ValueProviderResult GetValue(string key)
        {
            if (_values.TryGetValue(key, out var value))
            {
                return new ValueProviderResult(value, CultureInfo.InvariantCulture);
            }

            return new ValueProviderResult(value, CultureInfo.InvariantCulture);
        }
    }
}