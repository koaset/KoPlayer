using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using IF.Lastfm.Core.Scrobblers;
using KoPlayer.Playlists;
using Microsoft.Win32;
using System;
using System.IO;
using System.Collections.Generic;
using System.Timers;
using System.ComponentModel;


namespace KoPlayer
{
    public class LastfmHandler
    {
        private const string sessionRegistryPath = "Software\\KoPlayer";
        private const string apiKey = "4eedd270b2824403af15f9a81407f7ce";
        private const string apiSecret = "963517814b9653e6c9eaeab075f0d336";

        public event EventHandler StatusChanged;

        public string Status { get; set; }

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

        private BackgroundWorker scrobbleWorker;
        private List<Scrobble> scrobbleQueue;
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
            Status = "Not Connected";

            scrobbleWorker = new BackgroundWorker();
            scrobbleWorker.WorkerSupportsCancellation = false;
            scrobbleWorker.WorkerReportsProgress = false;
            scrobbleWorker.DoWork += scrobbleWorker_DoWork;

            scrobbleQueue = new List<Scrobble>();
        }

        public void TryResumeSession()
        {
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

        public void ScrobbleSong(Song song)
        {
            scrobbleQueue.Add(new Scrobble(song.Artist, song.Album, song.Title, song.LastPlayed));
            StartWorker();
        }

        private void StartWorker()
        {
            if (!scrobbleWorker.IsBusy)
                scrobbleWorker.RunWorkerAsync();
        }

        private void scrobbleWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            TryScrobble();
        }

        public async void TryScrobble()
        {
            if (scrobbleQueue.Count == 0)
                return;

            var toScrobble = new List<Scrobble>();

            // Only allowed 50 songs in each scrobble
            if (scrobbleQueue.Count > 50)
                toScrobble.AddRange(scrobbleQueue.GetRange(0, 50));
            else
                toScrobble.AddRange(scrobbleQueue);

            var response = await scrobbler.ScrobbleAsync(toScrobble);

            if (response.Success)
                scrobbleQueue.RemoveRange(0, toScrobble.Count);
        }

        private void ChangeStatus(string newStatus)
        {
            Status = newStatus;

            if (Status == "Connected" && scrobbleQueue.Count > 0)
                StartWorker();

            if (StatusChanged != null)
                StatusChanged(this, new EventArgs());
        }

        private void SaveSession(LastUserSession session)
        {
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
