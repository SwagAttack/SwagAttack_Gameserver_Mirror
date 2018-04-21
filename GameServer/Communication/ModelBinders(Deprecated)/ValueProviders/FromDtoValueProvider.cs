using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace Communication.ModelBinders.ValueProviders
{
    public class FromDtoValueProvider : BindingSourceValueProvider
    {
        private readonly JObject _bodyObject;

        public FromDtoValueProvider(object jsonBody, BindingSource bindingSource) : base(bindingSource)
        {
            _bodyObject = jsonBody as JObject;
        }

        public override bool ContainsPrefix(string prefix)
        {
            return _bodyObject.ContainsKey(prefix);
        }

        public override ValueProviderResult GetValue(string key)
        {
            return _bodyObject.TryGetValue(key, out var output)
                ? new ValueProviderResult(new StringValues(output.ToString()))
                : new ValueProviderResult(new StringValues(""));
        }
    }
}