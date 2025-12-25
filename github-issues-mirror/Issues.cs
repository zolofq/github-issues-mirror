namespace github_issues_mirror
{
    public class Issues
    {
        public long id { get; set; }      
        public int number { get; set; }          
        public string title { get; set; } = null!;
        public string state { get; set; } = null!; 
        public DateTime updated_at { get; set; } 
        public string author { get; set; } = null!; 
        public string? body { get; set; }    
    }
}