using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using IF.Lastfm.Core.Scrobblers;
using KoPlayer.Playlists;
using Microsoft.Win32;
using System;
using System.IO;


namespace KoPlayer
{
    public class LastfmHandler
    {
        private const string sessionRegistryPath = "Software\\KoPlayer";
        private const string apiKey = "4eedd270b2824403af15f9a81407f7ce";
        private const string apiSecret = "963517814b9653e6c9eaeab075f0d336";

        public event EventHandler StatusChanged;

        public string Status { get; set; }
        public bool Enabled { get; set; }

        public string SessionUserName 
        {
            get 
            {
                if (currentSession != null)
                    return currentSession.Username;
                else
                    return "";
            }
        }

        private IScrobbler scrobbler;
        private LastfmClient client;
        private LastUserSession currentSession;

        public LastfmHandler()
        {
            client = new LastfmClient(apiKey, apiSecret);

            currentSession = LoadSession();

            Initialize();
        }

        public void Initialize()
        {
            Enabled = false;
            Status = "Not Connected";
        }

        public void TryResumeSession()
        {
            Enabled = true;

            if (currentSession != null)
            {
                ChangeStatus("Resuming");

                client.Auth.LoadSession(currentSession);

                ChangeStatus("Not Connected");
                if (client.Auth.UserSession != null)
                {
                    scrobbler = new Scrobbler(client.Auth, client.HttpClient);

                    if (client.Auth.Authenticated)
                        ChangeStatus("Connected");
                }
            }
        }

        public async void TryLoginAsync(string userName, string password)
        {
            Enabled = true;

            ChangeStatus("Connecting");
            var response = await client.Auth.GetSessionTokenAsync(userName, password);
            scrobbler = new Scrobbler(client.Auth, client.HttpClient);

            if (response.Success)
            {
                ChangeStatus("Connected");
                currentSession = client.Auth.UserSession;
                SaveSession(currentSession);
            }
            else
                ChangeStatus("Not connected");
        }

        public async void ScrobbleSong(Song song)
        {
            var scrobble = new Scrobble(song.Artist, song.Album, song.Title, song.LastPlayed);
            var response = await scrobbler.ScrobbleAsync(scrobble);
        }

        private void ChangeStatus(string newStatus)
        {
            Status = newStatus;
            if (StatusChanged != null)
                StatusChanged(this, new EventArgs());
        }

        private void SaveSession(LastUserSession session)
        {
            /*using (var sw = new StreamWriter(sessionPath))
            {
                sw.WriteLine(session.Username);
                sw.WriteLine(session.Token);
            }*/

            var key = Registry.CurrentUser.OpenSubKey(sessionRegistryPath, true);
            if (key == null)
                key = Registry.CurrentUser.CreateSubKey(sessionRegistryPath);
            key.Close();

            var usernameKey = Registry.CurrentUser.OpenSubKey(sessionRegistryPath, true);
            usernameKey.SetValue("username", session.Username);
            usernameKey.Close();

            var tokenKey = Registry.CurrentUser.OpenSubKey(sessionRegistryPath, true);
            tokenKey.SetValue("token", session.Token);
            tokenKey.Close();

            /*
             RegistryKey key = Registry.CurrentUser.OpenSubKey(startupRegKey, true);
            
            if (settings.RunAtStartup)
                key.SetValue(this.Text, Path.Combine(Path.GetDirectoryName(
                    System.Reflection.Assembly.GetExecutingAssembly().Location), "KoPlayer.exe"));
            else
                if (key.GetValue(this.Text) != null)
                    key.DeleteValue(this.Text);*/
        }

        private LastUserSession LoadSession()
        {
            var ret = new LastUserSession();

            var usernameKey = Registry.CurrentUser.OpenSubKey(sessionRegistryPath);
            if (usernameKey == null)
                return null;
            else
            {
                ret.Username = usernameKey.GetValue("username").ToString();
                usernameKey.Close();
            }

            var tokenKey = Registry.CurrentUser.OpenSubKey(sessionRegistryPath);
            if (tokenKey == null)
                return null;
            else
            {
                ret.Token = tokenKey.GetValue("token").ToString();
                tokenKey.Close();
            }

            if (String.IsNullOrEmpty(ret.Username) ||
                String.IsNullOrEmpty(ret.Token))
                return null;

            return ret;
        }
    }
}
