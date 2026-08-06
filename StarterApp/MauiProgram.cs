using Microsoft.Extensions.Logging;
using StarterApp.ViewModels;
using StarterApp.Database.Data;
using StarterApp.Views;
using System.Diagnostics;
using StarterApp.Services;

namespace StarterApp;

// Starting point for the app.
// Manages dependency injections. Registers services, ViewModels, and Pages.
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // builder object to store requirements for the builder service to run later.
        var builder = MauiApp.CreateBuilder();
        builder
            // tells Maui application that App.xaml.cs is root of app.
            .UseMauiApp<App>()
            // setting up fonts.
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Change to true to use the API service.
        // true = API service | false = local database.
        const bool useSharedApi = true;

        if (useSharedApi)
        {
            // builds a HttpClient with API URL base address.
            // Allows other services to reference a relative path instead of full URL.
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://set09102-api.b-davison.workers.dev/")
            };
            // specifies to only allow one instance to be created.
            // this lets the bearer token stay attached since.
            // each class that asks for HttpClient gets this shared instance.
            builder.Services.AddSingleton(httpClient);

            // creates singleton. When any class constructor asks for IAuthenticationService
            // it results in this being called.
            builder.Services.AddSingleton<IAuthenticationService, ApiAuthenticationService>();
        }
        else
        {
            // creates a local alternative that talks to the postgres db. Uses LocalAuthenticationService.
            builder.Services.AddDbContext<AppDbContext>();
            builder.Services.AddSingleton<IAuthenticationService, LocalAuthenticationService>();
        }
        
        // lets ViewModels navigate betwen pages without needing Maui shell class.
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        // creates a single AppShellViewModel for the session.
        builder.Services.AddSingleton<AppShellViewModel>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<App>();

        // creates singletons of the viewmodel logic for the session.
        // uses transient to recreate page contents when navigated to.
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddSingleton<RegisterViewModel>();
        builder.Services.AddTransient<RegisterPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // build everything configured in the builder request.
        return builder.Build();
    }
}