﻿
using CommunityToolkit.Maui.Views;
using System.Diagnostics;
using Wordle.ViewModels;
using System;
using System.IO;

namespace Wordle;

/*
 * 04/01/24 current tasks:
 *  add in references
 *  dynamic sizing depending on device
 */

public partial class MainPage : ContentPage
{
    //variables - pages / viewmodels
    //private AppSettings _settingsViewModel;
    private WordsViewModel _wordsViewModel = new WordsViewModel();

    //lists
    public List<Button> keys;
    private List<Label> addedLabels = new List<Label>();
    private List<Frame> addedFrames = new List<Frame>();
    private List<string> guesses = new List<string>();
    private List<string> words = new List<string>();
    private List<char> correctLettersGuessed = new List<char>();
    private List<char> yellowLettersGuessed = new List<char>();
    private List<char> wrongLettersGuessed = new List<char>();
    
    private Random random = new Random();
    private HttpClient httpClient = new HttpClient();

    public string SaveFilePath => System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, "savefile.txt");
    private string guess;
    private int letterCounter = 0, guessCounter = 0;
    private int numWins, gamesPlayed, percentWon, streak;
    public bool gameRunning = false;
    bool isHardMode;
    bool validInput;
    bool gridDrawn = false;
    bool won = false;
    private string chosenWord { get; set; }

    public MainPage()
    {
        InitializeComponent();
        BindingContext = _wordsViewModel;
       
        _wordsViewModel.GetWordsFromVM();

        //populate list of keys
        keys = new List<Button>
        {
            a_key, b_key, c_key, d_key, e_key, f_key, g_key, h_key, i_key, j_key,
            k_key, l_key, m_key, n_key, o_key, p_key, q_key, r_key, s_key, t_key,
            u_key, v_key, w_key, x_key, y_key, z_key, back_btn
        };

        if ((bool)Application.Current.Resources["IsDarkMode"])
        {
            stats_image.Source = "stats_dark.png";
            how_image.Source = "how_dark.png";
        }
        else
        {
            stats_image.Source = "stats.png";
            how_image.Source = "how.png";
        }

        isHardMode = (bool)Application.Current.Resources["IsHardMode"];
        PlayGame();

    }//MainPage constructor

    public void PlayGame()
    {
        GetDetails();
        RestartGame();
        GetWord();
    }//PlayGame()

    private void playAgain_btn_Clicked(object sender, EventArgs e)
    {
        PlayGame();
    }//playAgain_btn_Clicked()

    private void DrawGrid()
    {
        if (!gridDrawn)
        {
            for (int i = 0; i < 6; i++)
                GuessGrid.AddRowDefinition(new RowDefinition());

            for (int i = 0; i < 5; i++)
                GuessGrid.AddColumnDefinition(new ColumnDefinition());
        }//if grid hasnt been drawn - first iteration

        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 6; col++)
            {
                Frame styledFrame = new Frame
                {
                    BackgroundColor = (Color)Application.Current.Resources["BackgroundColor"],
                    CornerRadius = 0,
                    HasShadow = false,
                    Padding = new Thickness(5),
                    BorderColor = (Color)Application.Current.Resources["TextColor"]
                };
                GuessGrid.Add(styledFrame, row, col);

                addedFrames.Add(styledFrame);
            }//inner loop - col
        }//outer loop - row
        gridDrawn = true;
    }//DrawGrid()

    private async void GetWord()
    {
        words = _wordsViewModel.Words;
        chosenWord = PickWord();
        Debug.WriteLine(chosenWord);
    }//GetWord()

    public string PickWord()
    {
        if (words.Count > 0)
        {
            int randomNumber = random.Next(0, words.Count);
            return words[randomNumber];
        }//if list is populated / not empty
        else
        {
            return null;
        }//else list is empty
    }//PickWord()

    private async void Button_Clicked(object sender, EventArgs e)
    {
        if (guessCounter < 6 && letterCounter < 5)
        {
            if (sender is Button button)
            {
                string text = button.Text;

                var label = new Label
                {
                    Text = text,
                    FontSize = 30,
                    TextColor = (Color)Application.Current.Resources["TextColor"],
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center

                };

                GuessGrid.Add(label, letterCounter, guessCounter);
                addedLabels.Add(label);
                letterCounter++;

                //animation - increase size of letter briefly on input
                await label.ScaleTo(1.2, 200);
                await Task.Delay(100);
                await label.ScaleTo(1, 200);

                if (letterCounter == 5)
                    enter_btn.IsEnabled = true;
                else
                    enter_btn.IsEnabled = false;
            }//if button
        }//if num guesses is less than 6
    }//Button_Clicked()

    private void Backspace_Clicked(object sender, EventArgs e)
    {
        if(letterCounter > 0)
        {
           var lastLabel = addedLabels[addedLabels.Count - 1];
            GuessGrid.Children.Remove(lastLabel);
            addedLabels.Remove(lastLabel);
            letterCounter--;
            if (letterCounter < 5)
            {
                enter_btn.IsEnabled = false;
                //enter_btn.BackgroundColor = (Color)Application.Current.Resources["EnabledButtonColor"];
            }
        }//if greater than 0
        if(letterCounter < 0)
        {
            letterCounter = GuessGrid.ColumnDefinitions.Count - 1;
        }//if counter less than 0
    }//Backspace_Clicked()

    private void Enter_Clicked(object sender, EventArgs e)
    {
            if (guessCounter < 6)
            {
                guess = "";
                int rowIndex = guessCounter;
                bool isRowFull = true;

                foreach (var child in GuessGrid.Children)
                {
                    if (GuessGrid.GetRow(child) == rowIndex && child is Label label)
                    {
                        if (string.IsNullOrEmpty(label.Text))
                        {
                            isRowFull = false;
                            break;
                        }
                        else
                        {
                            guess += label.Text;
                        }
                    }
                }//for each letter - add to guess string
                guess = guess.ToLower();
                ValidWord();
                if(!validInput)
                {
                    DisplayAlert("Invalid Input", "Word not in word list", "Ok");
                }//if invalid
                else
                {
                    if (isRowFull)
                    {
                        guesses.Add(guess);
                        checkWord();
                        guessCounter++;
                        letterCounter = 0;
                        Debug.WriteLine(guess);
                    }
                }//else
            }//if less than 6 guesses
    }//Enter_Clicked()

    private async void checkWord()
    {
        enter_btn.IsEnabled = false;
        int row = guessCounter;

        List<char> chosenLetters = chosenWord.ToList();
        List<int> greenLetters = new List<int>();
        List<int> yellowLetters = new List<int>();


        
        for (int col = 0; col < 5; col++)
        {
            //go through word looking for correct spots
            if (guess[col] == chosenWord[col])
            {
                //record green indexes
                greenLetters.Add(col);
                correctLettersGuessed.Add(guess[col]);
                chosenLetters.Remove(guess[col]);
            }//if letter == letter
        }//for green spaces
        for (int col = 0; col < 5; col++)
        {
            if (chosenLetters.Contains(guess[col]))
            {
                yellowLetters.Add(col);
                chosenLetters.Remove(guess[col]);
                yellowLettersGuessed.Add(guess[col]);
            }//record yellow letters
        }//for yellow spaces
       
        for (int column = 0; column < 5; column++)
        {
            Frame currentFrame = null;

            foreach (var child in GuessGrid.Children)
            {
                if (child is Frame && GuessGrid.GetRow(child) == row && GuessGrid.GetColumn(child) == column)
                {
                    currentFrame = (Frame)child;
                    break;
                }
            }//for each frame
            if (currentFrame != null)
            {
                //frame animation
                await currentFrame.ScaleTo(1.2, 100);
                await Task.Delay(100);
                await currentFrame.ScaleTo(1, 100);

                if (greenLetters.Contains(column))
                {
                    //turn square green
                    currentFrame.BackgroundColor = Color.FromHex("#019a01");
                    ChangeKeyGreen(guess[column]);
                }//if green letter
                else if (yellowLetters.Contains(column))
                {
                    //else turn square yellow
                    currentFrame.BackgroundColor = Color.FromHex("#ffc425");
                    if (!correctLettersGuessed.Contains(guess[column]))
                    {
                        ChangeKeyYellow(guess[column]);
                    }
                }//if yellow letter
                else
                {
                    //turn square darker grey
                    currentFrame.BackgroundColor = Color.FromHex("#878686");
                    if (!correctLettersGuessed.Contains(guess[column]))
                    {
                        wrongLettersGuessed.Add(guess[column]);
                        if (!yellowLettersGuessed.Contains(guess[column]))
                            ChangeKeyGrey(guess[column]);
                    }
                }//or else turn grey
            }//for each letter in guess
        }//for 
        if(greenLetters.Count == 5) 
        {
            won = true;
            Win();
        }//if all letters are green
        chosenLetters.Clear();
        greenLetters.Clear();
        yellowLetters.Clear();
        if (guessCounter == 6 && !won)// if run out of guesses
           Lose();
    }//CheckWord()

    private void ValidWord()
    {
        for (int i = 0; i < words.Count; i++)
        {
            if (guess.Equals(words[i]))
            {
                validInput = true;
                return;
            }//if input does exist in list
            else
                validInput = false;
        }//for each word in list
    }//ValidWord()

    private async void Win()
    {
        gamesPlayed++;
        playAgain_btn.IsVisible = true;
        numWins++;
        streak++;
        await DisplayAlert("Win!", "You Won!", "Ok");
        DisableKeyboard();
        gameRunning = false;
        SaveDetails();
        
        StatsPopUp statsPage = new StatsPopUp();
        statsPage.UpdateStatistics();
        await this.ShowPopupAsync(statsPage);
    }//Win()
    private async void Lose()
    {
        gamesPlayed++;
        playAgain_btn.IsVisible = true;
        streak = 0;
        DisplayAnswer(chosenWord);
        DisableKeyboard();
        gameRunning = false;
        SaveDetails();
        StatsPopUp statsPage = new StatsPopUp();
        statsPage.UpdateStatistics();
        await this.ShowPopupAsync(statsPage);
    }//Lose()

    private async Task SaveDetails()
    {
        try
        {
            //write details to a file
            using (StreamWriter sw = new StreamWriter(SaveFilePath))
            {
                //number of wins
                sw.WriteLine(numWins);
                //streak
                sw.WriteLine(streak);
                //games played
                sw.WriteLine(gamesPlayed);
            }//streamWriter
        }//try
        catch(Exception ex) 
        {
            await Shell.Current.DisplayAlert("Error saving details", ex.Message, "OK");
        }//catch
    }//SaveDetails()

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
                    numWins = int.Parse(sr.ReadLine());
                    //streak
                    streak = int.Parse(sr.ReadLine());
                    //games played
                    gamesPlayed = int.Parse(sr.ReadLine());

                    //win percentage
                    if (gamesPlayed != 0)
                        percentWon = (int)(((double)numWins / gamesPlayed) * 100);
                    else
                        percentWon = 0;
                }//StreamReader
            }//try
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error reading details from file", ex.Message, "OK");
            }//catch
        }//if file exists
        else
        {
            numWins = 0;
            streak = 0;
            gamesPlayed = 0;
        }//else no file - initialise to 0
    }//GetDetails()

    public void RestartGame()
    {
        //clear grid
        ClearGrid();

        //redraw grid
        DrawGrid();

        //enable keyboard
        EnableKeyboard();
        ResetKeyColor();
        gameRunning = true;

        playAgain_btn.IsVisible = false;
        isHardMode = (bool)Application.Current.Resources["IsHardMode"];
    }//RestartGame()
    private void ClearGrid()
    {
        foreach (var label in addedLabels)
        {
            GuessGrid.Children.Remove(label);
        }//remove labels

        foreach (var frame in addedFrames)
        {
            GuessGrid.Children.Remove(frame);
        }//removes frames

        //reset variables / clear lists
        addedLabels.Clear();
        addedFrames.Clear();
        guesses.Clear();
        correctLettersGuessed.Clear();
        yellowLettersGuessed.Clear();
        wrongLettersGuessed.Clear();
        guess = "";
        letterCounter = 0;
        guessCounter = 0;
        won = false;
    }//ClearGrid();

    private async void DisplayAnswer(string answer)
    {
        await DisplayAlert("Answer", $"The correct word was: {answer}", "OK");
    }//DisplayAnswer()

    private async void GoToSettings(object sender, EventArgs e)
    {
        if (!gameRunning)
        {
            SettingsPopUp settingsPage = new SettingsPopUp();
            await this.ShowPopupAsync(settingsPage);
        }
    }//GoToSettings()
    private async void GoToStats(object sender, EventArgs e)
    {
        StatsPopUp statsPage = new StatsPopUp();
        statsPage.UpdateStatistics();
        await this.ShowPopupAsync(statsPage);
    }//GoToStats()
    private async void GoToHow(object sender, EventArgs e)
    {
        HowToPlay howToPlayPage = new HowToPlay();
        await this.ShowPopupAsync(howToPlayPage);
    }//GoToSettings()

    private void DisableKeyboard()
    {
        foreach (var button in keys)
        {
            button.IsEnabled = false;
        }//for each key
    }//DisableKeyboard()

    private void EnableKeyboard()
    {
        foreach(var button in keys)
        {
            button.IsEnabled = true;
        }//for each key
    }//EnableKeyboard()

    private void ChangeKeyGreen(char key)
    {
        char lowerKey = char.ToLower(key);

        foreach (var button in keys)
        {
            if (button.Text != null && char.ToLower(button.Text[0]) == lowerKey)
            {
                button.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break; //break after finding button
            }//if correct key
        }//for each key
    }//ChangeKeyGreen()

    private void ChangeKeyYellow(char key)
    {
        char lowerKey = char.ToLower(key);

        foreach (var button in keys)
        {
            if (button.Text != null && char.ToLower(button.Text[0]) == lowerKey)
            {
                button.BackgroundColor = Color.FromHex("#ffc425");
                break; 
            }//if correct key
        }//for each key
    }//ChangeKeyYellow()
    private void ChangeKeyGrey(char key)
    {
        char lowerKey = char.ToLower(key);

        foreach (var button in keys)
        {
            if (button.Text != null && char.ToLower(button.Text[0]) == lowerKey)
            {
                button.BackgroundColor = Color.FromHex("#878686");
                break;
            }//if correct key
        }//for each key
        
        if (isHardMode)
        {
            foreach (var button in keys)
            {
                if (button.Text != null && char.ToLower(button.Text[0]) == lowerKey)
                {
                    button.IsEnabled = false;
                    break;
                }//if correct button
            }//for each key
        }//if hard mode is enabled
    }//ChangeKeyGrey()

    private void ResetKeyColor() 
    {
        foreach (var button in keys) 
        {
            button.BackgroundColor = Color.FromHex("#d6d4d4");
        }//for each key
    }//ResetKeyColor()

}//Class

