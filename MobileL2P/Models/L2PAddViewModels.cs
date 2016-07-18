using System.ComponentModel.DataAnnotations;

namespace MobileL2P.Models
{
    public class HyperLinkViewModel
    {
        [Required]
        [DataType(DataType.Url)]
        [Display(Name = "Hyperlink URL")]
        public string URL { get; set; }

        [StringLength(50)]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [StringLength(200)]
        [Display(Name = "Notes")]
        public string Notes { get; set; }

        public int itemId { get; set; }
    }

    public class FolderViewModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Name")]
        public string Name { get; set; }

    }

    public class GroupWorkSpaceViewModel
    {
        [Required]
        [StringLength(1000)]
       public string ListOfUsers { get; set; }

    }


    public class LoginViewModel
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class AnnouncementViewModel
    {
        [Required]
        [StringLength(100)]
        public string title { get; set; }

        [Required]
        [StringLength(100)]
        public string body { get; set; }

        public string folderName { get; set; }
        public int itemId { get; set; }

    }

    public class EmailViewModel
    {
        [Required]
        [StringLength(100)]
        public string subject { get; set; }

        [Required]
        [StringLength(100)]
        public string body { get; set; }

        [Required]
        public string recipients { get; set; }

        [Required]
        public string cc { get; set; }

        public string sender { get; set; }
        public int itemId { get; set; }
    }
    public class AssignmentViewModel
    {

        [Required]
        [StringLength(100)]
        [Display(Name = "Assignment Title")]
        public string Title { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Allow Group Submission")]
        public bool groupSubmissionAllowed { get; set; }

        [Required]
        [Display(Name = "Total Point")]
        public double totalPoint { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Due Date")]
        public string DueDate { get; set; }
        [Required]
        [StringLength(50)]
        [DisplayFormat(DataFormatString = "{0:hh:mm}", ApplyFormatInEditMode = true)]
        //[Display(Name = "Due Date Hours")]
        public string DueDatehours { get; set; }

     
        [DataType(DataType.Url)]
        [Display(Name = "Assignment Documents")]
        public string AssignmentDocuments { get; set; }


        [DataType(DataType.Url)]
        [Display(Name = "Sample Solution")]
        public string SampleSolution { get; set; }

        public int itemId{ get;set;}

    }

    public class SolutionViewModel
    {

        public bool Status { set; get; }
        public int assignID { set; get; }

        [Display(Name = "Solution Title")]
        public string solName { set; get; }
        [Display(Name = "Assignment Name")]
        public string assignmentName { set; get; }


        public bool groupsubmission { set; get; }

        [Display(Name = "Student Comment")]
        public string StudentComment { set; get; }

        public string url { set; get; }
    }


    public class LiteratureViewModel
    {

        [StringLength(50)]
        [Display(Name = "Availability")]
        public string availability { get; set; }

        public int itemId { get; set; }

        [StringLength(50)]
        [Display(Name = "Editor")]
        public string editor { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Title")]
        public string title { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Publication Type")]
        public string type { get; set; }


        [Required]
        [StringLength(50)]
        [Display(Name = "Author")]
        public string authors { get; set; }


        [StringLength(50)]
        [Display(Name = "Publisher")]
        public string publisher { get; set; }

        [StringLength(50)]
        [Display(Name = "Address")]
        public string address { get; set; }

        [StringLength(50)]
        [Display(Name = "ISXN")]
        public string isxn { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Year")]
        public string year { get; set; }

        [StringLength(50)]
        [DataType(DataType.Url)]
        [Display(Name = "url")]
        public string url { get; set; }

        [StringLength(50)]
        [Display(Name = "Relevance")]
        public string relevance { get; set; }

        [StringLength(50)]
        [Display(Name = "Book Title")]
        public string booktitle { get; set; }

        [StringLength(100)]
        [Display(Name = "Comments")]
        public string comments { get; set; }

        [StringLength(50)]
        [Display(Name = "Doi")]
        public string doi { get; set; }

        [StringLength(50)]
        [Display(Name = "Edition")]
        public string edition { get; set; }

        [StringLength(50)]
        [Display(Name = "From Page")]
        public string fromPage { get; set; }




        [StringLength(50)]
        [Display(Name = "Journal Name")]
        public string journalName { get; set; }

        [StringLength(50)]
        [Display(Name = "Number")]
        public string number { get; set; }

        [StringLength(50)]
        [Display(Name = "Role")]
        public string role { get; set; }

        [StringLength(50)]
        [Display(Name = "Series")]
        public string series { get; set; }

        [StringLength(50)]
        [Display(Name = "To Page")]
        public string toPage { get; set; }

        [StringLength(50)]
        [Display(Name = "Volume")]
        public string volume { get; set; }

        [StringLength(50)]
        [Display(Name = "Url Description")]
        public string urlComment { get; set; }

    }

    public class DiscussionViewModel
    {
        [Required]
        [StringLength(100)]
        public string title { get; set; }

        [Required]
        [StringLength(200)]
        public string body { get; set; }

        public string from { get; set; }
        public bool isByMe { get; set; }

        [Required]
        public int dId { get; set; }

        public int pId { get; set; }

        public bool byMe { get; set; }
    }
}