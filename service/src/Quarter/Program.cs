using Microsoft.AspNetCore.Builder;
using Quarter;

var app = WebApplication.CreateBuilder(args)
    .AddQuarter()
    .Build()
    .UseQuarter();

app.RunMigrations();
await app.RunStartupTasksAsync();
await app.RunAsync();
