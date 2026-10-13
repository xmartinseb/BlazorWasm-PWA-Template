global using CT = System.Threading.CancellationToken;
using MojePwa.Server.Data;
using MojePwa.Server.Services.Exceptions;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MojePwa API",
        Version = "v1"
    }));
builder.Services.AddSingleton<FakeDbService>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

var app = builder.Build();
app.UseExceptionHandler();
//app.UseStatusCodePages(); // Note: AI doporučuje, zatim neni potřeba
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapFallback("/api/{**path}", () => Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Not Found", detail: "No API endpoint matches the requested path"));
app.MapFallbackToFile("index.html");
app.Run();