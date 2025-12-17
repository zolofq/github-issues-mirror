using github_issues_mirror;

var builder = WebApplication.CreateBuilder(args);

DbConfig.Username = builder.Configuration["username"] ?? throw new Exception("Username is required");
DbConfig.Password = builder.Configuration["password"] ?? throw new Exception("Passowrd is required");

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
