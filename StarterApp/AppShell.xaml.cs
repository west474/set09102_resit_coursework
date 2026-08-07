using StarterApp.ViewModels;
using StarterApp.Views;
using StarterApp.Services;

namespace StarterApp;

public partial class AppShell : Shell
{
	private readonly IAuthenticationService _authService;
	
	public AppShell(AppShellViewModel viewModel, IAuthenticationService authService)
	{	
		InitializeComponent();
		BindingContext = viewModel;
		_authService = authService;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		if (await _authService.TryRestoreSessionAsync())
		{
			await Shell.Current.GoToAsync("///items");
		}
	}
}
