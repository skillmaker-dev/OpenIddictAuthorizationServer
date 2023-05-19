using AuthorizationServer.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
// Add Authentication service

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.LoginPath = "/account/login";
        });

// Register dbContext

builder.Services.AddDbContext<DbContext>(options =>
{
    // Configure the context to use an in-memory store.
    options.UseInMemoryDatabase(nameof(DbContext));

    // Register the entity sets needed by OpenIddict.
    options.UseOpenIddict();
});


// Register OpenIddict

builder.Services.AddOpenIddict()

    // Register the OpenIddict core components.
    .AddCore(options =>
    {
        // Configure OpenIddict to use the EF Core stores/models.
        options.UseEntityFrameworkCore()
            .UseDbContext<DbContext>();
    })

    // Register the OpenIddict server components.
    .AddServer(options =>
    {
        options
            .AllowAuthorizationCodeFlow()
            .RequireProofKeyForCodeExchange()
            .AllowClientCredentialsFlow()
            .SetUserinfoEndpointUris("/connect/userinfo")
            .AllowRefreshTokenFlow();

        options
        .SetAuthorizationEndpointUris("connect/authorize")
            .SetTokenEndpointUris("/connect/token");

        // Encryption and signing of tokens
        options
            .AddEphemeralEncryptionKey()
            .AddEphemeralSigningKey()
            .DisableAccessTokenEncryption();

        // Register scopes (permissions)
        options.RegisterScopes("api");

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options
            .UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .EnableAuthorizationEndpointPassthrough()
            .EnableUserinfoEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        // Import the configuration from the local OpenIddict server instance.
        options.UseLocalServer();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    }); ;

builder.Services.AddHostedService<TestData>();


var app = builder.Build();



if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
});

app.Run();
