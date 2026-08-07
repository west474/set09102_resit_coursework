using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class ItemDetailPage : ContentPage
{
    public ItemDetailPage(ItemViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}