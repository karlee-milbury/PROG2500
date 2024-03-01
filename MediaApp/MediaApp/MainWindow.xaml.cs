using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using TagLib;

namespace MediaApp
{

    public partial class MainWindow : Window
    {
        public ICommand ExitCommand { get; } // for exit command binding
        public MainWindow()
        {
            InitializeComponent();
            ExitCommand = new RelayCommand(Exit); // exit command binding
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            media.Play();
            titleControl.TitleText = "Now Playing"; // Set title text top now playing when the button is clicked 
            titleControl.Visibility = Visibility.Visible;
        }

        private void MenuItem_EditTags_Click(object sender, RoutedEventArgs e)
        {
            titleControl.TitleText = "Edit Tags"; // set title text to edit tags when its clicked
            titleControl.Visibility = Visibility.Visible;

            btnSave.Visibility = Visibility.Visible; // make button appear when you're editing tags so they can save it 
        }


        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            media.Pause();
            titleControl.Visibility = Visibility.Collapsed; // hide the user control now playing because its not playing anymore
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            media.Stop();
            titleControl.Visibility = Visibility.Collapsed; // hide now playing when stopped
        }



        private void OpenCommand_Executed(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                media.Source = new Uri(openFileDialog.FileName);

                // get album art from the mp3 file (set in windows media player)
                ExtractAlbumArt(openFileDialog.FileName);

                // get all its meta data (title, author etc)
                ExtractMetadata(openFileDialog.FileName);

                // display the metadata
                ShowMetadataTextBoxes();
            }
        }

        private void Exit()
        {
            this.Close();
        }

        // make the metadata visible
        private void ShowMetadataTextBoxes()
        {
            txtTitle.Visibility = Visibility.Visible;
            txtArtist.Visibility = Visibility.Visible;
            txtAlbum.Visibility = Visibility.Visible;
            txtYear.Visibility = Visibility.Visible;
        }
        private void media_MediaOpened(object sender, RoutedEventArgs e)
        {
    
        }
        private void ExtractAlbumArt(string filePath)
        {
            try
            {
                TagLib.File file = TagLib.File.Create(filePath);
                var tag = file.Tag;

                if (tag.Pictures.Length >= 1)
                {
                    // Get the album art
                    var bin = tag.Pictures[0].Data.Data;
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = new MemoryStream(bin);
                    bitmapImage.EndInit();

                    // Set the album art image source
                    albumArtImage.Source = bitmapImage;
                }
                else
                {
                    // cant find any album art :(
                    albumArtImage.Source = null;
                }
            }
            catch (Exception ex)
            {
                // handle exception if this doesn't work
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveMetadataChanges(media.Source.LocalPath);

            titleControl.Visibility = Visibility.Collapsed; // hide the title after saving to exit the editing mode and go back to the now playing screen (minus the title until play is clicked)
            btnSave.Visibility = Visibility.Collapsed; // hide the button because you're exiting edit mode
        }

        // function to save the changes when the user edits the metadata
        private void SaveMetadataChanges(string filePath)
        {
            try // using taglib 
            {
                TagLib.File file = TagLib.File.Create(filePath);
                var tag = file.GetTag(TagLib.TagTypes.Id3v2);

                // Update metadata with new values
                tag.Title = txtTitle.Text;
                tag.Performers = new[] { txtArtist.Text };
                tag.Album = txtAlbum.Text;
                if (int.TryParse(txtYear.Text, out int year))
                {
                    tag.Year = (uint)year;
                }

                // Save changes
                file.Save();
                MessageBox.Show("Metadata saved successfully.");
            }
            catch (Exception ex)
            {
                // Handle exception
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // VOLUME SLIDER
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (media != null)
            {
                media.Volume = volumeSlider.Value; // set the volume based on the sliders location
            }
        }

        private void ExtractMetadata(string filePath)
        {
            try
            {
                TagLib.File file = TagLib.File.Create(filePath);
                var tag = file.Tag;

                // Update UI with metadata
                txtTitle.Text = tag.Title;
                txtArtist.Text = tag.FirstPerformer;
                txtAlbum.Text = tag.Album;
                txtYear.Text = tag.Year.ToString();
            }
            catch (Exception ex)
            {
                // Handle exception (e.g., file not found, invalid format)
                MessageBox.Show("Error: " + ex.Message);
            }
        }

    }
}


// CLASS FOR COMMAND BINDING
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public RelayCommand(Action execute, Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object parameter) => _execute();

}
