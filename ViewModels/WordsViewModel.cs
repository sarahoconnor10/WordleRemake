﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Wordle.ViewModels
{
    public class WordsViewModel : INotifyPropertyChanged
    {
        //variables / objects
        public event PropertyChangedEventHandler PropertyChanged;
        private string FilePath => System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, "words.txt");
        private HttpClient httpClient;
        private List<string> ListofWords;
        public List<string> Words => ListofWords;
        private bool isBusy;

        public WordsViewModel()
        {
            //constructor initialise httpClient & list for words, checks if file exists.
            httpClient = new HttpClient();
            ListofWords = new List<string>();

            if(File.Exists(FilePath))
            {
                ReadFromFile();
            }//if file exists
        }//constructor

        private async Task ReadFromFile()
        {
            //reads file in to list.
            try
            {
                ListofWords = File.ReadAllLines(FilePath).ToList();
            }//try
            catch(Exception ex)
            {
                await Shell.Current.DisplayAlert("Error reading file", ex.Message, "OK");
            }//catch
        }//ReadFromFile()

        private async Task GetWords()
        {
            //clears any existing list, retrieves data from URL, splits words into array, assigns array to list, saves the words to a file.
            ListofWords.Clear();
            var response = await httpClient.GetAsync("https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/words.txt");
            string content = await response.Content.ReadAsStringAsync();
            string[] individualWords = content.Split(new[] { '\n' });
            ListofWords.AddRange(individualWords);

            //write to file
            SaveWordsFile(content);

        }//GetWords()

        private async Task SaveWordsFile(string data)
        {
            //saves words to a file.
            try
            {
                File.WriteAllText(FilePath, data);
            }//try
            catch(Exception ex)
            {
                await Shell.Current.DisplayAlert("Error writing file", ex.Message, "OK");

            }//catch
        }//SaveWordsFile()

        public async Task MakeList()
        {
            //if not busy, begins making list from file.
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;

                if (File.Exists(FilePath))
                     ReadFromFile(); 
                else
                    await GetWords();
            }//try
            catch(Exception ex)
            {
                await Shell.Current.DisplayAlert("Error making list", ex.Message, "OK");
            }//catch
            finally
            {
                IsBusy = false;
            }//finally
        }//MakeList()

        public async Task GetWordsFromVM()
        {
            //function to be used in main page
            await MakeList();
        }//GetWordsFromVM

        public bool IsBusy
        {
            //assigns / gets IsBusy
            get => isBusy;
            set
            {
                if (isBusy == value)
                    return;
                isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotBusy));
            }
        }//IsBusy

        public bool IsNotBusy => !IsBusy;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }//OnPropertyChanged()
    }//class
}//namespace


