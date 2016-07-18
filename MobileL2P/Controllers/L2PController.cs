using System;
using L2PAPIClient.DataModel;
using MobileL2P.Services;
using System.Threading.Tasks;
using MobileL2P.Models;
using static MobileL2P.Services.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web;

namespace MobileL2P.Controllers
{
    public class L2PController : Controller
    {
        // Get Method to add a new Hyperlink in a course
        // GET: /L2P/AddHyperlink
        [HttpGet]
        public async Task<ActionResult> AddHyperlink(string cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;
                        return View("~/Views/L2P/AddEditHyperlink.cshtml");
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to add a hyperlink";
                        return RedirectToAction("Error", "Home", new { @error = errorMessage });
                    }
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


        // Post Method to add a new Hyperlink in a course
        // POST: /L2P/AddHyperlink?
        [HttpPost]
        public async Task<ActionResult> AddHyperlink(HyperLinkViewModel model, string cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;

                        if (!ModelState.IsValid) // Check if the model was filled correctly (Always add)
                        {
                            return View("~/Views/L2P/AddEditHyperlink.cshtml", model);
                        }
                        if (model.URL != null) // Custom Validation / Validate URL
                        {
                            if (model.URL.ToLower().StartsWith("www."))
                                model.URL = "http://" + model.URL;
                            if (!Tools.checkURLValidity(model.URL))
                            {
                                ModelState.AddModelError(string.Empty, "The provided URL is not valid.");
                                return View("~/Views/L2P/AddEditHyperlink.cshtml", model);
                            }
                        }

                        L2PAddHyperlinkRequest newHyperlink = new L2PAddHyperlinkRequest();
                        newHyperlink.url = model.URL;
                        newHyperlink.description = model.Title;
                        newHyperlink.notes = model.Notes;

                        await L2PAPIClient.api.Calls.L2PAddHyperlink(cId, newHyperlink);

                        return RedirectToAction(nameof(MyCoursesController.Hyperlinks), "MyCourses", new { cId = cId, @msg = "Hyperlink was successfully added!" });
                    }
                    else
                    {

                        string errorMessage = "You do not have the sufficient rights to add a hyperlink";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // Post Method to add a new folder in the intended view (Learning Material,...)
        // POST: /L2P/AddFolder
        [HttpPost]
        public async Task<ActionResult> AddFolder(string cId, int ModelNb, string curDir, FolderViewModel model)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    string currentFolder = null;
                    if (curDir != null && curDir.Contains("/"))
                        currentFolder = curDir.Substring(curDir.LastIndexOf('/') + 1);
                    else if (curDir != null && curDir.Length > 0)
                        currentFolder = curDir;

                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && ((userRole.role.Contains("manager") || userRole.role.Contains("tutors")) || ModelNb == 2))
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;

                        if (!ModelState.IsValid) // Check if the model was filled correctly (Always add)
                        {
                            return RedirectToAction(nameof(MyCoursesController.LearningMaterials), "MyCourses", new { cId = cId, ExtdDir = currentFolder, @msg = "Invalid Folder Name!" });
                        }

                        await L2PAPIClient.api.Calls.L2PCreateFolder(cId, ModelNb, model.Name, curDir);

                        //moduleNb (0 = Learning Material, 1 = Media Library, 2 = SharedDocuments
                        ModuleNumber module = (ModuleNumber)Enum.ToObject(typeof(ModuleNumber), ModelNb);
                        string actionName = nameof(MyCoursesController.SharedDocuments);

                        if (module == ModuleNumber.LearningMaterials)
                            actionName = nameof(MyCoursesController.LearningMaterials);
                        else if (module == ModuleNumber.MediaLibrary)
                            actionName = nameof(MyCoursesController.MediaLibrary);

