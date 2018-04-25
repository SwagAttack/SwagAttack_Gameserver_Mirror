using System;
using System.Runtime.CompilerServices;
using System.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Net.Http.Headers;

namespace Communication.ModelBinders.Attributes
{
    
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromSwagDtoAttribute : Attribute, IBindingSourceMetadata, IFromSwagDtoAttribute
    {
        public string Name { get; set; }
        public FromSwagDtoAttribute()
        {
            /* Doesn't add anything */
            Name = "_Verified";
        }
        public BindingSource BindingSource => BindingSource.Body;
    }
}