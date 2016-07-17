using L2PAPIClient;
using L2PAPIClient.DataModel;
using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Grp.L2PSite.MobileApp.Services;
using ICSharpCode.SharpZipLib.Zip;
using Grp.L2PSite.MobileApp.Models;
using MobileL2P.Controllers;
using System.Web.Mvc;

namespace MobileL2P.Controllers
{
    public class MyCoursesController : BaseController
    {

        [HttpGet] // Get Method to retrieve the course What's New Page
        public async Task<ActionResult> WhatsNew(String cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext))
                {

                    HttpContext.Session.Add("CourseId", cId);
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["CourseWhatsNew"] = await L2PAPIClient.api.Calls.L2PwhatsNewSinceAsync(cId, 180000);
                    ViewData["ExamResults"] = await L2PAPIClient.api.Calls.L2PviewExamResults(cId);

                    L2PAssignmentList assnList = await L2PAPIClient.api.Calls.L2PviewAllAssignments(cId);
                    List<L2PAssignmentElement> assignments = new List<L2PAssignmentElement>();
                    if (assnList.dataSet != null)
                    {
                        assignments = assnList.dataSet;
                        // Sort by publish date desc
                        assignments.Sort((x, y) => y.assignmentPublishDate.CompareTo(x.assignmentPublishDate));
                    }
                    ViewData["Assignments"] = assignments;
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

        [HttpGet] // Get Method for Exam Results Display
        public async Task<ActionResult> ExamResults(string cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext))
                {

                    HttpContext.Session.Add("CourseId", cId);
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["ExamResults"] = await L2PAPIClient.api.Calls.L2PviewExamResults(cId);
                    ViewData["ExamResultStatistics"] = await L2PAPIClient.api.Calls.L2PviewExamResultsStatistics(cId);
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

        [HttpGet] // Get Method to show the subject info of a course.
        public ActionResult ShowSubject(String cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["CourseInfo"] = L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId).Result;
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

        [HttpGet] // Get Method to show the learning material of a course.
        public async Task<ActionResult> LearningMaterials(string cId, string ExtdDir, string msg)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["ChosenCourse"] = course;
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    L2PLearningMaterialList lmList = await L2PAPIClient.api.Calls.L2PviewAllLearningMaterials(cId);
                    List<L2PLearningMaterialElement> learningMaterials = new List<L2PLearningMaterialElement>();
                    if (lmList.dataSet != null)
                    {

                        string sourceDirectory = "/" + course.semester + "/" + course.uniqueid + "/Lists/StructuredMaterials";
                        if (ExtdDir != null)
                        {
                            var element = from elts in lmList.dataSet
                                          where elts.isDirectory == true && elts.name.Equals(ExtdDir)
                                          select elts;
                            if (element.Any())
                            {
                                sourceDirectory = element.First().selfUrl;
                            }
                        }
                        var materials = from elts in lmList.dataSet
                                        where elts.sourceDirectory.Equals(sourceDirectory)
                                        orderby elts.isDirectory descending
                                        select elts;
                        learningMaterials = materials.ToList();
                        ViewData["CurrentDirectory"] = sourceDirectory;
                    }
                    ViewData["Message"] = msg;
                    ViewData["CourseLearningMaterials"] = learningMaterials;
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


        [HttpGet] // Get Method to show the shared documents material of a course.
        public async Task<ActionResult> SharedDocuments(string cId, string ExtdDir, string msg)
        {

            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["ChosenCourse"] = course;
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    L2PLearningMaterialList sdList = await L2PAPIClient.api.Calls.L2PviewAllSharedDocuments(cId);
                    List<L2PLearningMaterialElement> sharedDocuments = new List<L2PLearningMaterialElement>();
                    if (sdList.dataSet != null)
                    {
                        string sourceDirectory = "/" + course.semester + "/" + course.uniqueid + "/collaboration/Lists/SharedDocuments";
                        if (ExtdDir != null)
                        {
                            var element = from elts in sdList.dataSet
                                          where elts.isDirectory == true && elts.name.Equals(ExtdDir)
                                          select elts;
                            if (element.Any())
                            {
                                sourceDirectory = element.First().selfUrl;
                            }
                        }
                        var materials = from elts in sdList.dataSet
                                        where elts.sourceDirectory.Equals(sourceDirectory)
                                        orderby elts.isDirectory descending
                                        select elts;
                        sharedDocuments = materials.ToList();
                        ViewData["CurrentDirectory"] = sourceDirectory;
                    }
                    ViewData["Message"] = msg;
                    ViewData["CourseSharedDocuments"] = sharedDocuments;
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

        [HttpGet] // Get Method to show all the hyperlinks of a course
        public async Task<ActionResult> Hyperlinks(string cId, string msg)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    L2PHyperlinkList hpList = await L2PAPIClient.api.Calls.L2PviewAllHyperlinks(cId);
                    List<L2PHyperlinkElement> hyperlinks = new List<L2PHyperlinkElement>();
                    if (hpList.dataSet != null)
                    {
                        hyperlinks = hpList.dataSet;
                    }

                    ViewData["Message"] = msg;
                    ViewData["CourseHyperlinks"] = hyperlinks;
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

        [HttpGet] // Get Method to show all the hyperlinks of a course
        public async Task<ActionResult> Literature(String cId)
        {

            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    L2PLiteratureSetDataType LList = await L2PAPIClient.api.Calls.L2PviewAllLiteratureAsync(cId);
                    // L2PLiteratureViewDataType LVList = new L2PLiteratureViewDataType();

                    List<L2PLiteratureElementDataType> literatures = new List<L2PLiteratureElementDataType>();
                    if (LList != null)
                    {
                        //L2PLiteratureElementDataType
                        literatures = LList.dataSet;
                    }
                    ViewData["CourseLiterature"] = literatures;
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


        [HttpGet] // Get Method to show all the hyperlinks of a course
        public async Task<ActionResult> Assignments(String cId, string msg)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    L2PAssignmentList assnList = await L2PAPIClient.api.Calls.L2PviewAllAssignments(cId);
                    List<L2PAssignmentElement> assignments = new List<L2PAssignmentElement>();
                    if (assnList.dataSet != null)
                    {
                        assignments = assnList.dataSet;
                        // Sort by publish date desc
                        assignments.Sort((x, y) => y.assignmentPublishDate.CompareTo(x.assignmentPublishDate));
                    }
                    ViewData["Assignments"] = assignments;
                    ViewData["Message"] = msg;
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

        [HttpGet] // Get Method to show the Media Library of a course.
        public async Task<ActionResult> MediaLibrary(string cId, string ExtdDir, string msg)
        {


            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["ChosenCourse"] = course;
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    L2PMediaLibraryList mdList = await L2PAPIClient.api.Calls.L2PviewAllMediaLibraries(cId);
                    List<L2PMediaLibraryElement> mediaLibrary = new List<L2PMediaLibraryElement>();
                    if (mdList.dataSet != null)
                    {
                        string sourceDirectory = "/" + course.semester + "/" + course.uniqueid + "/Lists/MediaLibrary";
                        if (ExtdDir != null)
                        {
                            var element = from elts in mdList.dataSet
                                          where elts.isDirectory == true && elts.name.Equals(ExtdDir)
                                          select elts;
                            if (element.Any())
                            {
                                sourceDirectory = element.First().selfUrl;
                            }
                        }
                        var materials = from elts in mdList.dataSet
                                        where elts.sourceDirectory.Equals(sourceDirectory)
                                        orderby elts.isDirectory descending
                                        select elts;
                        mediaLibrary = materials.ToList();
                        ViewData["CurrentDirectory"] = sourceDirectory;
                    }
                    ViewData["Message"] = msg;
                    ViewData["CourseMediaLibrary"] = mediaLibrary;
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
        public async Task<ActionResult> Announcement(String cId, String msg)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    L2PAnnouncementList aList = await L2PAPIClient.api.Calls.L2PviewAllAnnouncements(cId);
                    List<L2PAnnouncementElement> announcements = new List<L2PAnnouncementElement>();
                    if (aList.dataSet != null)
                    {
                        announcements = aList.dataSet;
                    }
					ViewData["Message"] = msg;
                    ViewData["CourseAnnouncements"] = announcements;
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
        public async Task<ActionResult> Email(String cId, String msg)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    L2PEmailList eList = await L2PAPIClient.api.Calls.L2PviewAllEmails(cId);
                    List<L2PEmailElement> emails = new List<L2PEmailElement>();
                    if (eList.dataSet != null)
                    {
                        emails = eList.dataSet;

                    }
					ViewData["Message"] = msg;
                    ViewData["CourseEmails"] = emails;
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

        // Function used to download files from the L2P Client API
        public ActionResult Downloads(string cId, string url, string filename)
        {
            try
            {
                if (url != null && !url.StartsWith("|"))
                    url = "|" + url;
                string callURL = Config.L2PEndPoint + "/downloadFile/" + filename + "?accessToken=" + Config.getAccessToken() + "&cid=" + cId + "&downloadUrl=" + url;
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(callURL);
                myHttpWebRequest.MaximumAutomaticRedirections = 1;
                myHttpWebRequest.AllowAutoRedirect = true;
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                return File(myHttpWebResponse.GetResponseStream(), myHttpWebResponse.ContentType, filename);
            }
            catch (Exception ex)
            {
                ViewData["error"] = ex.Message;
                return RedirectToAction(nameof(HomeController.Error), "Error");
            }
        }

        // Function used to download files from the L2P Client API
        public ActionResult DownloadAssignment(string cId, string url, string filename)
        {
            try
            {
                string callURL = Config.L2PEndPoint + "/downloadFile/" + filename + "?accessToken=" + Config.getAccessToken() + "&cid=" + cId + "&downloadUrl=" + url;
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(callURL);
                myHttpWebRequest.MaximumAutomaticRedirections = 1;
                myHttpWebRequest.AllowAutoRedirect = true;
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                return File(myHttpWebResponse.GetResponseStream(), myHttpWebResponse.ContentType, filename);
            }
            catch (Exception ex)
            {
                ViewData["error"] = ex.Message;
                return RedirectToAction(nameof(HomeController.Error), "Error");
            }
        }

        public async Task<ActionResult> DownloadsZip(string caid, string aid)
        {
            try
            {
                L2PAssignmentList assnList = await L2PAPIClient.api.Calls.L2PviewAssignment(caid, int.Parse(aid));
                List<L2PAssignmentElement> assignments = new List<L2PAssignmentElement>();
                if (assnList.dataSet != null)
                {
                    assignments = assnList.dataSet;
                }

                string tempFilePath = Path.GetTempPath() + "Grp.L2PSite.MobileApp" + DateTime.Now.ToString("dd.MM.yyyy.hh_mm_ss");
                string tempDownloadFilesPath = tempFilePath + "/downloadFiles/";
                string tempZipPath = tempFilePath + "/downloadZip/";

                if (!Directory.Exists(tempDownloadFilesPath))
                {
                    Directory.CreateDirectory(tempDownloadFilesPath);
                }
                if (!Directory.Exists(tempZipPath))
                {
                    Directory.CreateDirectory(tempZipPath);
                }

                foreach (L2PAssignmentElement a in assignments)
                {
                    List<L2PAttachmentElement> filePaths = a.assignmentDocuments;
                    
                    if (filePaths != null)
                    {
                        foreach (L2PAttachmentElement filePath in filePaths)
                        {
                            string callURL = Config.L2PEndPoint + "/downloadFile/" + filePath.fileName + "?accessToken=" + Config.getAccessToken() + "&cid=" + caid + "&downloadUrl=" + filePath.downloadUrl;
                            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(callURL);
                            myHttpWebRequest.MaximumAutomaticRedirections = 1;
                            myHttpWebRequest.AllowAutoRedirect = true;
                            HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                            FileStreamResult file = File(myHttpWebResponse.GetResponseStream(), myHttpWebResponse.ContentType, filePath.fileName);

                            Stream s = myHttpWebResponse.GetResponseStream();
                            FileStream s1 = new FileStream(tempDownloadFilesPath + filePath.fileName, FileMode.Create);
                            Tools.CopyStream(s, s1);
                            s1.Close();
                        }

                        FileStream fsOut = System.IO.File.Create(tempZipPath + a.title + ".zip");
                        ZipOutputStream zipStream = new ZipOutputStream(fsOut);

                        zipStream.SetLevel(3); //0-9, 9 being the highest level of compression

                        // This setting will strip the leading part of the folder path in the entries, to
                        // make the entries relative to the starting folder.
                        // To include the full path for each entry up to the drive root, assign folderOffset = 0.
                        int folderOffset = tempDownloadFilesPath.Length + 0;

                        Tools.CompressFolder(tempDownloadFilesPath, zipStream, folderOffset);

                        zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
                        zipStream.Close();

                        //Wait 10 seconds to delete the files in temp folder
                        Task t = Task.Run(async delegate
                        {
                            await Task.Delay(TimeSpan.FromSeconds(10));
                            Directory.Delete(tempDownloadFilesPath, true);
                            Directory.Delete(tempZipPath, true);
                            Directory.Delete(tempFilePath, true);
                        });
                        return File(tempZipPath + a.title + ".zip", "application/zip", a.title + ".zip");
                    }
                    else
                    {
                        return RedirectToAction(nameof(MyCoursesController.Assignments), "MyCourses", new { @cId = caid, @msg = "No Files to download." });
                    }

                }
                return RedirectToAction(nameof(MyCoursesController.Assignments), "MyCourses");
            }
            catch (Exception ex)
            {
                ViewData["error"] = ex.Message;
                return RedirectToAction(nameof(HomeController.Error), "Error");
            }
        }
        [HttpGet] // Get Method to show group Workspace
        public async Task<ActionResult> GroupWorkspace(string cId, string msg)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole.role.Contains("managers") || userRole.role.Contains("tutors"))
                    {
                        L2PAvailableGroups availableGroups = await L2PAPIClient.api.Calls.L2PviewAvailableGroupsInGroupWorkspace(cId);
                        List<L2PgwsElement> allGroups = availableGroups.dataSet;
                        ViewData["allGroups"] = allGroups;
                    }
                    else if (userRole.role.Contains("students") || userRole.role.Contains("extra"))
                    {
                        L2PgwsMyGroupWorkspace g = await L2PAPIClient.api.Calls.L2PviewMyGroupWorkspace(cId);

                        List<L2PgwsElement> groupList = g.dataSet;
                        ViewData["groupInfo"] = groupList;
                        ViewData["groupInvites"] = g.invitationFromOtherUsers;
                        

                    }
                    ViewData["Message"] = msg;
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
        [HttpGet] // Get Method to leave group
        public async Task<ActionResult> LeaveGroup(string cId, int groupId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    if (String.IsNullOrEmpty(cId))
                    {
                        string errorMessage = "You were redirected to this page with missing parameters.<br/> Please go back to the home page and try again.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }


                    await L2PAPIClient.api.Calls.L2PgwsLeaveGroup(cId, groupId);
                    return RedirectToAction(nameof(MyCoursesController.GroupWorkspace), "MyCourses", new { @cId = cId, @msg = "You have successfully exited the group." });


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
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddUsers(string cId, int groupId, GroupWorkSpaceViewModel model)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PgwsInvite invite = new L2PgwsInvite();
                    invite.emails = model.ListOfUsers.Replace(",", ";") + ";";
                    L2PgwsMyGroupWorkspace g = await L2PAPIClient.api.Calls.L2PviewMyGroupWorkspace(cId);
                    List<L2PgwsElement> groupList = g.dataSet;
                    int groupListCount = groupList.Count;
                    L2PgwsElement groupInfo = new L2PgwsElement();
                    if (groupListCount > 0)
                    {
                        var element = from elts in groupList
                                      where elts.groupId == groupId
                                      select elts;
                        if (element.Any())
                            groupInfo = element.First();
                        else
                        {
                            return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = "Group id doesn't exist" });
                        }

                        invite.systemGeneratedAlias = groupInfo.systemGeneratedAlias;
                        invite.comment = "This is an invite to the group " + groupInfo.systemGeneratedAlias;
                    }
                    if (invite.systemGeneratedAlias != null)
                        await L2PAPIClient.api.Calls.L2PgwsInviteMemberInGroup(cId, invite);
                    return RedirectToAction(nameof(MyCoursesController.GroupWorkspace), "MyCourses", new { @cId = cId, @msg = "You have successfully invited the member(s)." });


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
        public async Task<ActionResult> AcceptOrRejectInvite(string cId, int itemId, string response)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    if (response != null)
                    {
                        await L2PAPIClient.api.Calls.L2PgwsRespondToInvitation(cId, itemId, response);
                    }
                    return RedirectToAction(nameof(MyCoursesController.GroupWorkspace), "MyCourses", new { @cId = cId, @msg = "You have successfully responded to the invite." });
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
        public async Task<ActionResult> DiscussionForum(string cId, string ExtdDir)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, HttpContext);
                if (Tools.isUserLoggedInAndAPIActive(HttpContext) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["ChosenCourse"] = course;
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    L2PDiscussionItemList discussList = await L2PAPIClient.api.Calls.L2PviewAllDiscussionItems(cId);

                    var discuss = from elem in discussList.dataSet
                                  where elem.subject != "Reply"
                                  orderby Tools.toDateTime(elem.created) descending
                                  select elem;
                    ViewData["DIscussionForum"] = discuss.ToList();
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