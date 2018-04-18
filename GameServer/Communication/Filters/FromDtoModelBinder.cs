using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Newtonsoft.Json;

namespace Communication.Filters
{
    /// <summary>
    /// Dto to Model converter and binder. The model will bind to the "value" part of a
    /// "auth"/"val" request as in accordance with SwagAttack Standards
    /// </summary>
    public class FromDtoModelBinder : IModelBinder
    {
        public Type BinderType;

        private string _authenticationDelimeter = "auth";
        private string _valueDelimeter = "val";

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            bindingContext.ValueProvider = new JObjectValueProvider(bindingContext.ActionContext);
            bindingContext.ActionContext.ActionDescriptor.Properties.Clear();

            var provider = bindingContext.ValueProvider;
            var modelState = bindingContext.ModelState;

            // If the request doesnt contain a value
            if (!provider.ContainsPrefix(_valueDelimeter))
            {
                modelState.AddModelError(_valueDelimeter, "Not a valid format");
                return Task.CompletedTask;
            }
                
            BinderType = bindingContext.ModelType;

            // Add authenticantion to action-descriptor if request contains one
            if (provider.ContainsPrefix(_authenticationDelimeter))
            {
                // The token is raw json format containing authentication information
                var authenticationToken = provider.GetValue(_authenticationDelimeter).FirstValue;
                if (!bindingContext.ActionContext.ActionDescriptor.Properties.ContainsKey(_authenticationDelimeter))
                {
                    bindingContext.ActionContext.ActionDescriptor.Properties.Add(_authenticationDelimeter, authenticationToken);
                }                                      
            }
            
            // Get value as raw json format
            var value = provider.GetValue(_valueDelimeter).FirstValue;

            // Construct object
            var result = JsonConvert.DeserializeObject(value, BinderType, new JsonSerializerSettings
            {
                Error = (s, a) =>
                {
                    var memberInfo = a.ErrorContext.Member.ToString();
                    var errorMsg = a.ErrorContext.Error.GetBaseException().Message;
                    bindingContext.ModelState.AddModelError(memberInfo, errorMsg);
                    a.ErrorContext.Handled = true;
                }
            });        

            // No errors == succes
            if(bindingContext.ModelState.ErrorCount == 0)
                bindingContext.Result = ModelBindingResult.Success(result);
           
            return Task.CompletedTask;
        }       
    }

    public class JObjectValueProvider : IValueProvider
    {
        private readonly Dictionary<string, string> _values;

        public JObjectValueProvider(ActionContext context)
        {
            if(context == null)
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