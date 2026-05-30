using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PCL;

public partial class FriendsViewModel(ModProfile.McProfile profile) : ObservableObject
{
    private ModProfile.McProfile _profile = profile;

    [ObservableProperty] private ObservableCollection<FriendsItemViewModel> _friends = [new FriendsItemViewModel(), 
        new FriendsItemViewModel(),
        new FriendsItemViewModel(),
        new FriendsItemViewModel(),
        new FriendsItemViewModel(),
        new FriendsItemViewModel(),
        new FriendsItemViewModel()];
    [ObservableProperty] private ObservableCollection<FriendsItemViewModel> _requests = [];
}
