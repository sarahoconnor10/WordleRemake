using CommunityToolkit.Maui.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Wordle;

public partial class StatsPopUp : Popup, INotifyPropertyChanged
{
    //variables
    private int _percentWon;
    private int _numWins;
    private int _streak;
    private int _gamesPlayed;
    public string SaveFilePath => System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, "savefile.txt");

    public StatsPopUp()
	{
        //assigns binding context, updates stats.
		InitializeComponent();
        BindingContext = this;
        UpdateStatistics();
    }//constructor
    public void UpdateStatistics()
    {
        //gets details from file
        GetDetails();
    }//UpdateStatistics()
    public int PercentWon
    {
        get => _percentWon;

        set
        {
            if(_percentWon != value)
            {
                _percentWon = value;
                OnPropertyChanged(nameof(PercentWon));
            }
        }
    }//PercentWon
    public int NumWins
    {
        get => _numWins;
        set
        {
            if (_numWins != value)
            {
                _numWins = value;
                OnPropertyChanged(nameof(NumWins));
            }
        }
    }//NumWins
    public int Streak
    {
        get => _streak;
        set
        {
            if (_streak != value)
            {
                _streak = value;
                OnPropertyChanged(nameof(Streak));
            }
        }
    }//Streak
    public int GamesPlayed
    {
        get => _gamesPlayed;
        set
        {
            if (_gamesPlayed != value)
            {
                _gamesPlayed = value;
                OnPropertyChanged(nameof(GamesPlayed));
            }
        }
    }//GamesPlayed
    public async Task GetDetails()
    {
        if (File.Exists(SaveFilePath))
        {
            try
            {
                //read in variables from save file
                using (StreamReader sr = new StreamReader(SaveFilePath))
                {
                    //number of wins
                    NumWins = int.Parse(sr.ReadLine());
                    //streak
                    Streak = int.Parse(sr.ReadLine());
                    //games played
                    GamesPlayed = int.Parse(sr.ReadLine());

                    //win percentage
                    if (GamesPlayed != 0)
                        PercentWon = (int)(((double)NumWins / GamesPlayed) * 100);
                    else
                        PercentWon = 0;
                }//StreamReader
            }//try
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error reading details from file", ex.Message, "OK");
            }//catch
        }//if file exists - read from it
        else
        {
            NumWins = 0;
            PercentWon = 0;
            Streak = 0;
            GamesPlayed = 0;
        }//else no file
    }//GetDetails()

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }//OnPropertyChanged()
}//class