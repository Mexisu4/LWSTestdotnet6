
using WebPlatform;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Newtonsoft.Json.Serialization;
using System.Globalization;
using WebEssentials.AspNetCore.Pwa;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();



builder.Services.AddLocalization(o =>
{
    // We  put our translations in a folder called Resources
    o.ResourcesPath = "Resources";
});

builder.Services
    .AddLocalization(options => options.ResourcesPath = "Resources")
    .AddMvc()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(LabelResources));
    })
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(SharedResources));
    });
// .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver());
builder.Services.Configure<IISServerOptions>(options =>
{
    options.AutomaticAuthentication = false;
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
#if DEBUG
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // deconnexion after 1 minute (test)
#else
                    options.ExpireTimeSpan = TimeSpan.FromDays(3); // deconnexion after 30 minutes
#endif

        options.LoginPath = options.ReturnUrlParameter.ToLower().IndexOf("customer") != -1 ? "/customer/sign-in" : "/customer/sign-in";
        // if true - Javascript will not be able to read cookie.
        options.Cookie.HttpOnly = false;
        // required or else it will result in an endless-login / redirect loop if it's called from an iframe in sharepoint
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
    });
builder.Services.AddControllers();
builder.Services.AddAntiforgery(options => options.HeaderName = "MY-XSRF-TOKEN");
builder.Services.AddDistributedMemoryCache();
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    //options.CheckConsentNeeded = context => true;
    //options.MinimumSameSitePolicy = SameSiteMode.None;
    //options.Secure = CookieSecurePolicy.None;
});
builder.Services.AddRazorPages().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
}).AddRazorRuntimeCompilation();
builder.Services.AddProgressiveWebApp(new PwaOptions
{
    RegisterServiceWorker = false,
    RegisterWebmanifest = false,
    Strategy = ServiceWorkerStrategy.NetworkFirst

});
//services.AddAuthentication();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
    options.SupportedCultures = new[]{
            new CultureInfo("en-US"),
            new CultureInfo("fr-FR"),
            new CultureInfo("rw-RW")
        };
    options.SupportedUICultures = new[]{
            new CultureInfo("en-US"),
            new CultureInfo("fr-FR"),
            new CultureInfo("rw-RW")
        };

    options.RequestCultureProviders.Insert(0, new CustomRequestCultureProvider(context =>
    {
        // My custom request culture logic
        return Task.FromResult(new ProviderCultureResult("en-US"));
    }));
});
builder.Services.AddMvc();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
// IIS
builder.Services.Configure<IISServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});
builder.Services.AddSession();
builder.Services.AddOptions();
builder.Services.AddSignalR();
// https://www.c-sharpcorner.com/article/creating-pdf-in-asp-net-core-mvc-using-rotativa-aspnetcore/
var app = builder.Build();

//builder.Services.AddWkhtmltopdf("wkhtmltopdf");
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();

//app.UseAuthorization();

//app.MapRazorPages();
app.UseRouting();

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US", "en-US"),
    SupportedCultures = new[]{
            new CultureInfo("en-US"),
            new CultureInfo("fr-FR"),
            new CultureInfo("rw-RW")
        },
    SupportedUICultures = new[]{
            new CultureInfo("en-US"),
            new CultureInfo("fr-FR"),
            new CultureInfo("rw-RW")
        },
    // you can change the list of providers, if you don't want the default behavior
    // e.g. the following line enables to pick up culture ONLY from cookies
    RequestCultureProviders = new[] { new CookieRequestCultureProvider() }
};
app.UseRequestLocalization(localizationOptions);
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// app.UseResponseCompression();
app.UseStaticFiles();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapRazorPages();
});
// call rotativa conf passing env to get web root path
//RotativaConfiguration.Setup((Microsoft.AspNetCore.Hosting.IHostingEnvironment)env);
app.Run();
