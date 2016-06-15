using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProtonAnalytics.Models;
using RestSharp;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;

namespace ProtonAnalytics.Controllers
{
    public class UserController : ProtonAnalyticsController
    {
        public ActionResult LogIn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogIn(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = ExecuteApiCall("Account/Login", Method.POST, model);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                FormsAuthentication.SetAuthCookie(model.Email, model.RememberMe);
                HttpContext.User = new GenericPrincipal(new GenericIdentity(model.Email), null);
                return Redirect("~/");
            }
            else
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult LogOut()
        {
            var response = ExecuteApiCall("Account/LogOut", Method.POST);
            FormsAuthentication.SignOut();
            return Redirect("~/");
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var response = ExecuteApiCall("Account/Register", Method.POST, model);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    AddFlash("Registration complete. Please log in.");
                    return RedirectToAction("LogIn");
                }
                else
                {
                    var registrationError = this.GetWebApiErrorDetails(response);
                    ModelState.AddModelError("", registrationError);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
    }
}