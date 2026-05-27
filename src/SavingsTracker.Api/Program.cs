using SavingsTracker.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddPostgreDb();
builder.AddJwtAuthentication();
builder.Services.AddValidation();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();


app.MigrateDatabase();
app.Run();


