var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    
});