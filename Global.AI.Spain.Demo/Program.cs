using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;
using Global.AI.Spain.Demo;
using Global.AI.Spain.Demo.Options;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using Azure;

var assemblyName = typeof(Program).Assembly.GetName().Name;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
});

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());

/* Load Configuration */

if (Debugger.IsAttached)
{
    builder.Configuration.AddJsonFile(@"appsettings.debug.json", optional: true, reloadOnChange: true);
}

builder.Configuration.AddJsonFile($@"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                     .AddJsonFile($@"appsettings.{Environment.UserName}.json", optional: true, reloadOnChange: true)
                     .AddEnvironmentVariables();

/* Logging Configuration */

var applicationInsightsConnectionString = builder.Configuration.GetConnectionString(Constants.ConnectionStrings.ApplicationInsights);

builder.Logging.AddApplicationInsights((telemetryConfiguration) => telemetryConfiguration.ConnectionString = applicationInsightsConnectionString, (_) => { })
               .AddConsole()
               ;

builder.Services.AddProblemDetails()
                .AddApplicationInsightsTelemetry(builder.Configuration)
                .AddRouting();

builder.Services.AddControllers(options =>
                {
                    options.SuppressAsyncSuffixInActionNames = true;
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                })
                ;

/* Load Options */

builder.Services.AddOptionsWithValidateOnStart<AzureOpenAIOptions>().Bind(builder.Configuration.GetSection(nameof(AzureOpenAIOptions))).ValidateDataAnnotations();

/* Load Services */

// OpenAIClient
builder.Services.AddSingleton(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<AzureOpenAIOptions>>().Value;

    var clientOptions = new OpenAIClientOptions(options.ServiceVersion);

    return new OpenAIClient(options.Endpoint, new AzureKeyCredential(options.Key), clientOptions);
});

/* Application Middleware Configuration */

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting()
   .UseExceptionHandler()
   .UseAuthentication()
   .UseAuthorization()
   .UseStatusCodePages()
   .UseEndpoints(endpoints =>
   {
       endpoints.MapControllers();
   })
   ;

app.Run();
