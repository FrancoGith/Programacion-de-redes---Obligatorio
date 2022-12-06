namespace DTOs
{
    public class FilterDTO
    {
        public bool FilterDate { get; set; }
        public bool FilterCategory { get; set; }
        public bool FilterContent { get; set; }
        public string DateText { get; set; }
        public string CategoryText { get; set; }
        public string ContentText { get; set; }

        public FilterDTO()
        { 
            DateText = string.Empty;
            CategoryText = string.Empty;
            ContentText = string.Empty;
        }
    }
}