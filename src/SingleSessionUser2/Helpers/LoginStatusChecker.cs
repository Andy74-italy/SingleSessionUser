using System.Web;
using System.Web.Mvc;

namespace SingleSessionUser2.Helpers
{
    /// <summary>
    /// In order to ensure that the single access to be checked for each request, 
    /// a new decorator (or attribute) is implemented to add to the controllrrs/actions 
    /// where you want to activate the check.
    /// </summary>
    public class LoginStatusChecker : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            /// The check verifies that the value in Session coincides with the cookie 
            /// set in the user's browser. If these differ, the user is logged out (in 
            /// this case simulating the deletion of the "User" session variable, but 
            /// in this specific case using the relative manager). This is because, by
            /// initializing an identical session for two connected devices, they will
            /// share the same session variables, but not the same cookies. In this way
            /// the session variable will coincide with the user who logged in last, as
            /// it is regenerated at each generation of a new SessionID.
            if (!string.IsNullOrEmpty(HttpContext.Current.Session["User"]?.ToString())
                && HttpContext.Current.Session["Random-S"]?.ToString() != HttpContext.Current.Request.Cookies["Random-C"]?.Value)
            {
                HttpContext.Current.Session.Remove("User");
                HttpContext.Current.Session.Abandon();
                CustomSessionIDManager.RegenerateSessionId(HttpContext.Current);
            }
        }
    }
}