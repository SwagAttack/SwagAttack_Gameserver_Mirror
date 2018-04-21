using System;
using System.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Communication.ModelBinders.Attributes
{
    
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromSwagDtoAttribute : Attribute, IBinderTypeProviderMetadata, IFromSwagDtoAttribute
    {
        public BindingSource BindingSource => null;
        public Type BinderType => typeof(FromSwagDtoModelBinder);
    }
}