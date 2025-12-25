namespace github_issues_mirror
{
    using DotNetEnv;

    public static class DbConfig
    {
        static DbConfig()
        {
            Env.Load();
        }
        
        public static string Username => Env.GetString("DB_USERNAME");
        public static string Password => Env.GetString("DB_PASSWORD");
    }
}
