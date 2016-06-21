using KoScrobbler;
using KoPlayer.Lib;
using Microsoft.Win32;
using System;
using System.IO;
using System.Collections.Generic;
using System.Timers;
using System.ComponentModel;
using KoScrobbler.Entities;
using KoScrobbler.Interfaces;

namespace KoPlayer
{
    public class LastfmHandler
    {
        private const string sessionRegistryPath = "Software\\KoPlayer";
        private const string apiKey = "4eedd270b2824403af15f9a81407f7ce";
        private const string apiSecret = "963517814b9653e6c9eaeab075f0d336";

        public event EventHandler StatusChanged;

        public string Status { get; set; }

        /*public string SessionUserName
        {
            get
            {
                if (currentSession != null)
                    return currentSession.Username;
                else
                    return "";
            }
        }*/

        public string SessionUserName { get; set; }

        private BackgroundWorker scrobbleWorker;
        private List<Scrobble> scrobbleQueue;
        private ILastFmScrobbler scrobbler;

        public LastfmHandler()
        {
            scrobbler = new KoScrobbler.KoScrobbler(apiKey, apiSecret);
            scrobbler.SessionKey = LoadSession();

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
        /*
        public void TryResumeSession()
        {
            if (string.IsNullOrEmpty(sessionKey))
                return;

            ChangeStatus("Resuming");

            client.Auth.LoadSession(currentSession);

            ChangeStatus("Not Connected");

            if (client.Auth.UserSession == null)
                return;

            scrobbler = new Scrobbler(client.Auth, client.HttpClient);

            if (client.Auth.Authenticated)
                ChangeStatus("Connected");
        }*/

        public void TryLoginAsync(string userName, string password)
        {
            ChangeStatus("Connecting");

            try
            {
                var response = scrobbler.GetMobileSession(userName, password);
                
                if (response.Success)
                {
                    ChangeStatus("Connected");
                    scrobbler.SessionKey = response.SessionKey;
                    SaveSession(response.SessionKey);
                }
                else
                    ChangeStatus("Not connected");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("An error occured: " + ex.ToString());
                ChangeStatus("Connection problem");
            }
        }

        public void ScrobbleSong(Song song)
        {
            scrobbleQueue.Add(new Scrobble(song.Artist, song.Album, song.Title, DateTime.UtcNow));
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

        public void TryScrobble()
        {
            if (scrobbleQueue.Count == 0)
                return;

            var toScrobble = new List<Scrobble>();

            // Only allowed 50 songs in each scrobble
            if (scrobbleQueue.Count > 50)
                toScrobble.AddRange(scrobbleQueue.GetRange(0, 50));
            else
                toScrobble.AddRange(scrobbleQueue);

            try
            {
                var response = scrobbler.TryScrobble(toScrobble);

                if (response.Success)
                    scrobbleQueue.RemoveRange(0, toScrobble.Count);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex);
            }
        }

        private void ChangeStatus(string newStatus)
        {
            Status = newStatus;

            if (Status == "Connected" && scrobbleQueue.Count > 0)
                StartWorker();

            if (StatusChanged != null)
                StatusChanged(this, new EventArgs());
        }

        private void SaveSession(string sessionKey)
        {
            var key = Registry.CurrentUser.OpenSubKey(sessionRegistryPath, true);
            if (key == null)
                key = Registry.CurrentUser.CreateSubKey(sessionRegistryPath);
            key.Close();

            /*var usernameKey = Registry.CurrentUser.OpenSubKey(sessionRegistryPath, true);
            usernameKey.SetValue("username", session.Username);
            usernameKey.Close();*/

            var tokenKey = Registry.CurrentUser.OpenSubKey(sessionRegistryPath, true);
            tokenKey.SetValue("token", sessionKey);
            tokenKey.Close();
        }

        private string LoadSession()
        {
            var ret = string.Empty;

            /*var usernameKey = Registry.CurrentUser.OpenSubKey(sessionRegistryPath);
            if (usernameKey == null)
                return null;
            else
            {
                ret.Username = usernameKey.GetValue("username").ToString();
                usernameKey.Close();
            }*/

            var tokenKey = Registry.CurrentUser.OpenSubKey(sessionRegistryPath);
            if (tokenKey == null)
                return null;
            else
            {
                ret = tokenKey.GetValue("token").ToString();
                tokenKey.Close();
            }

            return ret;
        }
    }
}
