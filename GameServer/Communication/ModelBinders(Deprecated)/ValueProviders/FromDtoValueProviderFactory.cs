using System;
using System.Threading.Tasks;
using Communication.JsonSerializerExtensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Communication.ModelBinders.ValueProviders
{
    public class FromDtoValueProviderFactory : IValueProviderFactory
    {
        public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var req = context.ActionContext.HttpContext.Request;
            return req.ContentType == "application/json" ? AddValueProviderAsync(context) : Task.CompletedTask;
        }

        private static Task AddValueProviderAsync(ValueProviderFactoryContext context)
        {
            var req = context.ActionContext.HttpContext.Request;
            
            object body = null;

            try
            {
                body = Utility.DeserializeStream(req.Body);
            }
            catch (Exception)
            {
                return Task.CompletedTask;
            }

            
            var valueProvider = new FromDtoValueProvider(body, BindingSource.Form);
            context.ValueProviders.Add(valueProvider);
            return Task.CompletedTask;
        }
    }
}