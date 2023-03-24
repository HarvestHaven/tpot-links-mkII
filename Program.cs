using TPOT_Links;
using Neo4j.Driver;
using CodeMechanic.Extensions;
using CodeMechanic.RazorPages;

var builder = WebApplication.CreateBuilder(args);

// Load and inject .env files & values
DotEnv.Load();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddTransient<IEmbeddedResourceQuery, EmbeddedResourceQuery>();
// builder.UseElectron(args);


static void ConfigureServices(IServiceCollection services)
{
    string uri =  Environment.GetEnvironmentVariable("NEO4J_URI") ?? string.Empty;
    string user = Environment.GetEnvironmentVariable("NEO4J_USER") ?? string.Empty;
    string password = Environment.GetEnvironmentVariable("NEO4J_PASSWORD") ?? string.Empty;
    
    string airtable_bearer_token = Environment.GetEnvironmentVariable("AIRTABLE_BEARER_TOKEN") ?? string.Empty;

    bool devmode = Environment.GetEnvironmentVariable("DEVMODE").ToBoolean();   

    services.AddControllers();
    services.ConfigureAirtable();

    services.AddSingleton(GraphDatabase.Driver(
        uri
        , AuthTokens.Basic(
            user,
            password
        )
    ));

    // services.AddTransient<IAirtableRepo, AirtableRepo>();
}

ConfigureServices(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();