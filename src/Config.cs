namespace github_issues_mirror
{
    using DotNetEnv;

    public static class Config
    {
        static Config()
        {
            Env.Load();
        }
        
        private static string? _token;

        public static string Token
        {
            get => _token ?? DotNetEnv.Env.GetString("GITHUB_TOKEN");
            set => _token = value;
        }
        
        public static string Username => Env.GetString("DB_USERNAME");
        public static string Password => Env.GetString("DB_PASSWORD");
        
    }
}
