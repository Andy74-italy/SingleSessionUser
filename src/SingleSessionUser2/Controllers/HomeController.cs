using System;
using System.Web;
using System.Web.Mvc;
using SingleSessionUser2.Helpers;
using SingleSessionUser2.Models;

namespace SingleSessionUser2.Controllers
{
    [LoginStatusChecker]
    public class HomeController : Controller
    {
        public HttpContext Context { get { return System.Web.HttpContext.Current; } }
        public ActionResult Index()
        {

            return View(new HomeModel() { SessionID = Session.SessionID
                                        , User = (string)Session["User"]
                                        , Rnd_Value_Session = Session["Random-S"]?.ToString()
                                        , Rnd_Value_Cookie = Request.Cookies["Random-C"]?.Value });
        }

        [HttpPost]
        public ActionResult Index(string txtUsername)
        {
            var sessionID = string.Empty;
            // The logged in user is identified by a session variable set with his name.
            if (Session["User"] == null || string.IsNullOrEmpty((string)Session["User"]))
            {
                /// User login is simulated and an additional session variable is set that 
                /// identifies the relationship between the client and the server. 
                /// The client will receive the equivalent value through a cookie. 
                /// As long as these two variables are the same, the user will be able to 
                /// stay connected, when the two values differ, the user will be automatically 
                /// logged out.
                Session.Add("User", txtUsername);
                var randomValue = new Random((int)DateTime.Now.Ticks).Next();
                Session.Add("Random-S", randomValue);

                HttpCookie sameSiteCookie = new HttpCookie("Random-C");
                sameSiteCookie.Value = randomValue.ToString();
                sameSiteCookie.Secure = true;
                sameSiteCookie.HttpOnly = true;
                sameSiteCookie.SameSite = SameSiteMode.None;
                Response.Cookies.Add(sameSiteCookie);

                /// The session ID is regenerated, with the one that uniquely identifies 
                /// the specific user.
                sessionID = CustomSessionIDManager.RegenerateSessionId(Context);
            }
            else
            {
                /// The action called in post with the logged in user identifies the logout action.
                Session.Remove("User");
                Session.Abandon();
            }
            return View(new HomeModel() { SessionID = sessionID
                                        , User = (string)Session["User"]
                                        , Rnd_Value_Session = (string)Session["Random-S"]?.ToString()
                                        , Rnd_Value_Cookie = Response.Cookies["Random-C"]?.Value });
        }
    }
}