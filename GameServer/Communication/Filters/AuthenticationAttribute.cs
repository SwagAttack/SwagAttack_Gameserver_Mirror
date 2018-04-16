using Application.Interfaces;
using Application.Managers;
using Application.Misc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Communication.Filters
{
    public class AuthenticationAttribute : ActionFilterAttribute
    {
        private string _wrongUsername = null;
        private string _wrongPassword = null; 
        private readonly ILoginManager _manager;
        public AuthenticationAttribute()
        {

             _manager = LoginManager.GetInstance(new CountDownTimer());
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var user = filterContext.RouteData.Values["username"] as string ?? _wrongUsername;
            var pass = filterContext.RouteData.Values["password"] as string ?? _wrongPassword;

            if (_manager.CheckLoginStatus(user))
            {

            }
            else
            {
                filterContext.Result = new UnauthorizedResult();
            }
        }
    }
}