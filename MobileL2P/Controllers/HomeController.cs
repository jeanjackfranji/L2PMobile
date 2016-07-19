using L2PAPIClient.DataModel;
using L2PAPIClient;
using System;
using System.Threading.Tasks;
using MobileL2P.Services;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web;
using MobileL2P.Helpers;

namespace MobileL2P.Controllers
{
    public class HomeController : BaseController
    {

        //GET Method to show the list of My Courses
        [HttpGet]
        public async Task<ActionResult> MyCourses(String semId)
        {
            try
            {
                // This method must be used before every L2P API call to check if user is authorized
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                UserAuth auth = null;
                if (HttpContext.Session["authenticator"] != null)
                {
                    auth = HttpContext.Session["authenticator"] as UserAuth;
                }
                if (await Tools.isUserLoggedInAndAPIActive(HttpContext, auth))
                {
                    //remove previously save course id
                    HttpContext.Session.Remove("CourseId");
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

        //GET Method to show the main calendar
        [HttpGet]
        public async Task<ActionResult> Calendar()
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (await Tools.isUserLoggedInAndAPIActive(HttpContext))
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

        //GET Method to show the about us page
        [HttpGet]
        public ActionResult About()
        {
            return View();
        }
        
        //General Error Page to display the error message.
        public ActionResult Error(string error)
        {
            if(error != null && error.Contains("401 (Unauthorized)"))
            {
                error = "The page you are trying to visit does not exist. Please choose from the options below.";
            }
            ViewData["error"] = error;
            return View("~/Views/Shared/Error.cshtml");
        }

        //Language Method to set the language culture of the project
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
            //Redirect to the same page
            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }

    }
}
