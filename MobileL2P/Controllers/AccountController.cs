using System.Threading.Tasks;
using MobileL2P.Models;
using MobileL2P.Services;
using Microsoft.AspNet.Identity;
using System.Threading;
using System;
using static MobileL2P.Services.Tools;
using L2PAPIClient;
using L2PAPIClient.DataModel;
using System.Web.Mvc;
using System.Web;

namespace MobileL2P.Controllers
{

    //Account Controller Responsible for Authorization and Login / Logout Process
    public class AccountController : BaseController
    {
        public AccountController() { }

        //
        // GET: /Account/Login
        [HttpGet]
        public async Task<ActionResult> Login(string returnUrl = null)
        {
            try
            {
                ViewData["ReturnUrl"] = returnUrl;
                //Check if user has already authorized
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (!Tools.hasCookieToken)
                {
                    //Init the Auth Process
                    UserAuth authenticator = new UserAuth();
                    ViewData["L2PURL"] = await authenticator.StartAuthenticationProcessAsync().ConfigureAwait(false);
                    HttpContext.Session.Add("authenticator", authenticator);
                    return View();
                }
                else
                {
                    return RedirectToAction(nameof(HomeController.MyCourses), "Home");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = ex.Message });
            }
        }

        //
        // POST: /Account/Login
        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            try
            {
                ViewData["ReturnUrl"] = returnUrl;
                // Wait for authentication
                bool done = false;
                while (!done)  // so far, not authenticated
                {
                    // Just wait 5 seconds - this is the recommended querying time for OAuth by ITC
                    Thread.Sleep(5000);

                    //Obj oriented approach for authorization process
                    UserAuth auth = new UserAuth();
                    if(HttpContext.Session["authenticator"] != null)
                    {
                        auth = HttpContext.Session["authenticator"] as UserAuth;
                    }

                    OAuthTokenRequestData reqData = await auth.CheckAuthenticationProgressAsync();

                    done = (auth.getState() == UserAuth.AuthenticationState.ACTIVE);
                    if (done)
                    {
                        //Add a Cookie to save tokens locally
                        HttpCookie accessCookie = new HttpCookie("CRTID", Encryptor.Encrypt(reqData.access_token));
                        HttpCookie refreshCookie = new HttpCookie("CRAID", Encryptor.Encrypt(reqData.refresh_token));
                        accessCookie.Expires = DateTime.MaxValue;
                        refreshCookie.Expires = DateTime.MaxValue;

                        Response.Cookies.Add(accessCookie);
                        Response.Cookies.Add(refreshCookie);

                        //Set logged in to true
                        HttpContext.Session.Add("LoggedIn", Tools.ObjectToByteArray(LoginStatus.LoggedIn));
                        return RedirectToAction(nameof(HomeController.MyCourses), "Home");
                    }
                }

                // If we got this far, something failed, redisplay form
                return View(model);
            }catch(Exception ex)
            {
                return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = ex.Message });
            }

        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [AllowAnonymous]
        public ActionResult LogOff()
        {
            HttpContext.Session.Add("LoggedIn", 0);
            SetCookiesToExpired();
            return RedirectToAction(nameof(AccountController.Login), "Account");
        }

        #region Helpers

        private void SetCookiesToExpired()
        {
            //Let Cookie Expire
            HttpCookie accessCookie = new HttpCookie("CRTID");
            HttpCookie refreshCookie = new HttpCookie("CRAID");
            accessCookie.Expires = DateTime.Now.AddDays(-1);
            refreshCookie.Expires = DateTime.Now.AddDays(-1);

            Response.Cookies.Set(accessCookie);
            Response.Cookies.Set(refreshCookie);
        }


        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }


        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.MyCourses), "Home");
            }
        }

        #endregion
    }
}
