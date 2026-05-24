using CommunityToolkit.Mvvm.ComponentModel;

namespace PCL;

public class FriendsViewModel : ObservableObject
{
    private ModProfile.McProfile _profile;

    public FriendsViewModel(ModProfile.McProfile profile)
    {
        _profile = profile;
    }
}
