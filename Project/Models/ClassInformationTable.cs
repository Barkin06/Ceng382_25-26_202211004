namespace Week2.Models
{
    public class ClassInformationTable
    {
        public string ClassName { get; set; }
        public int StudentCount { get; set; }
        public string Description { get; set; }

        // We'll use this HiddenId property for edit/delete behind the scenes
        public int HiddenId { get; set; }
    }
}
