using System.Collections.Generic;
using System.Linq;
using Application.Interfaces;
using Communication.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Communication.Filters
{
    /// <summary>
    ///     Provides authentication for requests.
    ///     Note that this requires <see cref="FromSwagDtoModelBinder" /> as the model binder
    /// </summary>
    public class AuthenticationAttribute : ActionFilterAttribute
    {
        private const string AuthenticationDelimeter = "auth";

        /// <summary>
        ///     Property name that controllers can use to fetch authentication token
        /// </summary>
        private const string ControllerAuthToken = "authtoken";

        private const string UserCredentials = "username";
        private const string KeyCredentials = "password";

        public ILoginManager LoginManager => Application.Managers.LoginManager.GetInstance();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Check if actionsDescriptions contains authentication
            var containsAuthentication =
                filterContext.ActionDescriptor.Properties.ContainsKey(AuthenticationDelimeter);

            if (containsAuthentication)
                if (
                    filterContext.ActionDescriptor.Properties[AuthenticationDelimeter] is Dictionary<string, string>
                        msg && msg.ContainsKey(UserCredentials) && msg.ContainsKey(KeyCredentials))
                {
                    // Fetch username and password
                    var user = msg[UserCredentials];
                    var pass = msg[KeyCredentials];

                    // If value type was a user compare username to passed username to make sure it's the same user
                    if (LoginManager.CheckLoginStatus(user, pass))
                    {
                        SetController(filterContext.Controller, msg);
                        return;
                    }
                }

            /* If we reach this point the request is unauthorized */
            filterContext.Result = new UnauthorizedResult();
        }

        private static void SetController(object controller, Dictionary<string, string> auth)
        {
            var controllerProperties = controller.GetType().GetProperties();

            var targetProp = controllerProperties.FirstOrDefault(prop =>
                prop.Name.ToLower().Contains(ControllerAuthToken) && prop.PropertyType == auth.GetType());

            if (targetProp != null) targetProp.SetValue(controller, auth);
        }
    }
}