using System;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using MobileL2P.Helpers;

namespace MobileL2P.Controllers
{
    //Base Controller that handles adding a culture cookie (EN/DE localization)
    public class BaseController : Controller
    {

        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            string cultureName = null;

            // Attempt to read the culture cookie from Request
            HttpCookie cultureCookie = Request.Cookies["_culture"];
            if (cultureCookie != null)
                cultureName = cultureCookie.Value;
            else
                cultureName = Request.UserLanguages != null && Request.UserLanguages.Length > 0 ? Request.UserLanguages[0] : null; // obtain it from HTTP header AcceptLanguages

            // Validate culture name
            cultureName = CultureHelper.GetImplementedCulture(cultureName); // This is safe


            // Modify current thread's cultures            
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            if(cultureCookie != null)
                HttpContext.Session.Add("Language", cultureCookie.Value.Replace("EN-US","EN"));

            return base.BeginExecuteCore(callback, state);
        }
        

    }
}