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
        
        public List<Comments> Comments { get; set; } = new();
    }

    public class Comments
    {
        public long id { get; set; } // GitHub comment ID
        public string author { get; set; } = null!;
        public string body { get; set; } = null!;
        public DateTime updated_at { get; set; }
        
        // Foreign key to communicate with Issues
        public long Issueid { get; set; }
        public Issues Issue { get; set; } = null!;
    }
}