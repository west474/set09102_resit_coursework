using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using StarterApp.Database.Models;
using StarterApp.Repositories;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class ItemViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IItemRepository _itemRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

    private int? _itemId;
    private int? _itemOwnerId;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private int? selectedCategoryId;

    [ObservableProperty]
    private Category? selectedCategory;

    [ObservableProperty]
    private double latitude;

    [ObservableProperty]
    private double longitude;

    [ObservableProperty]
    private decimal dailyRate;

    [ObservableProperty]
    private bool isAvailable = true;

    [ObservableProperty]
    private List<Category> categories = new();

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private bool isOwner;

    [ObservableProperty]
    private bool canRequestRental;

    partial void OnSelectedCategoryChanged(Category? value)
    {
        SelectedCategoryId = value?.Id;
    }

    [ObservableProperty]
    private DateTime rentalStartDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private DateTime rentalEndDate = DateTime.Today.AddDays(2);

    public ItemViewModel(
        IItemRepository itemRepository,
        IRepository<Category> categoryRepository,
        IRentalRepository rentalRepository,
        IAuthenticationService authService,
        INavigationService navigationService)
    {
        _itemRepository = itemRepository;
        _categoryRepository = categoryRepository;
        _rentalRepository = rentalRepository;
        _authService = authService;
        _navigationService = navigationService;
        Title = "New Item";
    }

    // applyQueryAttributes ref: https://tinyurl.com/3fbpvxy8
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idValue) && int.TryParse(idValue?.ToString(), out var id))
        {
            _ = InitialiseAsync(id);
        }
        else
        {
            _ = InitialiseAsync();
        }
    }

    public async Task InitialiseAsync(int? itemId = null)
    {
        try
        {
            IsBusy = true;

            Categories = await _categoryRepository.GetAllAsync();

            if (itemId.HasValue)
            {
                _itemId = itemId.Value;
                IsEditMode = true;
                Title = "Edit Item";

                var item = await _itemRepository.GetByIdAsync(itemId.Value);
                if (item != null)
                {
                    Title = item.Title;
                    Description = item.Description ?? string.Empty;
                    SelectedCategoryId = item.CategoryId;
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == item.CategoryId);
                    Latitude = item.Latitude;
                    Longitude = item.Longitude;
                    DailyRate = item.DailyRate;
                    IsAvailable = item.IsAvailable;

                    _itemOwnerId = item.OwnerId;
                    IsOwner = _authService.CurrentUser != null && item.OwnerId == _authService.CurrentUser.Id;
                    CanRequestRental = !IsOwner;
                }
            }
            else
            {
                IsEditMode = false;
                Title = "New Item";
                IsOwner = true;
                CanRequestRental = false;
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            SetError("Title is required");
            return;
        }

        if (SelectedCategoryId == null || SelectedCategoryId == 0)
        {
            SetError("Please select a category");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            if (IsEditMode && _itemId.HasValue)
            {
                var item = new Item
                {
                    Id = _itemId.Value,
                    Title = Title,
                    Description = Description,
                    CategoryId = SelectedCategoryId,
                    Latitude = Latitude,
                    Longitude = Longitude,
                    DailyRate = DailyRate,
                    IsAvailable = IsAvailable
                };

                await _itemRepository.UpdateAsync(item);
            }
            else
            {
                if (_authService.CurrentUser == null)
                {
                    SetError("Must be logged in to list an item.");
                    return;
                }

                var item = new Item
                {
                    Title = Title,
                    Description = Description,
                    CategoryId = SelectedCategoryId,
                    Latitude = Latitude,
                    Longitude = Longitude,
                    DailyRate = DailyRate,
                    IsAvailable = IsAvailable,
                    OwnerId = _authService.CurrentUser.Id
                };

                await _itemRepository.AddAsync(item);
            }

            await _navigationService.NavigateToAsync("///items");
        }
        catch (Exception ex)
        {
            SetError($"Failed to save: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RequestRentalAsync()
    {
        if (_authService.CurrentUser == null)
        {
            SetError("Must be logged in to request a rental.");
            return;
        }

        if (RentalEndDate <= RentalStartDate)
        {
            SetError("End date must be after start date.");
            return;
        }

        if (!_itemId.HasValue)
        {
            SetError("Item not loaded.");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            var rental = new Rental
            {
                ItemId = _itemId.Value,
                ItemTitle = Title,
                OwnerId = _itemOwnerId ?? 0,
                BorrowerId = _authService.CurrentUser.Id,
                BorrowerName = $"{_authService.CurrentUser.FirstName} {_authService.CurrentUser.LastName}",
                StartDate = RentalStartDate,
                EndDate = RentalEndDate
            };

            await _rentalRepository.AddAsync(rental);

            await Shell.Current.DisplayAlertAsync("Success", "Rental request submitted.", "OK");
            await _navigationService.NavigateToAsync("///items");
        }
        catch (Exception ex)
        {
            SetError($"Failed to request rental: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigationService.NavigateToAsync("///items");
    }
}