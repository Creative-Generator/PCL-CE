using CommunityToolkit.Mvvm.ComponentModel;

namespace PCL;

public partial class FriendsItemViewModel : ObservableObject
{
    
    
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _logo;
}