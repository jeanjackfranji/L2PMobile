using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace L2PAPIClient.DataModel
{
    #region OAuth-DataTypes

    /// <summary>
    /// The basic response on starting an authorization process (Call to /code )
    /// </summary>
    public class OAuthRequestData
    {
        public string status { get; set; }
        public string device_code { get; set; }
        public int expires_in { get; set; }
        public int interval { get; set; }
        public string verification_url { get; set; }
        public string user_code { get; set; }
    }

    /// <summary>
    /// The response on a authorization token request. (Call to /token )
    /// </summary>
    public class OAuthTokenRequestData
    {
        public string status { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public string error { get; set; }
    }

    /// <summary>
    /// The datatype that is send to the Token endpoint for checking authorization status
    /// </summary>
    public class OAuthTokenRequestSendData
    {
        public string client_id { get; set; }
        public string code { get; set; }
        public string grant_type { get; set; }
    }

    /// <summary>
    /// The response to the TokenInfo request (/tokeninfo)
    /// </summary>
    public class OAuthTokenInfo
    {
        public string status { get; set; }
        public string audience { get; set; }
        public string scope { get; set; }
        public int expires_in { get; set; }
        public string state { get; set; }
    }

    #endregion

    #region L2P API-DataTypes (answers)

    /// <summary>
    /// Basic Datatype. Stores the status of the request and eventual Error-Details
    /// </summary>
    public class L2PBaseData
    {
        public string errorDescription;
        public string errorId;
        public bool status;
    }

    public class L2PBaseActionResponse : L2PBaseData
    {
        public string comment;
    }

    /// <summary>
    /// Representation of a Course Room in L2P
    /// 
    /// REMARK: for now, status and courseStatus are disabled because the API will change at that point
    /// </summary>
    public class L2PCourseInfoData : L2PBaseData
    {
        public string uniqueid { get; set; }
        public string semester { get; set; }
        public string courseTitle { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        //public bool status { get; set; }
        public string courseStatus { get; set; }
        public int itemId { get; set; }
    }

    public class L2PCourseInfoSetData : L2PBaseData
    {
        public List<L2PCourseInfoData> dataset { get; set; }
    }

    /// <summary>
    /// Representation of a Ping
    /// </summary>
    public class L2PPingData : L2PBaseData
    {
        //public bool status { get; set; }
        public string comment { get; set; }
    }

    /// <summary>
    /// Representation of the user Role in a Course room
    /// </summary>
    public class L2PRole : L2PBaseData
    {
        public string role { get; set; }
    }

    public class L2PWikiElement
    {
        public int itemId;
        public string url;
        public string authors;
        public string editor;
        public List<int> versionIds;
        public long lastModified;
        public string title;
        public string body;
    }

    public class L2PWikiList : L2PBaseData
    {
        public List<L2PWikiElement> dataSet;
    }

    public class L2PgwsRequest
    {
        public int itemId;
        public string groupName;
        public string comment;
    }

    public class L2PgwsElement
    {
        public string systemGeneratedAlias;
        public int groupId;
        public List<String> members;
        public int memberCount;
        public string workspaceUrl;
        public string description;
        public string name;
    }

    public class L2PgwsMyGroupWorkspace : L2PBaseData
    {
        public List<L2PgwsRequest> invitationFromOtherUsers;
        public List<L2PgwsRequest> requestFromOtherUsers;
        public List<L2PgwsRequest> requestToOtherGroups;
        public List<L2PgwsElement> dataSet;
    }

    public class L2PgwsInvite : L2PBaseRequestData
    {
        public string emails;
        public string systemGeneratedAlias;
        public string comment;
    }

    public class L2PMediaLibraryElement
    {
        public string sourceDirectory;
        public string thumbnailUrl;
        public string name;
        public int itemId;
        public int parentFolderId;
        public bool isDirectory;
        public string selfUrl;
        public object fileInformation;
        public long created;
        public long lastModified;
    }

    public class L2PMediaLibraryList : L2PBaseData
    {
        public List<L2PMediaLibraryElement> dataSet;
    }

    public class L2PLearningMaterialElement
    {
        public string sourceDirectory;
        public int itemId;
        public string name;
        public int parentFolderId;
        public bool isDirectory;
        public string selfUrl;
        public object fileInformation;
        public long created;
        public long lastModified;
        public List<long> relatedLectureDates;
        public bool byMe;
    }

    public class L2PLearningMaterialList : L2PBaseData
    {
        public List<L2PLearningMaterialElement> dataSet;
    }

    public class L2PHyperlinkElement
    {
        public int itemId;
        public string url;
        public string notes;
        public string description;
        public long created;
        public long lastModified;
    }

    public class L2PHyperlinkList : L2PBaseData
    {
        public List<L2PHyperlinkElement> dataSet;
    }

    public class L2PAttachmentElement
    {
        public string downloadUrl;
        public int itemId;
        public string fileSize;
        public long modifiedTimestamp;
        public string fileName;
    }

    public class L2PEmailElement
    {
        public string from;
        public int itemId;
        public long modifiedTimestamp;
        public long created;
        public List<L2PAttachmentElement> attachments;
        public string recipients;
        public string cc;
        public string body;
        public string subject;
        public string replyTo;
    }

    public class L2PEmailList : L2PBaseData
    {
        public List<L2PEmailElement> dataSet;
    }

    public class L2PDiscussionItemElement
    {
        public string from;
        public bool byMe;
        public long modifiedTimestamp;
        public long created;
        public int selfId;
        public int replyToId;
        public int parentDiscussionId;
        public string subject;
        public string body;
    }

    public class L2PDiscussionItemList : L2PBaseData
    {
        public List<L2PDiscussionItemElement> dataSet;
    }

    public class L2PCourseEvent
    {
        public string createdBy;
        public string modifiedBy;
        public string courseID;
        public int itemID;
        // Yes, it is a typo in the API
        public bool isRecurringItem;
        public string title;
        public string contentType;
        public string location;
        public long eventDate;
        public long endDate;
        public bool allDay;
        public string description;
        public string category;
        public long created;
        public long lastModified;
    }

    public class L2PCourseEventList : L2PBaseData
    {
        public List<L2PCourseEvent> dataSet;
    }

    public class L2PAvailableGroups : L2PBaseData
    {
        public List<L2PgwsElement> dataSet;
    }

    public class L2PAssignmentElement
    {
        public int itemId;
        public string title;
        public string description;
        public double totalPoint;
        public long dueDate;
        public long assignmentPublishDate;
        public bool groupSubmissionAllowed;
        public List<L2PAttachmentElement> assignmentDocuments;
        public L2PAssignmentCorrection correction;
        public L2PAssignmentSolution solution;
        public List<L2PAttachmentElement> SampleSolutionDocuments;
    }

    public class L2PAssignmentCorrection
    {
        public List<L2PAttachmentElement> correctionDocuments;
        public long creationTimestamp;
        public long modifiedTimestamp;
        public double obtainedPoint;
        public string tutorComment;
    }

    public class L2PAssignmentSolution
    {
        public bool Status;
        public long creationTimestamp;
        public int itemId;
        public long modifiedTimestamp;
        public string solutionDirectory;
        public List<L2PAttachmentElement> solutionDocuments;
        public string studentComment;
        public List<string> submittedByStudents;
    }



    public class L2PAssignmentList : L2PBaseData
    {
        public List<L2PAssignmentElement> dataSet;
    }

    public class L2PFileInformationElement
    {
        public string downloadUrl;
        public string fileName;
        public int fileSize;
        public int itemId;
        public long modifiedTimestamp;

    }

    public class L2PAnnouncementElement
    {
        public List<L2PAttachmentElement> attachments;
        public long modifiedTimestamp;
        public int itemId;
        public string attachmentDirectory;
        public string title;
        public string body;
        public long expireTime;
        public long created;
    }

    public class L2PAnnouncementList : L2PBaseData
    {
        public List<L2PAnnouncementElement> dataSet;
    }

    public class L2PLiteratureSetDataType : L2PBaseData
    {
        public List<L2PLiteratureElementDataType> dataSet; //changed from L2PLiteratureViewDataType to L2PLiteratureElementDataType
    }

   
    public class L2PLiteratureElementDataType
    {
        public string state;
        public string availability;
        public int itemID;
        public long modified;
        public long created;
        public string author;
        public string editor;
        public string literatureCategory;
        //public List<FileInformationDataType> attachments;
        public List<object> attachments;
        public string title;
        public string authors;
        public string year;
        public string url;
        public string publisher;
        public string relevance;
        public string address;
        public string booktitle;
        public string comments;
        public string doi;
        public string edition;
        public string fromPage;
        public string isxn;
        public string journalName;
        public string number;
        public string role;
        public string series;
        public string toPage;
        public string type;
        public string volume;
        public string urlComment;
        public string contentType;
    }

    public class L2PLiteratureViewDataType : L2PBaseData
    {
        public List<L2PLiteratureElementDataType> dataset;
    }

    public class L2PCountViewDataType : L2PBaseData
    {
        public int count;
    }

    public class L2PViewAllCountDataType : L2PBaseData
    {
        public int DiscussionItems;
        public int Hyperlinks;
        public int Announcements;
        public int LearningMaterials;
        public int Wikis;
        public int SharedDocuments;
        public int MediaLibraryitems;
        public int LiteratureItems;
        public string failed;
    }

    public class L2PViewActiveFeaturesDataType : L2PBaseData
    {
        public List<string> active;
        public List<string> inactive;
    }

    public class L2PWhatsNewDataType : L2PBaseData
    {
        public List<L2PAnnouncementElement> announcements;
        public List<L2PAssignmentElement> assignments;
        public List<L2PDiscussionItemElement> discussionItems;
        public List<L2PEmailElement> emails;
        public List<L2PHyperlinkElement> hyperlinks;
        public List<L2PLiteratureElementDataType> literature;
        public List<L2PLearningMaterialElement> learningMaterials;
        public List<L2PMediaLibraryElement> mediaLibraries;
        public List<L2PLearningMaterialElement> sharedDocuments;
        public List<L2PWikiElement> wikis;
    }

    public class L2PWhatsNewExtendedDataType : L2PWhatsNewDataType
    {
        public string cid;
    }

    public class L2PWhatsAllNewDataType : L2PBaseData
    {
        List<L2PWhatsNewExtendedDataType> dataset;
    }

    public class L2PLearningObjectElement
    {
        public string title;
        public int itemId;
        public string description;
        public long created;
        public long lastModified;
        public List<L2PLiteratureElementDataType> relatedLiterature;
        public List<L2PLearningMaterialElement> relatedLearningMaterials;
        public List<L2PMediaLibraryElement> relatedMediaElements;
        public List<L2PHyperlinkElement> relatedHyperlinks;
        public List<long> relatedLectureDates;
    }

    public class L2PLearningObjectViewDataType : L2PBaseData
    {
        public List<L2PLearningObjectElement> dataset;
    }

    public class L2PExamResultDataType
    {
        public string matNr;
        public string firstname;
        public string lastname;
        public string grade;
        public string freiVermerk;
        public string pversuch;
        public string pvermerk;
        public bool isPublished;
        public string studentUser;
        //public Dictionary<string, string> customFields;
        public customKVList customFields;
    }

    public class L2PExamResultViewDataType : L2PBaseData
    {
        public List<L2PExamResultDataType> dataset;
    }

    public class L2PGradeBookViewDataType
    {
        public string student;
        public customKVList fields;
    }

    public class L2PGradeBookResultViewDataType : L2PBaseData
    {
        public List<L2PGradeBookViewDataType> dataSet;
    }

    public class L2PGradeViewDataType
    {
        public string grade;
        public int count;
    }

    public class L2PGradeDistributionViewDataType : L2PBaseData
    {
        public List<L2PGradeViewDataType> dataset;
    }

    #endregion

    public class customKVPair
    {
        public string Key;
        public string Value;
    }

    public class customKVList
    {
        public List<customKVPair> pairs;
        public customKVList() { pairs = new List<customKVPair>(); }
        public Dictionary<string,string> ToDictionary()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var item in pairs)
            {
                dict.Add(item.Key, item.Value);
            }
            return dict;
        }
    }

    #region Responses for Requests (not view-Calls)

    /// <summary>
    /// The Response for adding Add-Calls
    /// </summary>
    public class L2PAddUpdateResponse : L2PGenericRequestResponse
    {
        public int itemId;
        public string itemUrl;
        public long modifiedTimestamp;
        public long creationTimestamp;
        public string attachmentFolderPath;
    }



    /// <summary>
    /// The response for Generic Calls
    /// </summary>
    public class L2PGenericRequestResponse : L2PBaseData
    {
        public string comment;
    }

    public class L2PCreateFolderResponse : L2PGenericRequestResponse
    {
        public string sourceDirectory;
        public int itemId;
        public long modifiedTimestamp;
        public string name;
        public string selfUrl;
    }

    public class L2PSolutionDocument
    {
        public string downloadUrl;
        public int itemId;
        public string fileSize;
        public long modifiedTimestamp;
        public string fileName;
    }

    public class L2PProvideAssignmentSolutionResponse : L2PGenericRequestResponse
    {
        public long creationTimestamp;
        public long modifiedTimestamp;
        public string studentComment;
        public List<L2PSolutionDocument> solutionDocuments;
        public string solutionDirectory;
        public List<String> submittedByStudents;
        public int itemId;
    }

    public class L2PRegisterResponse : L2PBaseData
    {
        public bool Status;
    }

    

    #endregion

    #region L2P request DataTypes

    public class L2PBaseRequestData
    {
        public override string ToString()
        {
            // If you do not want to use Newtonsoft, create JSON object in a different way
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }

    public class L2PAssignmentSolutionRequest : L2PBaseRequestData
    {
        public string comment;
    }

    public class L2PAddAnnouncementRequest : L2PBaseRequestData
    {
        public string body;
        public long expiretime;
        public string title;
    }

    public class L2PAddAssignmentRequest : L2PBaseRequestData
    {
        public long  dueDate;
        public bool groupSubmissionAllowed;
        public double totalMarks; //changed from totalmark to totalPoint
        public string description;
        public string title;
    }

    public class L2PAddCourseEventRequest : L2PBaseRequestData
    {
        public string category;
        public string description;
        public bool allDay;
        public string location;
        public long endDate;
        public long eventDate;
        public string contentType;
        public string title;
    }

    public class L2PAddDiscussionThreadRequest : L2PBaseRequestData
    {
        public string subject;
        public string body;
    }

    public class L2PAddDiscussionThreadReplyRequest : L2PBaseRequestData
    {
        public string subject;
        public string body;
    }

    public class L2PAddEmailRequest : L2PBaseRequestData
    {
        public string recipients;
        public string cc;
        public string body;
        public string subject;
        public string replyTo;
        public List<L2PUploadRequest> attachmentsToUpload;
        //public bool replyTo;
    }

    public class L2PAddHyperlinkRequest : L2PBaseRequestData
    {
        public string url;
        public string notes;
        public string description;
    }

    public class L2PAddWikiRequest : L2PBaseRequestData
    {
        public string title;
        public string body;
    }

    public class L2PUploadRequest : L2PBaseRequestData
    {
        public string fileName;
        // A base64 encoded stream
        public string stream;
    }

    public class L2PLiteratureAddRequest : L2PBaseRequestData
    {
        public string title;
        public string authors;
        public string year;
        public string url;
        public string publisher;
        public string relevance;
        public string address;
        public string booktitle;
        public string comments;
        public string doi;
        public string edition;
        public string fromPage;
        public string isxn;
        public string journalName;
        public string number;
        public string role;
        public string series;
        public string toPage;
        public string type;
        public string volume;
        public string urlComment;
        public string contentType;

    }

    public class L2PRegisterRequest : L2PBaseRequestData
    {
        public string groupName;
        public string uid;
    }

    public class L2PLearningObjectAddDataType : L2PBaseRequestData
    {
        public string title;
        public string description;
        public List<int> literature;
        public List<int> materials;
        public List<int> hyperlinks;
        public int relatedMedia;
    }

    #endregion
}
