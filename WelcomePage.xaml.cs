using CommunityToolkit.Maui.Views;

namespace Wordle;

public partial class WelcomePage : ContentPage
{
    public WelcomePage()
    {
        InitializeComponent();
    }//constructor

    private async void PlayButton_Clicked(object sender, EventArgs e)
    {
        //redirect to main game page upon clicking
        await Shell.Current.GoToAsync("/MainPage");
    }//PlayButton_Clicked

    private async void SettingsButton_Clicked(object sender, EventArgs e)
    {
        //redirect to settings page upon clicking
        await this.ShowPopupAsync(new SettingsPopUp());
    }//SettingsButton_Clicked

    private async void HowToPlay_Clicked(object sender, EventArgs e)
    {
        //redirect to HowToPlay page upon clicking
        await this.ShowPopupAsync(new HowToPlay());
    }//StatsButton_Clicked
}//WelcomePage Class