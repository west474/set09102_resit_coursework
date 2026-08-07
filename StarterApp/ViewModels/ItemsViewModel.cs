using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Repositories;
using StarterApp.Services;
using System.Collections.ObjectModel;

namespace StarterApp.ViewModels;

// viewmodel to display list of items.
public partial class ItemsViewModel : BaseViewModel
{
    private readonly IItemRepository _itemRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<Item> items = new();

    [ObservableProperty]
    private List<Category> categories = new();

    [ObservableProperty]
    private int? selectedCategoryId;

    [ObservableProperty]
    private Category? selectedCategory;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isRefreshing;

    public ItemsViewModel(
        IItemRepository itemRepository,
        IRepository<Category> categoryRepository,
        INavigationService navigationService)
    {
        _itemRepository = itemRepository;
        _categoryRepository = categoryRepository;
        _navigationService = navigationService;
        Title = "Browse Items";
    }

    // load categories/items
    public async Task InitialiseAsync()
    {
        await LoadCategoriesAsync();
        await LoadItemsAsync();
    }

    // load all categories.
    private async Task LoadCategoriesAsync()
    {
        try
        {
            var allCategories = await _categoryRepository.GetAllAsync();

            var combined = new List<Category>
            {
                new Category { Id = 0, Name = "All Categories" }
            };
            combined.AddRange(allCategories);

            Categories = combined;
        }
        catch (Exception ex)
        {
            SetError($"Failed to load categories: {ex.Message}");
        }
    }

    // load items.
    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            var itemsList = (SelectedCategoryId.HasValue && SelectedCategoryId.Value > 0)
                ? await _itemRepository.GetByCategoryAsync(SelectedCategoryId.Value)
                : await _itemRepository.GetAllAsync();

            Items.Clear();
            foreach (var item in itemsList)
            {
                Items.Add(item);
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load items: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    // search using the text entered in the search bar.
    [RelayCommand]
    private async Task SearchAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            var itemsList = string.IsNullOrWhiteSpace(SearchText)
                ? await _itemRepository.GetAllAsync()
                : await _itemRepository.SearchAsync(SearchText);

            Items.Clear();
            foreach (var item in itemsList)
            {
                Items.Add(item);
            }
        }
        catch (Exception ex)
        {
            SetError($"Search failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // navigate to create new item
    [RelayCommand]
    private async Task AddItemAsync()
    {
        await _navigationService.NavigateToAsync("///item");
    }

    // navigate to edit existing item
    [RelayCommand]
    private async Task EditItemAsync(Item item)
    {
        if (item == null) return;
        await _navigationService.NavigateToAsync($"///item?id={item.Id}");
    }

    // refresh item list
    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadItemsAsync();
    }

    // category change refresh — still fires because SelectedCategoryId still
    // changes, just now via OnSelectedCategoryChanged below instead of
    // directly from the Picker.
    partial void OnSelectedCategoryIdChanged(int? value)
    {
        _ = LoadItemsAsync();
    }

    // fires whenever the Picker's selection changes; keeps SelectedCategoryId
    // in sync so everything above continues to work unmodified.
    partial void OnSelectedCategoryChanged(Category? value)
    {
        SelectedCategoryId = value?.Id;
    }
}