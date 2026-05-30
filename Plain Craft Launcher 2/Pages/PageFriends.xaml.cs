using System.Windows;

namespace PCL;

public partial class PageFriends
{
    public PageFriends()
    {
        InitializeComponent();

        // 加载 McProfile
        var profile = ModMain.FrmMain?.PageCurrent.Additional?.Profile ?? throw new InvalidOperationException("无法读取玩家档案信息。");
        // MVVM
        DataContext = new FriendsViewModel(profile);
        // 傻逼 WPF
        foreach (MyRadioButton Btn in PanFriendsFilter.Children)
            Btn.LabText.Margin = new Thickness(-3, 0d, 8d, 0d);
        foreach (MyRadioButton Btn in PanRequestsFilter.Children)
            Btn.LabText.Margin = new Thickness(-3, 0d, 8d, 0d);
    }
}