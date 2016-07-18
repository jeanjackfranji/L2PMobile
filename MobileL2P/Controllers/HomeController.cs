using L2PAPIClient.DataModel;
using L2PAPIClient;
using System;
using System.Threading.Tasks;
using MobileL2P.Services;
using static MobileL2P.Services.Tools;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using System.Web;
using MobileL2P.Helpers;

namespace MobileL2P.Controllers
{
    public class HomeController : BaseController
    {

        public async Task<ActionResult> MyCourses(String semId)
        {
            try
            {
                LoginStatus lStatus = LoginStatus.Waiting;
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (HttpContext.Session["LoggedIn"] != null)
                    lStatus = (LoginStatus)(HttpContext.Session["LoggedIn"]);

                if (lStatus == LoginStatus.LoggedIn)
                {
                    //remove previously save course id
                    HttpContext.Session.Remove("CourseId");
                    if (Tools.hasCookieToken)
                    {
                        UserAuth auth = new UserAuth();
                        if (HttpContext.Session["authenticator"] != null)
                        {
                            auth = HttpContext.Session["authenticator"] as UserAuth;
                        }
                        await auth.CheckAccessTokenAsync();
                    }

                    L2PCourseInfoSetData result = await L2PAPIClient.api.Calls.L2PviewAllCourseInfoAsync();
                    L2PCourseInfoSetData resultBySemester = null;
                    if (semId != null)
                    {
                        ViewData["chosenSemesterCode"] = semId;
                        resultBySemester = await L2PAPIClient.api.Calls.L2PviewAllCourseIfoBySemesterAsync(semId);
                    }
                    else
                    {
                        resultBySemester = await L2PAPIClient.api.Calls.L2PviewAllCourseInfoByCurrentSemester();
                    }
                    ViewData["semestersList"] = result.dataset;
                    ViewData["currentSemesterCourses"] = resultBySemester.dataset;
                    return View();
                }
                else
                {
                    if (Tools.hasCookieToken && !String.IsNullOrEmpty(Config.getRefreshToken()))
                    {
                        await AuthenticationManager.CheckAccessTokenAsync();
                    }
                }
            }
            catch(Exception ex)
            {
                //Let Cookie Expire
                //Let Cookie Expire
                HttpCookie accessCookie = new HttpCookie("CRTID");
                HttpCookie refreshCookie = new HttpCookie("CRAID");
                accessCookie.Expires = DateTime.Now.AddDays(-1);
                refreshCookie.Expires = DateTime.Now.AddDays(-1);

                Response.Cookies.Set(accessCookie);
                Response.Cookies.Set(refreshCookie);

                return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = ex.Message });
            }
            return RedirectToAction(nameof(AccountController.Login), "Account");
        }

        public async Task<ActionResult> Calendar()
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext))
                {
                    //remove previously save course id
                    HttpContext.Session.Remove("CourseId");

                    List<L2PCourseEvent> courseEvents = new List<L2PCourseEvent>();
                    L2PCourseInfoSetData courses = await L2PAPIClient.api.Calls.L2PviewAllCourseInfoAsync();

                    if(courses.dataset != null)
                    {
                        foreach(L2PCourseInfoData course in courses.dataset)
                        {
                            L2PCourseEventList result = await L2PAPIClient.api.Calls.L2PviewCourseEvents(course.uniqueid);
                            if (result.dataSet != null)
                            {
                                courseEvents.AddRange(result.dataSet);
                            }
                        }
                    }
                    ViewData["courseEventsList"] = courseEvents;
                    return View();
                }
                else
                {
                    return RedirectToAction(nameof(AccountController.Login), "Account");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = ex.Message });
            }
        }

        public ActionResult About()
        {
            return View();
        }
        
        public ActionResult Error(string error)
        {
            if(error != null && error.Contains("401 (Unauthorized)"))
            {
                error = "The page you are trying to visit does not exist. Please choose from the options below.";
            }
            ViewData["error"] = error;
            return View("~/Views/Shared/Error.cshtml");
        }

        public ActionResult Language(string culture = "EN")
        {
            // Validate input
            culture = CultureHelper.GetImplementedCulture(culture);

            // Save culture in a cookie
            HttpCookie cookie = Request.Cookies["_culture"];
            if (cookie != null)
                cookie.Value = culture;   // update cookie value
            else
            {

                cookie = new HttpCookie("_culture");
                cookie.Value = culture;
                cookie.Expires = DateTime.Now.AddYears(1);
            }
            Response.Cookies.Add(cookie);

            return RedirectToAction(nameof(AccountController.Login), "Account");
        }

    }
}
