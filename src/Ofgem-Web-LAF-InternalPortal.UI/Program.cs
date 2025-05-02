using Azure.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Ofgem_Web_LAF_InternalPortal;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem_Web_LAF_InternalPortal.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

// Add services to the container
builder.Services.AddHttpClient();

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddLogsConfiguration(builder.Configuration);
}

//Azure Key Vault Configuration
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
    new DefaultAzureCredential());

// Authentication
//Internal Portal Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".laf.int.session";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AtLeastOneRole",
        policy => policy.RequireRole(
            Ofgem.LAF.SharedLibrary.Models.UserRoles.Basic,
            Ofgem.LAF.SharedLibrary.Models.UserRoles.Standard,
            Ofgem.LAF.SharedLibrary.Models.UserRoles.Advanced,
            Ofgem.LAF.SharedLibrary.Models.UserRoles.Expert,
            Ofgem.LAF.SharedLibrary.Models.UserRoles.Admin));
});


//Adding Microsoft Web App Auth
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration, "AAD:InternalReg")
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();

builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.AccessDeniedPath = new PathString("/Errors/403");
});

builder.Services.AddHttpContextAccessor();

//Adding and configuring Razor Pages
builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.AuthorizePage(LafPages.Dashboard.ROUTE, "AtLeastOneRole");
        options.Conventions.AuthorizePage(LafPages.DeclarationDetail.ROUTE, "AtLeastOneRole");
        options.Conventions.AuthorizePage(LafPages.DuplicateEntries.ROUTE, "AtLeastOneRole");
        options.Conventions.AuthorizePage(LafPages.LANDING, "AtLeastOneRole");
        options.Conventions.AuthorizePage(LafPages.UploadTemplate.ROUTE, "AtLeastOneRole");
    })
    .AddViewOptions(options =>
    {
        options.HtmlHelperOptions.ClientValidationEnabled = false;
    })
    .AddMicrosoftIdentityUI()
    .AddSessionStateTempDataProvider();

builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IBasicValidationService, BasicValidationService>();
builder.Services.AddScoped<IRedactionService, RedactionService>();
builder.Services.AddScoped<ISoiService, SoiService>();
builder.Services.AddScoped<IUserService, UserService>();

string localAuthorityServiceUrl = builder.Configuration["LocalAuthorityServiceApiUrl"] ?? throw new InvalidOperationException();
builder.Services.AddHttpClient<ILaManagementService, LaManagementService>(
    x => x.BaseAddress = new Uri(localAuthorityServiceUrl!));

string declarationManagementServiceUrl = builder.Configuration["DeclarationServiceApiUrl"] ?? throw new InvalidOperationException();
builder.Services.AddHttpClient<IDeclarationManagementService, DeclarationManagementService>(
    x => x.BaseAddress = new Uri(declarationManagementServiceUrl!));

builder.Services.AddTransient<HeaderHandler>();

// Add named services to the container.
DocumentApi.ConfigureHttpClient(builder);
DeclarationApi.ConfigureHttpClient(builder);
UserApi.ConfigureHttpClient(builder);
LocalAuthorityApi.ConfigureHttpClient(builder);
FeatureService.ConfigureFeatureService(builder);


builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.Use(next => context =>
{
    Console.WriteLine($"Found: {context.GetEndpoint()?.DisplayName}");
    return next(context);
});

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();
app.MapHealthChecks("/health");
app.MapRazorPages();

app.UseStatusCodePagesWithReExecute("/Errors/{0}");

app.Run();
