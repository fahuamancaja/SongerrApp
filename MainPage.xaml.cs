using Flurl;
using Flurl.Http;
using System.Text.RegularExpressions;

namespace SongerrApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {

            InitializeComponent();
        }

        private async void OnSendClicked(object sender, EventArgs e)
        {
            try
            {
                sendButton.IsEnabled = false;

                var songUrl = inputText.Text.Trim();
                var playlistId = string.Empty;
                var playlistRegex = new Regex(@"list=([^&]*)");
                var songMatch = new Regex(@"v=([^&]*)").Match(songUrl);

                if (playlistRegex.IsMatch(songUrl))
                {
                    playlistId = playlistRegex.Match(songUrl).Groups[1].Value;
                    var confirmPlaylist = await DisplayAlert("Playlist Detected", "Do you want to download the playlist?", "Yes", "No");
                    if (confirmPlaylist)
                    {
                        try
                        {
                            var playlistUrl = "http://url:5002/api/Songerr/DownloadPlaylistSongs"
                                .SetQueryParam("playlistId", playlistId);

                            await ProcessDownload(playlistUrl, songUrl, true);
                        }
                        catch (Exception)
                        {

                            await DisplayAlert("Error", "Unable to download playlist.", "OK");
                        }

                    }
                    else
                    {
                        await DisplayAlert("Action Cancelled", "Playlist download cancelled.", "OK");
                    }
                }
                var confirmSong = await DisplayAlert("Song Detected", "Do you want to download the song?", "Yes", "No");
                if (confirmSong && songMatch.Success)
                {
                    try
                    {
                        await ProcessDownload("http://url:5002/api/Songerr", songUrl, false);
                    }
                    catch 
                    { 
                        await DisplayAlert("Error", "Unable to download song.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Action Cancelled", "Song download cancelled.", "OK");
                }
            }
            catch (FlurlHttpException ex)
            {
                var error = await ex.GetResponseJsonAsync<dynamic>();
                await DisplayAlert("Error", $"An error occurred: {error}", "OK");
            }
            finally
            {
                sendButton.IsEnabled = true;
                inputText.Text = "";
            }
        }

        private async Task ProcessDownload(string endpointUrl, string songUrl, bool isPlaylist = false)
        {
            try
            {
                var response = isPlaylist
                    ? await endpointUrl
                        .WithHeader("X-API-Key", "apiKey")
                        .GetStringAsync()
                    : await endpointUrl
                        .WithHeader("Content-Type", "application/json")
                        .WithHeader("X-API-Key", "apiKey")
                        .PostJsonAsync(new { url = songUrl })
                        .ReceiveString();

                if (response != null)
                {
                    await DisplayAlert("Response", $"{response}", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Error receiving response.", "OK");
                }
            }
            catch (FlurlHttpException ex)
            {
                var error = await ex.GetResponseJsonAsync<dynamic>();
                await DisplayAlert("Error", $"An error occurred: {error}", "OK");
            }
        }
    }
}
