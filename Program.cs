using System.Reflection;
using Microsoft.OpenApi.Models;
using REST_API_NET8.Data;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
       c.SwaggerDoc("Sap", new OpenApiInfo { Title = "SAP API", Version = "v1" });
       c.DocInclusionPredicate((groupName, apiDesc) => apiDesc.GroupName == groupName);

       var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
       var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
       c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
       options.AddPolicy("CorsPolicy", policy =>
       {
              policy.WithOrigins(builder.Configuration["Origins"]!.Split(";"))
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
       });
});
builder.Services.AddSignalR();
builder.Services.AddDbContext<FaceScanDbContext>();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI(c =>
{
       c.SwaggerEndpoint("/swagger/Sap/swagger.json", "SAP API");
       c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
});

app.UseCors("CorsPolicy");
app.MapControllers();
app.Run();