                        return RedirectToAction(actionName, "MyCourses", new { cId = cId, ExtdDir = currentFolder, @msg = "Folder was successfully added!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to add a folder";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // Post Method to add a new file in the intended view (Learning Material,...)
        // POST: /L2P/AddFile
        [HttpPost]
        public async Task<ActionResult> AddFile(HttpPostedFileBase file, string cId, int ModelNb, string curDir)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    string currentFolder = null;
                    if (curDir != null && curDir.Contains("/"))
                        currentFolder = curDir.Substring(curDir.LastIndexOf('/') + 1);
                    else if (curDir != null && curDir.Length > 0)
                        currentFolder = curDir;

                    //moduleNb (0 = Learning Material, 1 = Media Library, 2 = SharedDocuments
                    ModuleNumber module = (ModuleNumber)Enum.ToObject(typeof(ModuleNumber), ModelNb);
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && ((userRole.role.Contains("manager") || userRole.role.Contains("tutors")) || ModelNb == 2))
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;

                        if (file == null) // Check if the model was filled correctly (Always add)
                        {
                            if (module == ModuleNumber.LearningMaterials)
                                return RedirectToAction(nameof(MyCoursesController.LearningMaterials), "MyCourses", new { cId = cId, ExtdDir = currentFolder, @msg = "No file was selected!" });
                            else if (module == ModuleNumber.MediaLibrary)
                                return RedirectToAction(nameof(MyCoursesController.MediaLibrary), "MyCourses", new { cId = cId, ExtdDir = currentFolder, @msg = "No file was selected!" });
                            else if (module == ModuleNumber.SharedDocuments)
                                return RedirectToAction(nameof(MyCoursesController.SharedDocuments), "MyCourses", new { cId = cId, ExtdDir = currentFolder, @msg = "No file was selected!" });
                        }

                        L2PUploadRequest data = new L2PUploadRequest();
                        data.fileName = file.FileName;

                        using (System.IO.Stream stream = file.InputStream)
                        {
                            byte[] buffer = new byte[stream.Length];
                            await stream.ReadAsync(buffer, 0, (int)stream.Length);
                            data.stream = Convert.ToBase64String(buffer);
                        }

                        if (module == ModuleNumber.LearningMaterials)
                        {
                            await L2PAPIClient.api.Calls.L2PuploadInLearningMaterials(cId, curDir, data);
                            return RedirectToAction(nameof(MyCoursesController.LearningMaterials), "MyCourses", new { cId = cId, ExtdDir = currentFolder, @msg = "File was successfully added!" });
                        }
                        else if (module == ModuleNumber.MediaLibrary)
                        {
                            await L2PAPIClient.api.Calls.L2PuploadInMediaLibrary(cId, curDir, data);
                            return RedirectToAction(nameof(MyCoursesController.MediaLibrary), "MyCourses", new { cId = cId, ExtdDir = currentFolder, @msg = "File was successfully added!" });
                        }
                        else if (module == ModuleNumber.SharedDocuments)
                        {
                            await L2PAPIClient.api.Calls.L2PuploadInSharedDocuments(cId, curDir, data);
                            return RedirectToAction(nameof(MyCoursesController.SharedDocuments), "MyCourses", new { cId = cId, ExtdDir = currentFolder, @msg = "File was successfully added!" });
                        }
                        return RedirectToAction(nameof(MyCoursesController.WhatsNew), "MyCourses", new { cId = cId });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to add a folder";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // View Hyperlink with Privilege Validation

        // GET: /L2P/ShowHyperlink?
        [HttpGet]

        public async Task<ActionResult> ShowHyperlink(string cId, int hId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);

                    L2PHyperlinkList hlList = await L2PAPIClient.api.Calls.L2PviewHyperlink(cId, hId);

                    if (hlList != null)
                    {
                        HyperLinkViewModel model = new HyperLinkViewModel();
                        foreach (L2PHyperlinkElement hyperlink in hlList.dataSet)
                        {
                            model.URL = hyperlink.url;
                            model.Title = hyperlink.description;
                            model.Notes = hyperlink.notes;
                            model.itemId = hyperlink.itemId;
                        }
                        ViewData["HyperlinkModel"] = model;
                        return View();
                    }
                    else
                    {
                        string errorMessage = "The Hyperlink you are trying to view does not exist.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // Get Method to Edit a Hyperlink in a course
        // GET: /L2P/EditHyperlink
        [HttpGet]
        public async Task<ActionResult> EditHyperlink(string cId, int hId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        L2PHyperlinkList hlList = await L2PAPIClient.api.Calls.L2PviewHyperlink(cId, hId);
                        if (hlList != null)
                        {
                            HtmlConverter con = new HtmlConverter();
                            HyperLinkViewModel model = new HyperLinkViewModel();
                            foreach (L2PHyperlinkElement hyperlink in hlList.dataSet)
                            {
                                model.URL = hyperlink.url;
                                model.Title = hyperlink.description;
                                model.Notes = con.ConvertHtml(hyperlink.notes);
                                model.itemId = hyperlink.itemId;
                            }
                            ViewData["EditMode"] = true;
                            ViewData["ChosenCourse"] = course;
                            ViewData["userRole"] = userRole;
                            return View("~/Views/L2P/AddEditHyperlink.cshtml", model);
                        }
                        else
                        {
                            string errorMessage = "The Hyperlink you are trying to view does not exist.";
                            return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                        }
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to edit this hyperlink";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // POST Method to Edit a Hyperlink in a course
        // POST: /L2P/EditHyperlink
        [HttpPost]
        public async Task<ActionResult> EditHyperlink(HyperLinkViewModel model, string cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;

                        if (!ModelState.IsValid) // Check if the model was filled correctly (Always add)
                        {
                            return View("~/Views/L2P/AddEditHyperlink.cshtml", model);
                        }
                        if (model.URL != null) // Custom Validation / Validate URL
                        {
                            if (model.URL.ToLower().StartsWith("www."))
                                model.URL = "http://" + model.URL;
                            if (!Tools.checkURLValidity(model.URL))
                            {
                                ModelState.AddModelError(string.Empty, "The provided URL is not valid.");
                                View("~/Views/L2P/AddEditHyperlink.cshtml", model);
                            }
                        }

                        L2PAddHyperlinkRequest editHyperlink = new L2PAddHyperlinkRequest();
                        editHyperlink.url = model.URL;
                        editHyperlink.description = model.Title;
                        editHyperlink.notes = model.Notes;

                        await L2PAPIClient.api.Calls.L2PupdateHyperlink(cId, model.itemId, editHyperlink);

                        return RedirectToAction(nameof(MyCoursesController.Hyperlinks), "MyCourses", new { cId = cId, @msg = "Hyperlink was successfully edited!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to edit a hyperlink";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // Get Method to add a new Hyperlink in a course
        // GET: /L2P/DeleteHyperlinks
        [HttpGet]
        public async Task<ActionResult> DeleteHyperlinks(string cId, string hIds)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext))
                {
                    if (String.IsNullOrEmpty(cId))
                    {
                        string errorMessage = "You were redirected to this page with missing parameters.<br/> Please go back to the home page and try again.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
                    else if (String.IsNullOrEmpty(hIds))
                    {
                        return RedirectToAction(nameof(MyCoursesController.Hyperlinks), "MyCourses", new { @cId = cId });
                    }

                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        hIds = hIds.TrimEnd('-');
                        string[] hyperlinkIds = hIds.Split('-');
                        foreach (string hId in hyperlinkIds)
                        {
                            int id = -1;
                            int.TryParse(hId, out id);
                            await L2PAPIClient.api.Calls.L2PDeleteHyperlink(cId, id);
                        }
                        return RedirectToAction(nameof(MyCoursesController.Hyperlinks), "MyCourses", new { @cId = cId, @msg = "Hyperlinks(s) successfully deleted!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to delete hyperlinks";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // Get Method to delete learning materials in a course
        // GET: /L2P/DeleteLearningMaterials
        [HttpGet]
        public async Task<ActionResult> DeleteLearningMaterials(string cId, string LMIds, string curDir)
        {
            try
            {
                string currentFolder = null;
                if (curDir != null && curDir.Contains("/"))
                    currentFolder = curDir.Substring(curDir.LastIndexOf('/') + 1);
                else if (curDir != null && curDir.Length > 0)
                    currentFolder = curDir;

                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext))
                {
                    if (String.IsNullOrEmpty(cId))
                    {
                        string errorMessage = "You were redirected to this page with missing parameters.<br/> Please go back to the home page and try again.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
                    else if (String.IsNullOrEmpty(LMIds))
                    {
                        return RedirectToAction(nameof(MyCoursesController.LearningMaterials), "MyCourses", new { @cId = cId });
                    }

                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        LMIds = LMIds.TrimEnd('-');
                        string[] lrnMaterialIds = LMIds.Split('-');
                        foreach (string hId in lrnMaterialIds)
                        {
                            int id = -1;
                            int.TryParse(hId, out id);
                            await L2PAPIClient.api.Calls.L2PDeleteLearningMaterial(cId, id);
                        }
                        return RedirectToAction(nameof(MyCoursesController.LearningMaterials), "MyCourses", new { @cId = cId, @ExtdDir = currentFolder, @msg = "Material(s)/Folder(s) successfully deleted!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to delete hyperlinks";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // Get Method to delete MediaLibrary in a course
        // GET: /L2P/DeleteMediaLibrary
        [HttpGet]
        public async Task<ActionResult> DeleteMediaLibrary(string cId, string MLIds, string curDir)
        {
            try
            {
                string currentFolder = null;
                if (curDir != null && curDir.Contains("/"))
                    currentFolder = curDir.Substring(curDir.LastIndexOf('/') + 1);
                else if (curDir != null && curDir.Length > 0)
                    currentFolder = curDir;

                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext))
                {
                    if (String.IsNullOrEmpty(cId))
                    {
                        string errorMessage = "You were redirected to this page with missing parameters.<br/> Please go back to the home page and try again.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
                    else if (String.IsNullOrEmpty(MLIds))
                    {
                        return RedirectToAction(nameof(MyCoursesController.MediaLibrary), "MyCourses", new { @cId = cId });
                    }

                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        MLIds = MLIds.TrimEnd('-');
                        string[] mediaIds = MLIds.Split('-');
                        foreach (string mId in mediaIds)
                        {
                            int id = -1;
                            int.TryParse(mId, out id);
                            await L2PAPIClient.api.Calls.L2PDeleteMediaLibrary(cId, id);
                        }
                        return RedirectToAction(nameof(MyCoursesController.MediaLibrary), "MyCourses", new { @cId = cId, @ExtdDir = currentFolder, @msg = "Media(s)/Folder(s) successfully deleted!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to delete media in media library.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // Get Method to delete Shared Documents in a course
        // GET: /L2P/DeleteSharedDocuments
        [HttpGet]
        public async Task<ActionResult> DeleteSharedDocuments(string cId, string SDIds, string curDir)
        {
            try
            {
                string currentFolder = null;
                if (curDir != null && curDir.Contains("/"))
                    currentFolder = curDir.Substring(curDir.LastIndexOf('/') + 1);
                else if (curDir != null && curDir.Length > 0)
                    currentFolder = curDir;

                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext))
                {
                    if (String.IsNullOrEmpty(cId))
                    {
                        string errorMessage = "You were redirected to this page with missing parameters.<br/> Please go back to the home page and try again.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
                    else if (String.IsNullOrEmpty(SDIds))
                    {
                        return RedirectToAction(nameof(MyCoursesController.MediaLibrary), "MyCourses", new { @cId = cId });
                    }

                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors") || userRole.role.Contains("students") || userRole.role.Contains("extra")))
                    {
                        SDIds = SDIds.TrimEnd('-');
                        string[] mediaIds = SDIds.Split('-');
                        foreach (string mId in mediaIds)
                        {
                            int id = -1;
                            int.TryParse(mId, out id);
                            await L2PAPIClient.api.Calls.L2PDeleteSharedDocument(cId, id);
                        }
                        return RedirectToAction(nameof(MyCoursesController.SharedDocuments), "MyCourses", new { @cId = cId, @ExtdDir = currentFolder, @msg = "Shared Document(s)/Folder(s) successfully deleted!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to delete shared documents";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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
        // Get Method to add a new Announcement in a course
        // GET: /L2P/AddAnnouncement
        [HttpGet]
        public async Task<ActionResult> AddAnnouncement(string cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;
                        return View("~/Views/L2P/AddEditAnnouncement.cshtml");
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to add an announcement";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // Post Method to add a new Announcement in a course
        // POST: /L2P/AddAnnouncement?
        [HttpPost]
        public async Task<ActionResult> AddAnnouncement(AnnouncementViewModel model, string cId, HttpPostedFileBase file)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;

                        if (!ModelState.IsValid) // Check if the model was filled correctly (Always add)
                        {
                            return View("~/Views/L2P/AddEditAnnouncement.cshtml", model);
                        }

                        L2PAddAnnouncementRequest newAnnouncement = new L2PAddAnnouncementRequest();
                        newAnnouncement.title = model.title;
                        newAnnouncement.body = model.body;


                        L2PAddUpdateResponse response = await L2PAPIClient.api.Calls.L2PAddAnnouncement(cId, newAnnouncement);

                        if (file != null)
                        {

                            L2PUploadRequest data = new L2PUploadRequest();
                            data.fileName = file.FileName;

                            using (System.IO.Stream stream = file.InputStream)
                            {
                                byte[] buffer = new byte[stream.Length];
                                await stream.ReadAsync(buffer, 0, (int)stream.Length);
                                data.stream = Convert.ToBase64String(buffer);
                            }

                            L2PAnnouncementList elem = await L2PAPIClient.api.Calls.L2PviewAnnouncement(cId, response.itemId);
                            if (elem.dataSet != null && elem.dataSet.Any())
                            {
                                await L2PAPIClient.api.Calls.L2PuploadInAnnouncements(cId, elem.dataSet.First().attachmentDirectory, data);

                            }
                        }
                        return RedirectToAction(nameof(MyCoursesController.Announcement), "MyCourses", new { cId = cId, @msg = "Announcement was successfully added!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to add an announcement";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // View Announcement with Privilege Validation
        // GET: /L2P/ShowAnnouncement?
        [HttpGet]
        public async Task<ActionResult> ShowAnnouncement(string cId, int aId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);

                    L2PAnnouncementList aList = await L2PAPIClient.api.Calls.L2PviewAnnouncement(cId, aId);
                    if (aList != null)
                    {
                        AnnouncementViewModel model = new AnnouncementViewModel();
                        foreach (L2PAnnouncementElement announcement in aList.dataSet)
                        {
                            model.title = announcement.title;
                            model.body = announcement.body;
                            model.itemId = announcement.itemId;
                            model.folderName = announcement.attachmentDirectory;
                            ViewData["attachments"] = announcement.attachments;
                        }
                        ViewData["AnnouncementModel"] = model;

                        return View();
                    }
                    else
                    {
                        string errorMessage = "The Announcement you are trying to view does not exist.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // Get Method to Edit a Announcement in a course
        // GET: /L2P/EditAnnouncement
        [HttpGet]
        public async Task<ActionResult> EditAnnouncement(string cId, int aId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        L2PAnnouncementList aList = await L2PAPIClient.api.Calls.L2PviewAnnouncement(cId, aId);
                        if (aList != null)
                        {

                            HtmlConverter con = new HtmlConverter();
                            AnnouncementViewModel model = new AnnouncementViewModel();
                            foreach (L2PAnnouncementElement announcement in aList.dataSet)
                            {
                                model.title = announcement.title;
                                model.body = con.ConvertHtml(announcement.body);
                                model.itemId = announcement.itemId;
                            }
                            ViewData["EditMode"] = true;
                            ViewData["ChosenCourse"] = course;
                            ViewData["userRole"] = userRole;
                            return View("~/Views/L2P/AddEditAnnouncement.cshtml", model);
                        }
                        else
                        {
                            string errorMessage = "The Announcement you are trying to view does not exist.";
                            return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                        }
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to edit this announcement";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // POST Method to Edit a Hyperlink in a course
        // POST: /L2P/EditAnnouncement
        [HttpPost]
        public async Task<ActionResult> EditAnnouncement(AnnouncementViewModel model, string cId, HttpPostedFileBase file)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;

                        if (!ModelState.IsValid) // Check if the model was filled correctly (Always add)
                        {
                            return View("~/Views/L2P/AddEditAnnouncement.cshtml", model);
                        }

                        L2PAddAnnouncementRequest editAnnouncement = new L2PAddAnnouncementRequest();
                        editAnnouncement.title = model.title;
                        editAnnouncement.body = model.body;


                        await L2PAPIClient.api.Calls.L2PupdateAnnouncement(cId, model.itemId, editAnnouncement);
                        if (file != null)
                        {

                            L2PUploadRequest data = new L2PUploadRequest();
                            data.fileName = file.FileName;
                            using (System.IO.Stream stream = file.InputStream)
                            {
                                byte[] buffer = new byte[stream.Length];
                                await stream.ReadAsync(buffer, 0, (int)stream.Length);
                                data.stream = Convert.ToBase64String(buffer);
                            }

                            String currDir = "/" + course.semester + "/" + cId + "/" + "Lists/AnnouncementDocuments/A" + model.itemId;
                            await L2PAPIClient.api.Calls.L2PuploadInAnnouncements(cId, currDir, data);

                        }

                        return RedirectToAction(nameof(MyCoursesController.Announcement), "MyCourses", new { cId = cId, @msg = "Announcement was successfully edited!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to edit a announcement";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // Get Method to add a new Announcement in a course
        // GET: /L2P/DeleteAnnouncements
        [HttpGet]
        public async Task<ActionResult> DeleteAnnouncements(string cId, string aIds)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext))
                {
                    if (String.IsNullOrEmpty(cId))
                    {
                        string errorMessage = "You were redirected to this page with missing parameters.<br/> Please go back to the home page and try again.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
                    else if (String.IsNullOrEmpty(aIds))
                    {
                        return RedirectToAction(nameof(MyCoursesController.Announcement), "MyCourses", new { @cId = cId });
                    }

                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        aIds = aIds.TrimEnd('-');
                        string[] announcementIds = aIds.Split('-');
                        foreach (string hId in announcementIds)
                        {
                            int id = -1;
                            int.TryParse(hId, out id);
                            await L2PAPIClient.api.Calls.L2PDeleteAnnouncement(cId, id);
                        }
                        return RedirectToAction(nameof(MyCoursesController.Announcement), "MyCourses", new { @cId = cId, @msg = "Announcement(s) successfully deleted!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to delete announcements";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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
        // Get Method to add a new Email in a course
        // GET: /L2P/AddEmail
        [HttpGet]
        public async Task<ActionResult> AddEmail(string cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    List<string> allPossibleRecipients = new List<string>();

                    L2PAvailableGroups availableGroupList = await L2PAPIClient.api.Calls.L2PviewAvailableGroupsInGroupWorkspace(cId);

                    foreach (var el in availableGroupList.dataSet)
                    {
                        allPossibleRecipients.Add(el.systemGeneratedAlias);
                    }
                    ViewData["allPossibleRecipients"] = allPossibleRecipients;

                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;
                        return View("~/Views/L2P/AddEmail.cshtml");
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to add an email";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // Post Method to add a new Email in a course
        // POST: /L2P/AddEmail?
        [HttpPost]
        public async Task<ActionResult> AddEmail(EmailViewModel model, string cId, HttpPostedFileBase file)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);


                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;

                        if (!ModelState.IsValid) // Check if the model was filled correctly (Always add)
                        {
                            return View("~/Views/L2P/AddEmail.cshtml", model);
                        }

                        L2PAddEmailRequest newEmail = new L2PAddEmailRequest();
                        newEmail.body = model.body;
                        newEmail.replyTo = "true";
                        newEmail.attachmentsToUpload = new List<L2PUploadRequest>();
                        if (model.cc != null)

                            newEmail.cc = model.cc.Replace(",", ";") + ";";

                        var recipients = Request.Form.Get("recipients");
                        if (recipients != null)
                        {
                            newEmail.recipients = recipients.Replace(",",";") + ";";
                        }
                        newEmail.subject = model.subject;

                        if (file != null)
                        {
                            L2PUploadRequest data = new L2PUploadRequest();
                            data.fileName = file.FileName;
                            using (System.IO.Stream stream = file.InputStream)
                            {
                                byte[] buffer = new byte[stream.Length];
                                await stream.ReadAsync(buffer, 0, (int)stream.Length);
                                data.stream = Convert.ToBase64String(buffer);
                            }

                            List<L2PUploadRequest> listOfUploads = new List<L2PUploadRequest>();
                            listOfUploads.Add(data);
                            newEmail.attachmentsToUpload = listOfUploads;

                        }

                        L2PAddUpdateResponse response = await L2PAPIClient.api.Calls.L2PAddEmail(cId, newEmail);


                        return RedirectToAction(nameof(MyCoursesController.Email), "MyCourses", new { cId = cId, @msg = "Email was successfully added!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to add an Email";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // View Email with Privilege Validation
        // GET: /L2P/ShowEmail?
        [HttpGet]
        public async Task<ActionResult> ShowEmail(string cId, int eId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);

                    L2PEmailList eList = await L2PAPIClient.api.Calls.L2PviewEmail(cId, eId);
                    if (eList != null)
                    {
                        EmailViewModel model = new EmailViewModel();
                        foreach (L2PEmailElement email in eList.dataSet)
                        {
                            model.recipients = email.recipients;
                            model.body = email.body;
                            model.itemId = email.itemId;
                            model.cc = email.cc;
                            model.sender = email.from;
                            model.subject = email.subject;
                            ViewData["attachments"] = email.attachments;
                        }
                        ViewData["EmailModel"] = model;

                        return View();
                    }
                    else
                    {
                        string errorMessage = "The Email you are trying to view does not exist.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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



        // Get Method to delete Emails in a course
        // GET: /L2P/DeleteEmails
        [HttpGet]
        public async Task<ActionResult> DeleteEmails(string cId, string eIds)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext))
                {
                    if (String.IsNullOrEmpty(cId))
                    {
                        string errorMessage = "You were redirected to this page with missing parameters.<br/> Please go back to the home page and try again.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
                    else if (String.IsNullOrEmpty(eIds))
                    {
                        return RedirectToAction(nameof(MyCoursesController.Email), "MyCourses", new { @cId = cId });
                    }

                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        eIds = eIds.TrimEnd('-');
                        string[] emailIds = eIds.Split('-');
                        foreach (string hId in emailIds)
                        {
                            int id = -1;
                            int.TryParse(hId, out id);
                            await L2PAPIClient.api.Calls.L2PDeleteEmail(cId, id);
                        }
                        return RedirectToAction(nameof(MyCoursesController.Email), "MyCourses", new { @cId = cId, @msg = "Email(s) successfully deleted!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to delete emails";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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



        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> AddSolution(IFormFile file, SolutionViewModel model, String cId, string aId)
        //{
        //    try
        //    {
        //        // This method must be used before every L2P API call
        //        Tools.getAndSetUserToken(Request.Cookies, HttpContext);
        //        if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
        //        {
        //            L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
        //            L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
        //            if (userRole != null && (userRole.role.Contains("student")))
        //            {
        //                ViewData["ChosenCourse"] = course;
        //                ViewData["userRole"] = userRole;

        //                if (!ModelState.IsValid) // Check if the model was filled correctly (Always add)
        //                {
        //                    return View(model);
        //                }


        //                if (model != null)
        //                {


        //                    L2PAssignmentSolution sol = new L2PAssignmentSolution();
        //                    L2PUploadRequest data = new L2PUploadRequest();
        //                    data.fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition)
        //                        .FileName
        //                        .Trim('"');// FileName returns "fileName.ext"(with double quotes) in beta 3

        //                    using (System.IO.Stream stream = file.OpenReadStream())
        //                    {
        //                        byte[] buffer = new byte[stream.Length];
        //                        await stream.ReadAsync(buffer, 0, (int)stream.Length);
        //                        data.stream = Convert.ToBase64String(buffer);
        //                    }
        //                    String sourceDirectory = "/" + course.semester + "/" + course.uniqueid + "/assessment/Lists/LA_SolutionDocuments/A" + aId + "/S" + model.solName;


        //                    await L2PAPIClient.api.Calls.L2PuploadInAssignments(cId, sourceDirectory, data);


        //                    sol.solutionDirectory = "/" + course.semester + "/" + course.uniqueid + "/assessment/Lists/LA_SolutionDocuments/A" + model.assignID + "/S" + model.solName;
        //                    sol.itemId = model.assignID;

        //                    L2PAvailableGroups group = await L2PAPIClient.api.Calls.L2PviewAvailableGroupsInGroupWorkspace(cId);
        //                    L2PgwsElement GE = new L2PgwsElement();
        //                    //if(group !=null)

        //                    string groupAlias = "";
        //                    foreach (L2PgwsElement g in group.dataSet)
        //                    {
        //                        groupAlias = g.systemGeneratedAlias;
        //                    }

        //                    await L2PAPIClient.api.Calls.L2PprovideAssignmentSolution(cId, model.assignID, groupAlias, sol);

        //                }

        //                //await L2PAPIClient.api.Calls.L2PprovideAssignmentSolution()

        //                return RedirectToAction(nameof(L2PController.AddSolution), "L2P", new { model = model, cId = cId });
        //            }
        //            else
        //            {
        //                String errorMessage = "You do not have the sufficient rights to add a solution";
        //                return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
        //            }
        //        }
        //        else
        //        {
        //            return RedirectToAction(nameof(AccountController.Login), "Account");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = ex.Message });
        //    }
        //}


        //[HttpGet]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> AddSolution(String cId, int sid)
        //{
        //    try
        //    {
        //        // This method must be used before every L2P API call
        //        Tools.getAndSetUserToken(Request.Cookies, HttpContext);
        //        if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
        //        {
        //            L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
        //            L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
        //            if (userRole != null && (userRole.role.Contains("student")))
        //            {
        //                ViewData["ChosenCourse"] = course;
        //                ViewData["userRole"] = userRole;

        //                SolutionViewModel model = new SolutionViewModel();
        //               L2PAssignmentList L = await L2PAPIClient.api.Calls.L2PviewAssignment(cId, sid);
        //                //await L2PAPIClient.api.Calls.L2PprovideAssignmentSolution()
        //                foreach(L2PAssignmentElement a in L.dataSet)
        //                {
        //                    L2PAssignmentSolution asS = a.solution;
        //                    model.StudentComment = asS.studentComment;
        //                    model.assignID= asS.itemId;
        //                    model.Status = asS.Status;
        //                    model.assignmentName = a.title;



        //                }

        //                ViewData["ChosenCourse"] = course;
        //                ViewData["userRole"] = userRole;
        //                return View("~/Views/L2P/AddSolution.cshtml", model);
        //            }
        //            else
        //            {
        //                String errorMessage = "You do not have the sufficient rights to add a solution";
        //                return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
        //            }
        //        }
        //        else
        //        {
        //            return RedirectToAction(nameof(AccountController.Login), "Account");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = ex.Message });
        //    }
        //}



        // View Literature with Privileged Validation
        // GET: /L2P/ViewLiterature?
        [HttpGet]
        public async Task<ActionResult> ViewLiterature(string cId, int lId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);

                    L2PLiteratureSetDataType LList = await L2PAPIClient.api.Calls.L2PviewLiteratureAsync(cId, lId);
                    if (LList != null)
                    {
                        LiteratureViewModel model = new LiteratureViewModel();
                        L2PLiteratureElementDataType literature = LList.dataSet.First();
                        model.title = literature.title;
                        model.authors = literature.authors;
                        model.year = literature.year;
                        model.url = literature.url;
                        model.publisher = literature.publisher;
                        model.relevance = literature.relevance;
                        model.address = literature.address;
                        model.booktitle = literature.booktitle;
                        model.comments = literature.comments;
                        model.doi = literature.doi;
                        model.edition = literature.edition;
                        model.fromPage = literature.fromPage;
                        model.isxn = literature.isxn;
                        model.journalName = literature.journalName;
                        model.number = literature.number;
                        model.role = literature.role;
                        model.series = literature.series;
                        model.toPage = literature.toPage;
                        model.type = literature.type;
                        model.volume = literature.volume;
                        model.urlComment = literature.urlComment;
                        model.itemId = literature.itemID;

                        ViewData["LiteratureModel"] = model;
                        return View();
                    }
                    else
                    {
                        string errorMessage = "The Literature you are trying to view does not exist.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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


        // Get Method to delete a new Literature in a course
        // GET: /L2P/DeleteLiterature
        [HttpGet]
        public async Task<ActionResult> DeleteLiterature(string cId, string lIds)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext))
                {
                    if (String.IsNullOrEmpty(cId))
                    {
                        string errorMessage = "You were redirected to this page with missing parameters.<br/> Please go back to the home page and try again.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
                    else if (String.IsNullOrEmpty(lIds))
                    {
                        return RedirectToAction(nameof(MyCoursesController.Literature), "MyCourses", new { @cId = cId });
                    }

                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        lIds = lIds.TrimEnd('-');
                        string[] LiteratureIds = lIds.Split('-');
                        foreach (string lId in LiteratureIds)
                        {
                            int id = -1;
                            int.TryParse(lId, out id);
                            await L2PAPIClient.api.Calls.L2PDeleteLiterature(cId, id);
                        }
                        return RedirectToAction(nameof(MyCoursesController.Literature), "MyCourses", new { @cId = cId, @msg = "Literature(s) successfully deleted!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to delete Literature(s)";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // Get Method to Edit a Hyperlink in a course
        // GET: /L2P/EditHyperlink
        [HttpGet]
        public async Task<ActionResult> EditLiterature(string cId, int lId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        L2PLiteratureSetDataType lList = await L2PAPIClient.api.Calls.L2PviewLiteratureAsync(cId, lId);
                        if (lList != null)
                        {

                            LiteratureViewModel model = new LiteratureViewModel();
                            L2PLiteratureElementDataType literature = lList.dataSet.First();
                            model.title = literature.title;
                            model.authors = literature.authors;
                            model.year = literature.year;
                            model.url = literature.url;
                            model.publisher = literature.publisher;
                            model.relevance = literature.relevance;
                            model.address = literature.address;
                            model.booktitle = literature.booktitle;
                            model.comments = literature.comments;
                            model.doi = literature.doi;
                            model.edition = literature.edition;
                            model.fromPage = literature.fromPage;
                            model.isxn = literature.isxn;
                            model.journalName = literature.journalName;
                            model.number = literature.number;
                            model.role = literature.role;
                            model.series = literature.series;
                            model.toPage = literature.toPage;
                            model.type = literature.type;
                            model.volume = literature.volume;
                            model.urlComment = literature.urlComment;
                            model.itemId = literature.itemID;

                            ViewData["EditMode"] = true;
                            ViewData["ChosenCourse"] = course;
                            ViewData["userRole"] = userRole;
                            return View("~/Views/L2P/AddEditLiterature.cshtml", model);
                        }
                        else
                        {
                            string errorMessage = "The Literature you are trying to view does not exist.";
                            return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                        }
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to edit this Literature";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // POST Method to Edit a Hyperlink in a course
        // POST: /L2P/EditHyperlink
        [HttpPost]
        public async Task<ActionResult> EditLiterature(LiteratureViewModel model, string cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;

                        if (!ModelState.IsValid) // Check if the model was filled correctly (Always add)
                        {
                            return View("~/Views/L2P/AddEditLiterature.cshtml", model);
                        }

                        L2PLiteratureAddRequest editLiterature = new L2PLiteratureAddRequest();
                        editLiterature.title = model.title;
                        editLiterature.authors = model.authors;
                        editLiterature.year = model.year;
                        editLiterature.url = model.url;
                        editLiterature.publisher = model.publisher;
                        editLiterature.relevance = model.relevance;
                        editLiterature.address = model.address;
                        editLiterature.booktitle = model.booktitle;
                        editLiterature.comments = model.comments;
                        editLiterature.doi = model.doi;
                        editLiterature.edition = model.edition;
                        editLiterature.fromPage = model.fromPage;
                        editLiterature.isxn = model.isxn;
                        editLiterature.journalName = model.journalName;
                        editLiterature.number = model.number;
                        editLiterature.role = model.role;
                        editLiterature.series = model.series;
                        editLiterature.toPage = model.toPage;
                        editLiterature.type = model.type;
                        editLiterature.volume = model.volume;
                        editLiterature.urlComment = model.urlComment;

                        if (model.url != null)
                        {
                            if (model.url.ToLower().StartsWith("www."))
                                model.url = "http://" + model.url;
                            if (!Tools.checkURLValidity(model.url))
                            {
                                ModelState.AddModelError(string.Empty, "The provided URL is not valid.");
                                View("~/Views/L2P/AddEditLiterature.cshtml", model);
                            }
                        }

                        await L2PAPIClient.api.Calls.L2PupdateLiterature(cId, model.itemId, editLiterature);

                        return RedirectToAction(nameof(MyCoursesController.Literature), "MyCourses", new { cId = cId, @msg = "Literature was successfully edited!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to edit a Literature";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // Get Method to Add a Literature in a course
        // GET: /L2P/AddLiterature
        [HttpGet]
        public async Task<ActionResult> AddLiterature(string cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;
                        return View("~/Views/L2P/AddEditLiterature.cshtml");
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to edit this Literature";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // POST Method to Add Literature in a course
        // POST: /L2P/AddLiterature
        [HttpPost]
        public async Task<ActionResult> AddLiterature(LiteratureViewModel model, string cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;

                        if (!ModelState.IsValid) // Check if the model was filled correctly (Always add)
                        {
                            return View("~/Views/L2P/AddEditLiterature.cshtml", model);
                        }

                        L2PLiteratureAddRequest AddLiterature = new L2PLiteratureAddRequest();

                        AddLiterature.title = model.title;
                        AddLiterature.authors = model.authors;
                        AddLiterature.year = model.year;
                        AddLiterature.url = model.url;
                        AddLiterature.publisher = model.publisher;
                        AddLiterature.relevance = model.relevance;
                        AddLiterature.address = model.address;
                        AddLiterature.booktitle = model.booktitle;
                        AddLiterature.comments = model.comments;
                        AddLiterature.doi = model.doi;
                        AddLiterature.edition = model.edition;
                        AddLiterature.fromPage = model.fromPage;
                        AddLiterature.isxn = model.isxn;
                        AddLiterature.journalName = model.journalName;
                        AddLiterature.number = model.number;
                        AddLiterature.role = model.role;
                        AddLiterature.series = model.series;
                        AddLiterature.toPage = model.toPage;
                        AddLiterature.type = model.type;
                        AddLiterature.volume = model.volume;
                        AddLiterature.urlComment = model.urlComment;

                        if (model.url != null) // Custom Validation / Validate URL
                        {
                            if (model.url.ToLower().StartsWith("www."))
                                model.url = "http://" + model.url;
                            if (!Tools.checkURLValidity(model.url))
                            {
                                ModelState.AddModelError(string.Empty, "The provided URL is not valid.");
                                View("~/Views/L2P/AddEditLiterature.cshtml", model);
                            }
                        }

                        await L2PAPIClient.api.Calls.L2PAddLiterature(cId, AddLiterature);
                        return RedirectToAction(nameof(MyCoursesController.Literature), "MyCourses", new { cId = cId, @msg = "Literature was successfully Added!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to Add a hyperlink";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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


        // Get Method to add a new Hyperlink in a course
        // GET: /L2P/DeleteHyperlinks
        [HttpGet]
        public async Task<ActionResult> DeleteAssignment(string cId, string aId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext))
                {
                    if (String.IsNullOrEmpty(cId))
                    {
                        string errorMessage = "You were redirected to this page with missing parameters.<br/> Please go back to the home page and try again.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
                    else if (String.IsNullOrEmpty(aId))
                    {
                        return RedirectToAction(nameof(MyCoursesController.Literature), "MyCourses", new { @cId = cId });
                    }

                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {

                        await L2PAPIClient.api.Calls.L2PDeleteAssignment(cId, Int32.Parse(aId));
                        return RedirectToAction(nameof(MyCoursesController.Assignments), "MyCourses", new { @cId = cId, @msg = "Assignment successfully deleted!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to delete Assignment";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // Get Method to add a new assignment in a course
        // GET: /L2P/AddAssignment
        [HttpGet]
        public async Task<ActionResult> AddAssignment(string cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;
                        return View("~/Views/L2P/AddEditAssignment.cshtml");
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to add an Assignment";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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


        // Post Method to add a new Hyperlink in a course
        // POST: /L2P/AddHyperlink?
        [HttpPost]
        public async Task<ActionResult> AddAssignment(AssignmentViewModel model, string cId, HttpPostedFileBase file, HttpPostedFileBase fileSo)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;

                        if (!ModelState.IsValid) // Check if the model was filled correctly (Always add)
                        {
                            return View("~/Views/L2P/AddEditAssignment.cshtml", model);
                        }


                        L2PAddAssignmentRequest newAssignment = new L2PAddAssignmentRequest();
                        newAssignment.description = model.Description;

                        string one = model.DueDate;
                        string two = model.DueDatehours;

                        DateTime dt = Convert.ToDateTime(one + " " + two);

                        //DateTime dt1 = DateTime.ParseExact(one + " " + two, "dd/MM/yy h:mm:ss tt", CultureInfo.InvariantCulture);
                        long dtunix = (long)(TimeZoneInfo.ConvertTimeToUtc(dt) - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;


                        newAssignment.dueDate = dtunix;
                        if (model.groupSubmissionAllowed)
                            newAssignment.groupSubmissionAllowed = true;
                        else
                            newAssignment.groupSubmissionAllowed = false;
                        newAssignment.title = model.Title;
                        newAssignment.totalMarks = model.totalPoint;







                        L2PAddUpdateResponse response = await L2PAPIClient.api.Calls.L2PAddAssignment(cId, newAssignment);

                        //if (file != null)
                        //{

                        //    L2PUploadRequest data = new L2PUploadRequest();
                        //    data.fileName = ContentDispositionHeaderValue
                        //        .Parse(file.ContentDisposition)
                        //        .FileName
                        //        .Trim('"');// FileName returns "fileName.ext"(with double quotes) in beta 3

                        //    using (System.IO.Stream stream = file.OpenReadStream())
                        //    {
                        //        byte[] buffer = new byte[stream.Length];
                        //        await stream.ReadAsync(buffer, 0, (int)stream.Length);
                        //        data.stream = Convert.ToBase64String(buffer);
                        //    }

                        //    L2PAssignmentList elem = await L2PAPIClient.api.Calls.L2PviewAssignment(cId, response.itemId);

                        //    if (elem.dataSet != null && elem.dataSet.Any())
                        //    {

                        //        string directory = "/" + course.semester + "/" + cId + "/assessment/Lists/LA_AssignmentDocuments/A" + elem.dataSet.First().itemId + "/";
                        //        await L2PAPIClient.api.Calls.L2PuploadInAssignments(cId, directory, data);

                        //    }
                        //}
                        //if (fileSo != null)
                        //{

                        //    L2PUploadRequest data2 = new L2PUploadRequest();
                        //    data2.fileName = ContentDispositionHeaderValue
                        //        .Parse(fileSo.ContentDisposition)
                        //        .FileName
                        //        .Trim('"');// FileName returns "fileName.ext"(with double quotes) in beta 3

                        //    using (System.IO.Stream stream = fileSo.OpenReadStream())
                        //    {
                        //        byte[] buffer = new byte[stream.Length];
                        //        await stream.ReadAsync(buffer, 0, (int)stream.Length);
                        //        data2.stream = Convert.ToBase64String(buffer);
                        //    }

                        //    L2PAssignmentList elem = await L2PAPIClient.api.Calls.L2PviewAssignment(cId, response.itemId);
                        //    if (elem.dataSet != null && elem.dataSet.Any())
                        //    {
                        //        string directory = "/" + course.semester + "/" + cId + "/assessment/Lists/LA_SolutionDocuments/A" + elem.dataSet.First().itemId + "/" + "S/" ;
                        //        await L2PAPIClient.api.Calls.L2PuploadInAssignments(cId, directory, data2);

                        //    }
                        //}
                        return RedirectToAction(nameof(MyCoursesController.Assignments), "MyCourses", new { cId = cId, @msg = "Assignment was successfully added!" });
                    }

                    else
                    {

                        string errorMessage = "You do not have the sufficient rights to add an Assignment";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // Get Method to add a new assignment in a course
        // GET: /L2P/AddAssignment
        [HttpGet]
        public async Task<ActionResult> EditAssignment(string cId, string aId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;
                        L2PAssignmentList aList = await L2PAPIClient.api.Calls.L2PviewAssignment(cId, Int32.Parse(aId));
                        if (aList != null)
                        {
                            AssignmentViewModel model = new AssignmentViewModel();
                            foreach (L2PAssignmentElement a in aList.dataSet)
                            {

                                model.Description = a.description;

                                model.DueDate = Tools.toDateTimeString(a.dueDate);
                                model.DueDatehours = Tools.toHoursString(a.dueDate);
                                if (a.groupSubmissionAllowed)
                                {
                                    model.groupSubmissionAllowed = true;
                                }
                                else
                                {
                                    model.groupSubmissionAllowed = false;
                                }
                                model.Title = a.title;
                                model.totalPoint = a.totalPoint;


                            }
                            ViewData["AssignmentViewModel"] = model;
                            ViewData["EditMode"] = true;
                            return View("~/Views/L2P/AddEditAssignment.cshtml", model);
                        }
                        else
                        {
                            string errorMessage = "The Assignment you are trying to view does not exist.";
                            return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                        }
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to edit this Assignment";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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


        // Post Method to add a new Hyperlink in a course
        // POST: /L2P/AddHyperlink?
        [HttpPost]
        public async Task<ActionResult> EditAssignment(AssignmentViewModel model, string cId, HttpPostedFileBase file, HttpPostedFileBase fileSo)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;

                        if (!ModelState.IsValid) // Check if the model was filled correctly (Always add)
                        {
                            return View("~/Views/L2P/AddEditAssignment.cshtml", model);
                        }


                        L2PAddAssignmentRequest newAssignment = new L2PAddAssignmentRequest();
                        newAssignment.description = model.Description;

                        string one = model.DueDate;
                        string two = model.DueDatehours;

                        DateTime dt = Convert.ToDateTime(one + " " + two);
                        
                        //DateTime dt1 = DateTime.ParseExact(one + " " + two, "dd/MM/yy h:mm:ss tt", CultureInfo.InvariantCulture);
                        long dtunix = (long)(TimeZoneInfo.ConvertTimeToUtc(dt) - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;


                        newAssignment.dueDate = dtunix;
                        if (model.groupSubmissionAllowed)
                            newAssignment.groupSubmissionAllowed = true;
                        else
                            newAssignment.groupSubmissionAllowed = false;
                        newAssignment.title = model.Title;
                        newAssignment.totalMarks = model.totalPoint;

                        // await L2PAPIClient.api.Calls.L2PupdateAssignment(cId, newAssignment);

                        //if (file != null)
                        //{

                        //    L2PUploadRequest data = new L2PUploadRequest();
                        //    data.fileName = ContentDispositionHeaderValue
                        //        .Parse(file.ContentDisposition)
                        //        .FileName
                        //        .Trim('"');// FileName returns "fileName.ext"(with double quotes) in beta 3

                        //    using (System.IO.Stream stream = file.OpenReadStream())
                        //    {
                        //        byte[] buffer = new byte[stream.Length];
                        //        await stream.ReadAsync(buffer, 0, (int)stream.Length);
                        //        data.stream = Convert.ToBase64String(buffer);
                        //    }

                        //    L2PAssignmentList elem = await L2PAPIClient.api.Calls.L2PviewAssignment(cId, response.itemId);

                        //    if (elem.dataSet != null && elem.dataSet.Any())
                        //    {

                        //        string directory = "/" + course.semester + "/" + cId + "/assessment/Lists/LA_AssignmentDocuments/A" + elem.dataSet.First().itemId + "/";
                        //        await L2PAPIClient.api.Calls.L2PuploadInAssignments(cId, directory, data);

                        //    }
                        //}
                        //if (fileSo != null)
                        //{

                        //    L2PUploadRequest data2 = new L2PUploadRequest();
                        //    data2.fileName = ContentDispositionHeaderValue
                        //        .Parse(fileSo.ContentDisposition)
                        //        .FileName
                        //        .Trim('"');// FileName returns "fileName.ext"(with double quotes) in beta 3

                        //    using (System.IO.Stream stream = fileSo.OpenReadStream())
                        //    {
                        //        byte[] buffer = new byte[stream.Length];
                        //        await stream.ReadAsync(buffer, 0, (int)stream.Length);
                        //        data2.stream = Convert.ToBase64String(buffer);
                        //    }

                        //    L2PAssignmentList elem = await L2PAPIClient.api.Calls.L2PviewAssignment(cId, response.itemId);
                        //    if (elem.dataSet != null && elem.dataSet.Any())
                        //    {
                        //        string directory = "/" + course.semester + "/" + cId + "/assessment/Lists/LA_SolutionDocuments/A" + elem.dataSet.First().itemId + "/" + "S/" ;
                        //        await L2PAPIClient.api.Calls.L2PuploadInAssignments(cId, directory, data2);

                        //    }
                        //}
                        return RedirectToAction(nameof(MyCoursesController.Assignments), "MyCourses", new { cId = cId, @msg = "Sample Solution was successfully added!" });
                    }

                    else
                    {

                        string errorMessage = "You do not have the sufficient rights to add an Assignment";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        [HttpGet] // Get Method to show specific assignments
        public async Task<ActionResult> ShowAssignment(string cId, string aid)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    L2PAssignmentList assnList = await L2PAPIClient.api.Calls.L2PviewAssignment(cId, int.Parse(aid));
                    ViewData["ChosenAssignment"] = assnList;
                    List<L2PAssignmentElement> assignments = new List<L2PAssignmentElement>();
                    if (assnList.dataSet != null)
                    {

                        assignments = assnList.dataSet;

                    }
                    ViewData["ViewAssignment"] = assignments;
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

        // View Discussion with Privilege Validation
        // GET: /L2P/ShowDiscussion?
        [HttpGet]
        public async Task<ActionResult> ShowDiscussion(string cId, string dId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    int discussId = -1;
                    int.TryParse(dId, out discussId);

                    L2PDiscussionItemList dList = await L2PAPIClient.api.Calls.L2PviewAllDiscussionItems(cId);
                    if (dList != null)
                    {

                        var mainDiscussionForum = from elts in dList.dataSet
                                                  where elts.selfId == discussId
                                                  select elts;

                        var discussionReplies = from elts in dList.dataSet
                                                where elts.parentDiscussionId == discussId && elts.selfId != discussId
                                                select elts;

                        if (mainDiscussionForum.Any())
                        {
                            ViewData["mainThread"] = mainDiscussionForum.ToList();
                        }
                        if (discussionReplies.Any())
                        {
                            ViewData["mainThreadReplies"] = discussionReplies.ToList();
                        }

                        return View();
                    }
                    else
                    {
                        string errorMessage = "The discussion thread you are trying to view does not exist.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // Get Method to Edit a Announcement in a course
        // GET: /L2P/EditAnnouncement
        [HttpGet]
        public async Task<ActionResult> EditDiscussion(string cId, int dId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null)
                    {
                        L2PDiscussionItemList dList = await L2PAPIClient.api.Calls.L2PviewDiscussionItem(cId, dId);
                        if (dList != null)
                        {

                            HtmlConverter con = new HtmlConverter();
                            DiscussionViewModel model = new DiscussionViewModel();
                            foreach (L2PDiscussionItemElement discuss in dList.dataSet)
                            {
                                model.title = discuss.subject;
                                model.body = con.ConvertHtml(discuss.body);
                                model.dId = discuss.selfId;
                            }
                            ViewData["EditMode"] = true;
                            ViewData["ChosenCourse"] = course;
                            ViewData["userRole"] = userRole;
                            return View("~/Views/L2P/AddEditDiscussion.cshtml", model);
                        }
                        else
                        {
                            string errorMessage = "The Announcement you are trying to view does not exist.";
                            return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                        }
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to edit this announcement";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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


        // POST: /L2P/PostDiscussion

        [HttpGet]
        public async Task<ActionResult> PostDiscussion(string cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null)
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;
                        return View("~/Views/L2P/AddEditDiscussion.cshtml");
                    }
                    else
                    {
                        string errorMessage = "Unauthorized Action!";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // Post Method to add a new Announcement in a course
        // POST: /L2P/AddAnnouncement?
        [HttpPost]
        public async Task<ActionResult> PostDiscussion(DiscussionViewModel model, string cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null)
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;

                        if (!ModelState.IsValid) // Check if the model was filled correctly (Always add)
                        {
                            return View("~/Views/L2P/AddEditDiscussion.cshtml", model);
                        }

                        L2PAddDiscussionThreadRequest discuss = new L2PAddDiscussionThreadRequest();
                        discuss.subject = model.title;
                        discuss.body = model.body;
                        //get discussion id

                        return RedirectToAction(nameof(MyCoursesController.DiscussionForum), "MyCourses", new { cId = cId, @msg = "Discussion Post was successfully added!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to add an announcement";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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


        // Get Method to Delete a Discussions
        // GET: /L2P/DeleteDiscussions
        [HttpGet]
        public async Task<ActionResult> DeleteDiscussions(string cId, string dIds)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext))
                {
                    if (String.IsNullOrEmpty(cId))
                    {
                        string errorMessage = "You were redirected to this page with missing parameters.<br/> Please go back to the home page and try again.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
                    else if (String.IsNullOrEmpty(dIds))
                    {
                        return RedirectToAction(nameof(MyCoursesController.Announcement), "MyCourses", new { @cId = cId });
                    }

                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null)
                    {
                        dIds = dIds.TrimEnd('-');
                        string[] discussionIds = dIds.Split('-');
                        foreach (string dId in discussionIds)
                        {
                            int id = -1;
                            int.TryParse(dId, out id);
                            await L2PAPIClient.api.Calls.L2PDeleteDiscussionItem(cId, id);
                        }

                    }
                    return RedirectToAction(nameof(MyCoursesController.DiscussionForum), "MyCourses", new { @cId = cId, @msg = "Discussion successfully deleted!" });
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

        [HttpGet]
        public async Task<ActionResult> ReplyDiscussion(string cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null)
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;
                        return View("~/Views/L2P/AddReplyDiscussion.cshtml");
                    }
                    else
                    {
                        string errorMessage = "You are not authorized to add a reply!";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // Post Method to add a new Reply in a Discussion
        // POST: /L2P/ReplyDiscussion?
        [HttpPost]
        public async Task<ActionResult> ReplyDiscussion(DiscussionViewModel model, string cId, int pId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                L2PDiscussionItemList elem;
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null)
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;

                        if (!ModelState.IsValid) // Check if the model was filled correctly (Always add)
                        {
                            return View("~/Views/L2P/AddReplyDiscussion", model);
                        }


                        //to be seen
                        L2PAddDiscussionThreadReplyRequest newReply = new L2PAddDiscussionThreadReplyRequest();
                        newReply.body = model.body;
                        newReply.subject = model.title;
                        L2PAddUpdateResponse response = await L2PAPIClient.api.Calls.L2PAddDiscussionThreadReply(cId, pId, newReply);

                        elem = await L2PAPIClient.api.Calls.L2PviewDiscussionItem(cId, response.itemId);
                        if (elem.dataSet != null && elem.dataSet.Any())
                        {
                            return RedirectToAction(nameof(MyCoursesController.DiscussionForum), "MyCourses", new { cId = cId, @msg = "Reply was successfully added!" });
                        }
                        else
                        {
                            return RedirectToAction(nameof(MyCoursesController.DiscussionForum), "MyCourses", new { cId = cId, @msg = "Unable to post reply!!" });
                        }
                    }
                    //return RedirectToAction(nameof(MyCoursesController.Announcement), "MyCourses", new { cId = cId, @msg = "Reply was successfully added!" });

                    else
                    {
                        string errorMessage = "Unauthorized Reply";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }


                    ViewData["ShowDiscussion"] = elem;
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

    }
}

