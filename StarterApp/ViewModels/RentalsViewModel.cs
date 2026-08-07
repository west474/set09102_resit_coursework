using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Repositories;
using System.Collections.ObjectModel;

namespace StarterApp.ViewModels;

public partial class RentalsViewModel : BaseViewModel
{
    private readonly IRentalRepository _rentalRepository;

    [ObservableProperty]
    private ObservableCollection<Rental> rentals = new();

    [ObservableProperty]
    private bool isShowingIncoming = true;

    [ObservableProperty]
    private bool isRefreshing;

    public RentalsViewModel(IRentalRepository rentalRepository)
    {
        _rentalRepository = rentalRepository;
        Title = "My Rentals";
    }

    public async Task InitialiseAsync() => await LoadRentalsAsync();

    [RelayCommand]
    private async Task LoadRentalsAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            var rentalsList = IsShowingIncoming
                ? await _rentalRepository.GetIncomingAsync()
                : await _rentalRepository.GetOutgoingAsync();

            Rentals.Clear();
            foreach (var rental in rentalsList)
            {
                Rentals.Add(rental);
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load rentals: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task ShowIncomingAsync()
    {
        IsShowingIncoming = true;
        await LoadRentalsAsync();
    }

    [RelayCommand]
    private async Task ShowOutgoingAsync()
    {
        IsShowingIncoming = false;
        await LoadRentalsAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadRentalsAsync();
    }
}