using System.Threading.Tasks;
using Communication.JsonSerializerExtensions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;

namespace Communication.Formatters
{
    public class JsonInputFormatter : InputFormatter
    {
        public override bool CanRead(InputFormatterContext context)
        {
            if (context.HttpContext.Request.ContentType == "application/json")
                return true;

            return false;
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var req = context.HttpContext.Request;
            var ms = context.ModelState;
            var type = context.ModelType;

            object result = null;

            var serializer = new JsonSerializer();
            serializer.Error += (s, a) =>
            {
                var memberInfo = a.ErrorContext.Member.ToString();
                var errorMsg = a.ErrorContext.Error.GetBaseException().Message;
                ms.AddModelError(memberInfo, errorMsg);
                a.ErrorContext.Handled = true;
            };

            serializer.DefaultValueHandling = DefaultValueHandling.Populate;

            result = Utility.DeserializeStream(req.Body, type, serializer);

            // If errors abort
            if (ms.ErrorCount != 0)
                return await InputFormatterResult.FailureAsync();

            return await InputFormatterResult.SuccessAsync(result);
        }
    }
}