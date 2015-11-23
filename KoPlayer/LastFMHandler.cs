using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lpfm.LastFmScrobbler;
using Lpfm.LastFmScrobbler.Api;
using Microsoft.Win32;

namespace KoPlayer
{
    class LastFMHandler
    {
        private const string LpfmRegistryNameSpace = "HKEY_CURRENT_USER\\Software\\LastFmScrobbler.KoPlayer";
        private string apiKey;
        private string apiSecret;

        private readonly QueuingScrobbler scrobbler;

        public LastFMHandler(string apiKey, string apiSecret)
        {
            this.apiKey = apiKey;
            this.apiSecret = apiSecret;

            string sessionKey = GetSessionKey(apiKey, apiSecret);

            if (String.IsNullOrEmpty(sessionKey))
            {
                throw new Exception("Invalid session key");
            }
            else
                scrobbler = new QueuingScrobbler(apiKey, apiSecret, sessionKey);
        }

        private static string GetSessionKey(string apiKey, string apiSecret)
        {
            const string sessionKeyRegistryKeyName = "LastFmSessionKey";

            // try get the session key from the registry
            string sessionKey = GetRegistrySetting(sessionKeyRegistryKeyName, null);

            // if no key found, try getting it online
            if (string.IsNullOrEmpty(sessionKey))
            {
                // instantiate a new scrobbler
                var scrobbler = new Scrobbler(apiKey, apiSecret);

                try
                {
                    sessionKey = scrobbler.GetSession();
                    SetRegistrySetting(sessionKeyRegistryKeyName, sessionKey);
                }
                catch (LastFmApiException exception)
                {
                    MessageBox.Show(exception.Message);
                    // get a url to authenticate this application
                    string url = scrobbler.GetAuthorisationUri();
                    // open the URL in the default browser
                    Process.Start(url);
                }
            }

            return sessionKey;
        }

        public static string GetRegistrySetting(string valueName, string defaultValue)
        {
            if (string.IsNullOrEmpty(valueName)) throw new ArgumentException("valueName cannot be empty or null", "valueName");
            valueName = valueName.Trim();

            object regValue = Registry.GetValue(LpfmRegistryNameSpace, valueName, defaultValue);

            if (regValue == null)
            {
                // Key does not exist
                return defaultValue;
            }
            else
            {
                return regValue.ToString();
            }
        }

        public static void SetRegistrySetting(string valueName, string value)
        {
            if (string.IsNullOrEmpty(valueName)) throw new ArgumentException("valueName cannot be empty or null", "valueName");
            valueName = valueName.Trim();

            Registry.SetValue(LpfmRegistryNameSpace, valueName, value);
        }
    }
}
