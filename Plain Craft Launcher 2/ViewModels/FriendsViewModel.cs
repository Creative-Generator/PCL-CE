using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PCL.Core.Minecraft.Profile.Models;

namespace PCL.ViewModels;

public partial class FriendsViewModel(McProfile profile) : ObservableObject
{
    private McProfile _profile = profile;

    [ObservableProperty] private ObservableCollection<FriendsItemViewModel> _friends = [new FriendsItemViewModel(), 
        new FriendsItemViewModel(),
        new FriendsItemViewModel(),
        new FriendsItemViewModel(),
        new FriendsItemViewModel(),
        new FriendsItemViewModel(),
        new FriendsItemViewModel()];
    [ObservableProperty] private ObservableCollection<FriendsItemViewModel> _requests = [];
}
