using System;
using System.Windows.Forms;
using System.Reflection;

namespace KoPlayer.Forms
{
    public class Fixes
    {
        /// <summary>
        /// Ups limit on tray icon text from 63 to 127 chars. Found at stackoverflow
        /// </summary>
        /// <param name="ni"></param>
        /// <param name="text"></param>
        public static void SetNotifyIconText(NotifyIcon ni, string text)
        {
            if (text.Length >= 128) throw new ArgumentOutOfRangeException("Text limited to 127 characters");
            Type t = typeof(NotifyIcon);
            BindingFlags hidden = BindingFlags.NonPublic | BindingFlags.Instance;
            t.GetField("text", hidden).SetValue(ni, text);
            if ((bool)t.GetField("added", hidden).GetValue(ni))
                t.GetMethod("UpdateIcon", hidden).Invoke(ni, new object[] { true });
        }
    }
}