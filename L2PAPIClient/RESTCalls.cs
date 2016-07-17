using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using Newtonsoft.Json;
using L2PAPIClient.DataModel;
using System.Threading;

namespace L2PAPIClient.api
{
    public class Calls
    {
        #region generic calls

        public static async Task<string> GetAsync(string url)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
            client.DefaultRequestHeaders.IfModifiedSince = DateTimeOffset.Now;
            string result = await client.GetStringAsync(url);
            return result;
        }

        public static async Task<byte[]> GetAsyncByteArray(string url)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
            client.DefaultRequestHeaders.IfModifiedSince = DateTimeOffset.Now;
            var result = await client.GetByteArrayAsync(url);
            return result;
        }


        public static async Task<string> PostAsync(string url, string data)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
            client.DefaultRequestHeaders.IfModifiedSince = DateTimeOffset.Now;

            StringContent content = new StringContent(data);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(data, Encoding.UTF8, "application/json");

            //HttpResponseMessage response = await client.PostAsync(url, content);
            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();
            return result;

        }

        /// <summary>
        /// A generic REST-Call to an endpoint using GET or POST method
        /// 
        /// Uses a WebRequest for POST, a httpClient for GET calls
        /// 
        /// throws an Exception in Case of Error
        /// </summary>
        /// <typeparam name="T1">The Datatype to await for response</typeparam>
        /// <param name="input">the data as string (ignored, if using GET)</param>
        /// <param name="endpoint">The REST-Endpoint to call</param>
        /// <param name="post">A flag indicating whether to use POST or GET</param>
        /// <returns>The datatype that has been awaited for the call or default(T1) on error</returns>
        public async static Task<T1> RestCallAsync<T1>(string input, string endpoint, bool post)
        {

            if (post)
            {
                var answerCall = await PostAsync(endpoint, input);
                T1 answer = JsonConvert.DeserializeObject<T1>(answerCall);
                return answer;
            }
            else
            {
                var answerCall = await GetAsync(endpoint);
                T1 answer = JsonConvert.DeserializeObject<T1>(answerCall);
                return answer;
            }

        }


        #endregion

        /**
         * 
         * For Desciption of the API-Calls refer to the API Documentation
         * 
         * *****/

        #region L2P Misc. API Calls

        /// <summary>
        /// Calls the Ping-API of the L2P
        /// </summary>
        /// <param name="ping">a sample text that should be returned</param>
        /// <returns>The result of the call</returns>
        public async static Task<string> L2PPingCallAsync(string ping)
        {
            // Check Auth.
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/Ping?accessToken=" + Config.getAccessToken() + "&p=" + ping;
            var answer = await RestCallAsync<L2PPingData>("", callURL, false);
            return answer.comment;
        }

        public async static Task<L2PProvideAssignmentSolutionResponse> L2PprovideAssignmentSolution(string cid, int assignmentid, string gws_name_alias, L2PAssignmentSolutionRequest data)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/provideAssignmentSolution?accessToken=" + Config.getAccessToken() + "&cid=" + cid
                    + "&assignmentid=" + assignmentid + "&gws_name_alias=" + gws_name_alias;

                //string postData = JsonConvert.SerializeObject(data);
                var answer = await RestCallAsync<L2PProvideAssignmentSolutionResponse>(data.ToString(), callURL, true);
                return answer;
            }
            catch
            {
                return new L2PProvideAssignmentSolutionResponse();
            }
        }

        public async static Task<L2PWhatsNewDataType> L2PwhatsNewAsync(string cid)
        {
            try
            {
                // Check Auth.
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/whatsNew?accessToken=" + Config.getAccessToken() + "&cid=" + cid;
                var answer = await RestCallAsync<L2PWhatsNewDataType>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PWhatsNewDataType();
            }
        }

        public async static Task<L2PWhatsNewDataType> L2PwhatsNewSinceAsync(string cid, int pastMinutes)
        {
            try
            {
                // Check Auth.
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/whatsNewSince?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&pastMinutes=" + pastMinutes;
                var answer = await RestCallAsync<L2PWhatsNewDataType>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PWhatsNewDataType();
            }
        }

        public async static Task<L2PWhatsAllNewDataType> L2PwhatsAllNewSinceAsync(int pastMinutes)
        {
            try
            {
                // Check Auth.
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/whatsAllNewSince?accessToken=" + Config.getAccessToken() + "&pastMinutes=" + pastMinutes;
                var answer = await RestCallAsync<L2PWhatsAllNewDataType>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PWhatsAllNewDataType();
            }
        }

        public async static Task<L2PWhatsAllNewDataType> L2PwhatsAllNewSinceForSemesterAsync(string semester, int pastMinutes)
        {
            try
            {
                // Check Auth.
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/whatsAllNewSinceForSemester?accessToken=" + Config.getAccessToken() + "&pastMinutes=" + pastMinutes + "&semester=" + semester;
                var answer = await RestCallAsync<L2PWhatsAllNewDataType>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PWhatsAllNewDataType();
            }
        }

        #endregion

        #region L2P Count-Calls

        public async static Task<L2PCountViewDataType> L2PviewAllAnnouncementCountAsync(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllAnnouncementCount?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PCountViewDataType>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PCountViewDataType();
            }
        }

        public async static Task<L2PCountViewDataType> L2PviewAllDiscussionitemCountAsync(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllDiscussionItemCount?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PCountViewDataType>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PCountViewDataType();
            }
        }

        public async static Task<L2PCountViewDataType> L2PviewAllHyperlinkCountAsync(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllHyperlinkCount?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PCountViewDataType>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PCountViewDataType();
            }
        }

        public async static Task<L2PCountViewDataType> L2PviewAllLiteratureCountAsync(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllLiteratureCount?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PCountViewDataType>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PCountViewDataType();
            }
        }

        public async static Task<L2PCountViewDataType> L2PviewAllMediaLibraryCountAsync(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllMediaLibraryCount?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PCountViewDataType>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PCountViewDataType();
            }
        }

        public async static Task<L2PCountViewDataType> L2PviewAllSharedDocumentCountAsync(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllSharedDocumentCount?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PCountViewDataType>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PCountViewDataType();
            }
        }

        public async static Task<L2PCountViewDataType> L2PviewAllWikiCountAsync(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllWikiCount?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PCountViewDataType>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PCountViewDataType();
            }
        }

        public async static Task<L2PCountViewDataType> L2PviewLearningMaterialCountAsync(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewLearningMaterialCount?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PCountViewDataType>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PCountViewDataType();
            }
        }

        public async static Task<L2PViewAllCountDataType> L2PviewAllCountCountAsync(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllCount?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PViewAllCountDataType>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PViewAllCountDataType();
            }
        }

        #endregion

        #region L2P View Calls

        /// <summary>
        /// Workaround:
        /// Check the Token for being valid by calling the L2P Api (Only in case of errors of the tokeninfo-endpoint)
        /// </summary>
        /// <returns>true, if the token is valid</returns>
        public async static Task<bool> CheckValidTokenAsync()
        {
            try
            {
                string callURL = Config.L2PEndPoint + "/Ping?accessToken=" + Config.getAccessToken() + "&p=ping";
                var answer = await RestCallAsync<L2PPingData>("", callURL, false);
                if ((answer == null) || (answer.status == false))
                    return false;
                return true;
            }catch
            {
                return false;
            }
        }

        public async static Task<L2PViewActiveFeaturesDataType> L2PviewActiveFeaturesAsync(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewActiveFeatures?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PViewActiveFeaturesDataType>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PViewActiveFeaturesDataType();
            }
        }

        //public async static Task<L2PLiteratureViewDataType> L2PviewLiteratureAsync(string cid, int itemid)
        //{
        //    try
        //    {
        //        await AuthenticationManager.CheckAccessTokenAsync();
        //        string callURL = Config.L2PEndPoint + "/viewLiterature?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid.ToString();

        //        var answer = await RestCallAsync<L2PLiteratureViewDataType>("", callURL, false);
        //        return answer;
        //    }
        //    catch
        //    {
        //        return new L2PLiteratureViewDataType();
        //    }
        //}

        public async static Task<L2PLiteratureSetDataType> L2PviewLiteratureAsync(string cid, int itemid)
        {//added 
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewLiterature?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid.ToString();


                var answer = await RestCallAsync<L2PLiteratureSetDataType>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PLiteratureSetDataType();
            }
        }

        public async static Task<L2PLiteratureSetDataType> L2PviewAllLiteratureAsync(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllLiterature?accessToken=" + Config.getAccessToken() + "&cid=" + cid; ///changed from view Literature to view all literature 

                var answer = await RestCallAsync<L2PLiteratureSetDataType>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PLiteratureSetDataType();
            }
        }

        /// <summary>
        /// Gets the Course Info for the provided Course
        /// </summary>
        /// <param name="cid">The course room id (14ss-xxxxx)</param>
        /// <returns>A representation of the course room or null, if no data was available</returns>
        public async static Task<L2PCourseInfoData> L2PviewCourseInfoAsync(string cid)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/viewCourseInfo?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

            var answer = await RestCallAsync<L2PCourseInfoSetData>("", callURL, false);
            if (answer.dataset.Count == 0)
            {
                // no elements!
                return null;
            }
            return answer.dataset[0];
        }


        /// <summary>
        /// Get all Courses of the user
        /// </summary>
        /// <returns>A representation of all courses</returns>
        public async static Task<L2PCourseInfoSetData> L2PviewAllCourseInfoAsync()
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllCourseInfo?accessToken=" + Config.getAccessToken();

                var answer = await RestCallAsync<L2PCourseInfoSetData>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PCourseInfoSetData();
            }
        }

        /// <summary>
        /// Gets all courses of the specified semester
        /// </summary>
        /// <param name="semester">the semester specifier (e.g. 14ss)</param>
        /// <returns>A representation of all courses of the semester</returns>
        public async static Task<L2PCourseInfoSetData> L2PviewAllCourseIfoBySemesterAsync(string semester)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllCourseInfoBySemester?accessToken=" + Config.getAccessToken() + "&semester=" + semester;

                var answer = await RestCallAsync<L2PCourseInfoSetData>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PCourseInfoSetData();
            }
        }

        public static Task L2PprovideAssignmentSolution(string cId, int assignID, string groupAlias, L2PAssignmentSolution sol)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Gets the Role of a user inside the coure room
        /// </summary>
        /// <param name="cid">the course room</param>
        /// <returns>A Role representation</returns>
        public async static Task<L2PRole> L2PviewUserRoleAsync(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewUserRole?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PRole>("", callURL, false);
                return answer;
            }
            catch 
            {
                return new L2PRole();
            }
        }

        public async static Task<L2PAssignmentList> L2PviewAllAssignments(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllAssignments?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PAssignmentList>("", callURL, false);
                return answer;
            }
            catch{
                return new L2PAssignmentList();
            }
        }

        public async static Task<L2PAnnouncementList> L2PviewAllAnnouncements(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllAnnouncements?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PAnnouncementList>("", callURL, false);
                return answer;
            }
            catch {
                return new L2PAnnouncementList();
            }
        }

        public async static Task<L2PCourseEventList> L2PviewAllCourseEvents()
        {
            try
            { 
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllCourseEvents?accessToken=" + Config.getAccessToken();

                var answer = await RestCallAsync<L2PCourseEventList>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PCourseEventList();
            }
        }

        public async static Task<L2PCourseInfoSetData> L2PviewAllCourseInfoByCurrentSemester()
        {
            try
            {

                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllCourseinfoByCurrentSemester?accessToken=" + Config.getAccessToken();

                var answer = await RestCallAsync<L2PCourseInfoSetData>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PCourseInfoSetData();
            }
        }

        public async static Task<L2PDiscussionItemList> L2PviewAllDiscussionItems(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllDiscussionItems?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PDiscussionItemList>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PDiscussionItemList();
            }
        }

        public async static Task<L2PDiscussionItemList> L2PviewAllDiscussionRootItems(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllDiscussionRootItems?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PDiscussionItemList>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PDiscussionItemList();
            }
        }

        public async static Task<L2PEmailList> L2PviewAllEmails(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllEmails?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PEmailList>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PEmailList();
            }
        }

        public async static Task<L2PHyperlinkList> L2PviewAllHyperlinks(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllHyperlinks?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PHyperlinkList>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PHyperlinkList();
            }
        }

        public async static Task<L2PLearningMaterialList> L2PviewAllLearningMaterials(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllLearningMaterials?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PLearningMaterialList>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PLearningMaterialList();
            }
        }

        public async static Task<L2PMediaLibraryList> L2PviewAllMediaLibraries(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllMediaLibraries?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PMediaLibraryList>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PMediaLibraryList();
            }
        }

        public async static Task<L2PLearningMaterialList> L2PviewAllSharedDocuments(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllSharedDocuments?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PLearningMaterialList>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PLearningMaterialList();
            }
        }

        public async static Task<L2PWikiList> L2PviewAllWikis(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllWikis?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PWikiList>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PWikiList();
            }
        }

        public async static Task<L2PAnnouncementList> L2PviewAnnouncement(string cid, int itemid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAnnouncement?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

                var answer = await RestCallAsync<L2PAnnouncementList>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PAnnouncementList();
            }
        }

        public async static Task<L2PAssignmentList> L2PviewAssignment(string cid, int itemid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAssignment?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

                var answer = await RestCallAsync<L2PAssignmentList>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PAssignmentList();
            }
        }

        public async static Task<L2PCourseEventList> L2PviewCourseEvents(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewCourseEvents?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PCourseEventList>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PCourseEventList();
            }
        }

        public async static Task<L2PDiscussionItemList> L2PviewDiscussionItem(string cid, int itemid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewDiscussionItem?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

                var answer = await RestCallAsync<L2PDiscussionItemList>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PDiscussionItemList();
            }
        }

        public async static Task<L2PEmailList> L2PviewEmail(string cid, int itemid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewEmail?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

                var answer = await RestCallAsync<L2PEmailList>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PEmailList();
            }
        }

        public async static Task<L2PHyperlinkList> L2PviewHyperlink(string cid, int itemid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewHyperlink?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

                var answer = await RestCallAsync<L2PHyperlinkList>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PHyperlinkList();
            }
        }

        public async static Task<L2PLearningMaterialList> L2PviewLearningMaterial(string cid, int itemid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewLearningMaterial?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

                var answer = await RestCallAsync<L2PLearningMaterialList>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PLearningMaterialList();
            }
        }

        public async static Task<L2PMediaLibraryList> L2PviewMediaLibrary(string cid, int itemid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewMediaLibrary?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

                var answer = await RestCallAsync<L2PMediaLibraryList>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PMediaLibraryList();
            }
        }

        public async static Task<L2PWikiList> L2PviewWiki(string cid, int itemid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewWiki?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

                var answer = await RestCallAsync<L2PWikiList>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PWikiList();
            }
        }


        public async static Task<L2PWikiList> L2PviewWikiVersion(string cid, int itemid, int versionid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewWikiVersion?accessToken=" + Config.getAccessToken() + "&cid=" + cid
                    + "&itemid=" + itemid + "&versionid=" + versionid;

                var answer = await RestCallAsync<L2PWikiList>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PWikiList();
            }
        }

        public async static Task<L2PLearningObjectViewDataType> L2PviewLearningObject(string cid, int itemid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewLearningObject?accessToken=" + Config.getAccessToken() + "&cid=" + cid
                    + "&itemid=" + itemid;

                var answer = await RestCallAsync<L2PLearningObjectViewDataType>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PLearningObjectViewDataType();
            }
        }

        public async static Task<L2PLearningObjectViewDataType> L2PviewAllLearningObjects(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewAllLearningObject?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PLearningObjectViewDataType>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PLearningObjectViewDataType();
            }
        }


        public async static Task<L2PExamResultViewDataType> L2PviewExamResults(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewExamResults?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PExamResultViewDataType>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PExamResultViewDataType();
            }
        }

        public async static Task<L2PGradeDistributionViewDataType> L2PviewExamResultsStatistics(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewExamResultsStatistics?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PGradeDistributionViewDataType>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PGradeDistributionViewDataType();
            }
        }

        public async static Task<L2PGradeBookResultViewDataType> L2PviewGradeBook(string cid)
        {
            try
            {
                await AuthenticationManager.CheckAccessTokenAsync();
                string callURL = Config.L2PEndPoint + "/viewGradeBook?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

                var answer = await RestCallAsync<L2PGradeBookResultViewDataType>("", callURL, false);
                return answer;
            }
            catch
            {
                return new L2PGradeBookResultViewDataType();
            }
        }

        #endregion

        #region L2P Add Calls

        public async static Task<L2PAddUpdateResponse> L2PAddLiterature(string cid, L2PLiteratureAddRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/addLiterature?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PAddUpdateResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PAddUpdateResponse> L2PAddAnnouncement(string cid, L2PAddAnnouncementRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/addAnnouncement?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PAddUpdateResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PAddUpdateResponse> L2PAddAssignment(string cid, L2PAddAssignmentRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/addAssignment?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PAddUpdateResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PAddUpdateResponse> L2PAddCourseEvents(string cid, L2PAddCourseEventRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/addCourseEvents?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PAddUpdateResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PAddUpdateResponse> L2PAddDiscussionThread(string cid, L2PAddDiscussionThreadRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/addDiscussionThread?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PAddUpdateResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PAddUpdateResponse> L2PAddDiscussionThreadReply(string cid, int replyToId, L2PAddDiscussionThreadReplyRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/addDiscussionThreadReply?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&replyToId=" + replyToId;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PAddUpdateResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PAddUpdateResponse> L2PAddEmail(string cid, L2PAddEmailRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/addEmail?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PAddUpdateResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PAddUpdateResponse> L2PAddHyperlink(string cid, L2PAddHyperlinkRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/addHyperlink?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PAddUpdateResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PAddUpdateResponse> L2PAddWiki(string cid, L2PAddWikiRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/addWiki?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PAddUpdateResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PCreateFolderResponse> L2PCreateFolder(string cid, int moduleNumber, string desiredFolderName, string sourceDirectory)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/createFolder?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&moduleNumber=" + moduleNumber.ToString()
                + "&desiredFolderName=" + desiredFolderName + "&sourceDirectory=" + sourceDirectory;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PCreateFolderResponse>(null, callURL, false);
            return answer;
        }

        public async static Task<L2PAddUpdateResponse> L2PAddLearningObject(string cid, L2PLearningObjectAddDataType data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/addLearningObject?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PAddUpdateResponse>(data.ToString(), callURL, false);
            return answer;
        }

        #endregion

        #region L2P Update Calls

        public async static Task<L2PAddUpdateResponse> L2PupdateAnnouncement(string cid, int itemid, L2PAddAnnouncementRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/updateAnnouncement?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PAddUpdateResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PAddUpdateResponse> L2PupdateLiterature(string cid, int itemid, L2PLiteratureAddRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/updateLiterature?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PAddUpdateResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PAddUpdateResponse> L2PupdateCourseEvents(string cid, int itemid, L2PAddCourseEventRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/updateCourseEvents?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PAddUpdateResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PAddUpdateResponse> L2PupdateDiscussionThread(string cid, int selfid, L2PAddDiscussionThreadRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/updateDiscussionThread?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&selfid=" + selfid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PAddUpdateResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PAddUpdateResponse> L2PupdateDiscussionThreadReply(string cid, int selfid, L2PAddDiscussionThreadReplyRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/updateDiscussionThreadReply?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&selfid=" + selfid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PAddUpdateResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PAddUpdateResponse> L2PupdateHyperlink(string cid, int itemid, L2PAddHyperlinkRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/updateHyperlink?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PAddUpdateResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PAddUpdateResponse> L2PupdateWiki(string cid, int itemid, L2PAddWikiRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/updateWiki?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PAddUpdateResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PAddUpdateResponse> L2PupdateLearningObject(string cid, int itemid, L2PLearningObjectAddDataType data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/updateLearningObject?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PAddUpdateResponse>(data.ToString(), callURL, true);
            return answer;
        }

        #endregion

        #region L2P Deletion Calls

        public async static Task<L2PBaseActionResponse> L2PDeleteAnnouncement(string cid, int itemid)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/deleteAnnouncement?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(null, callURL, false);
            return answer;
        }

        public async static Task<L2PBaseActionResponse> L2PDeleteHyperlink(string cid, int itemid)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/deleteHyperlink?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(null, callURL, false);
            return answer;
        }

        public async static Task<L2PBaseActionResponse> L2PDeleteLiterature(string cid, int itemid)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/deleteLiterature?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(null, callURL, false);
            return answer;
        }

        public async static Task<L2PBaseActionResponse> L2PDeleteAssignment(string cid, int itemid)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/deleteAssignment?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(null, callURL, false);
            return answer;
        }

        public async static Task<L2PBaseActionResponse> L2PDeleteAssignmentSolution(string cid, int itemid)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/deleteAssignmentSolution?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(null, callURL, false);
            return answer;
        }

        public async static Task<L2PBaseActionResponse> L2PDeleteCourseEvent(string cid, int itemid)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/deleteCourseEvent?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(null, callURL, false);
            return answer;
        }

        public async static Task<L2PBaseActionResponse> L2PDeleteDiscussionItem(string cid, int itemid)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/deleteDiscussionItem?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(null, callURL, false);
            return answer;
        }

        public async static Task<L2PBaseActionResponse> L2PDeleteEmail(string cid, int itemid)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/deleteEmail?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(null, callURL, false);
            return answer;
        }

        public async static Task<L2PBaseActionResponse> L2PDeleteLearningMaterial(string cid, int itemid)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/deleteLearningMaterial?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(null, callURL, false);
            return answer;
        }

        public async static Task<L2PBaseActionResponse> L2PDeleteMediaLibrary(string cid, int itemid)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/deleteMediaLibrary?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(null, callURL, false);
            return answer;
        }

        public async static Task<L2PBaseActionResponse> L2PDeleteSharedDocument(string cid, int itemid)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/deleteSharedDocument?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(null, callURL, false);
            return answer;
        }


        public async static Task<L2PBaseActionResponse> L2PDeleteWiki(string cid, int itemid)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/deleteWiki?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(null, callURL, false);
            return answer;
        }

        public async static Task<L2PBaseActionResponse> L2PDeleteLearningObject(string cid, int itemid)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/deleteLearningObject?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&itemid=" + itemid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(null, callURL, false);
            return answer;
        }

        #endregion

        #region L2P File Operations

        public async static Task<Stream> L2PdownloadFile(string cid, string downloadUrl, string filename)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/downloadFile/" + filename + "?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&downloadUrl=" + downloadUrl;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await GetAsyncByteArray(callURL);
            MemoryStream ms = new MemoryStream(answer);
            return ms;
        }

        public async static Task<L2PBaseActionResponse> L2PuploadInAnnouncements(string cid, string attachmentDirectory, L2PUploadRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/uploadInAnnouncement?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&attachmentDirectory=" + attachmentDirectory;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PBaseActionResponse> L2PuploadInAssignments(string cid, string solutionDirectory, L2PUploadRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/uploadInAssignments?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&solutionDirectory=" + solutionDirectory;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(data.ToString(), callURL, true);
            return answer;
        }

      
        [Obsolete]
        public async static Task<L2PBaseActionResponse> L2PuploadInEmail(string cid, L2PUploadRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/uploadInEmail?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PBaseActionResponse> L2PuploadInEmailId(string cid, int id, L2PUploadRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/uploadInEmailId?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&emailItemId=" + id.ToString();

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PBaseActionResponse> L2PuploadInLearningMaterials(string cid, string sourceDirectory, L2PUploadRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/uploadInLearningMaterials?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&sourceDirectory=" + sourceDirectory;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PBaseActionResponse> L2PuploadInMediaLibrary(string cid, string sourceDirectory, L2PUploadRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/uploadInMediaLibrary?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&sourceDirectory=" + sourceDirectory;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PBaseActionResponse> L2PuploadInSharedDocuments(string cid, string sourceDirectory, L2PUploadRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/uploadInSharedDocuments?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&sourceDirectory=" + sourceDirectory;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(data.ToString(), callURL, true);
            return answer;
        }

        #endregion

        #region L2P Group Workspace Management

        public async static Task<L2PBaseActionResponse> L2PgwsInviteMemberInGroup(string cid, L2PgwsInvite data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/gwsInviteMemberInGroup?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PBaseActionResponse> L2PgwsLeaveGroup(string cid, int groupid)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/gwsLeaveGroup?accessToken=" + Config.getAccessToken() + "&cid=" + cid + "&groupId=" + groupid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(null, callURL, false);
            return answer;
        }

        public async static Task<L2PBaseActionResponse> L2PgwsRespondToInvitation(string cid, int itemId, string action)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/gwsRespondToInvitation?accessToken=" + Config.getAccessToken() + "&cid=" + cid
                + "&itemId=" + itemId + "&action=" + action;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(null, callURL, false);
            return answer;
        }

        public async static Task<L2PBaseActionResponse> L2PgwsRespondToRequest(string cid, int itemId, string action)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/gwsRespondToRequest?accessToken=" + Config.getAccessToken() + "&cid=" + cid
                + "&itemId=" + itemId + "&action=" + action;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PBaseActionResponse>(null, callURL, false);
            return answer;
        }

        public async static Task<L2PAvailableGroups> L2PviewAvailableGroupsInGroupWorkspace(string cid)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/viewAvailableGroupsInGroupWorkspace?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

            var answer = await RestCallAsync<L2PAvailableGroups>("", callURL, false);
            return answer;
        }

        public async static Task<L2PgwsMyGroupWorkspace> L2PviewMyGroupWorkspace(string cid)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/viewMyGroupWorkspace?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

            var answer = await RestCallAsync<L2PgwsMyGroupWorkspace>("", callURL, false);
            return answer;
        }

        #endregion

        #region L2P Participants Management

        public async static Task<L2PRegisterResponse> L2PRegisterUser(string cid, L2PRegisterRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/registerParticipant?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PRegisterResponse>(data.ToString(), callURL, true);
            return answer;
        }

        public async static Task<L2PRegisterResponse> L2PDeregisterUser(string cid, L2PRegisterRequest data)
        {
            await AuthenticationManager.CheckAccessTokenAsync();
            string callURL = Config.L2PEndPoint + "/deregisterParticipant?accessToken=" + Config.getAccessToken() + "&cid=" + cid;

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await RestCallAsync<L2PRegisterResponse>(data.ToString(), callURL, true);
            return answer;
        }

        #endregion

    }
}

/*
 * This Application uses the Newtonsoft JSON package. This packages is licensed under MIT-License:
  
 The MIT License (MIT)

Copyright (c) 2007 James Newton-King

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

 */
