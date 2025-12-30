namespace github_issues_mirror
{
    using DotNetEnv;

    public static class Config
    {
        static Config()
        {
            Env.Load();
        }
        
        public static string Username => Env.GetString("DB_USERNAME");
        public static string Password => Env.GetString("DB_PASSWORD");
        
        public static string Token { get; set; } = Env.GetString("GITHUB_TOKEN");
        
    }
}
