#nullable enable
using cAlgo.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VWAPLib
{
    public class SessionHelper : IDisposable
    {
        private readonly Bars _bars;
        private List<TimeOnly> defaultSession = new List<TimeOnly>() { new TimeOnly(0, 0, 0) };
        public bool IsReady { get; private set; } = false;

        private Window? window = null;
        private WebView? webView = null;
        private bool reset;
        List<TimeOnly> Sessions = new List<TimeOnly>();
        string sessionFile => $"AIO_{_bars.SymbolName}_{_bars.TimeFrame.ShortName}.txt";
        string HtmlFile => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "cAlgo\\Data\\Indicators\\index.html");
        string title => $"{_bars.SymbolName} {_bars.TimeFrame.Name}";
        bool sessionFileExit = false;
        public event Action<SessionHelperEventArgs>? OnInitCompleted;
        public SessionHelper(Bars bars, bool resetSession)
        {
            _bars = bars;

            reset = resetSession;
            
        }
        public void Init()
        {
            webView = new WebView()
            {
                Height = 1024,
                Width = 1280,
                Left = 0,
                Top = 0,
            };
            webView.WebMessageReceived += WebView_WebMessageReceived;

            window = new Window()
            {
                Height = 1024,
                Width = 1280,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Title = title,
                Child = webView,
            };
            window.Closed += Window_Closed;
            // Check the resetSession flag to determine behavior
            if (reset)
            {
                Show(); // Show the user interface for session interaction
            }
            else
            {
                FileRead(); // Attempt to read session data from file
            }
        }
        private void WebView_WebMessageReceived(WebViewWebMessageReceivedEventArgs args)
        {
            
            var msg = args.Message;
            if (!string.IsNullOrEmpty(msg))
                ParseSessions(msg);
            if (Sessions.Count > 0)
            {
                File.WriteAllText(sessionFile, msg.Replace("\"", ""));
            }
            else
            {
                Sessions = defaultSession;
                MessageBox.Show("No session has been selected. Default 0:00 UTC is used.");
            }
            window?.Hide();
            OnInitCompleted?.Invoke(new SessionHelperEventArgs(Sessions)); // Notify the indicator after closing
        }

        private void Window_Closed(WindowClosedEventArgs args)
        {
            MessageBox.Show(Sessions.Count.ToString() + " Sessions selected");
            OnInitCompleted?.Invoke(new SessionHelperEventArgs(Sessions)); // Notify indicator of completion
        }

        public void FileRead()
        {
            if (File.Exists(sessionFile))
            {
                var txt = File.ReadAllText(sessionFile);
                if (!string.IsNullOrEmpty(txt))
                {
                    ParseSessions(txt);
                    if (Sessions.Count > 0)
                        OnInitCompleted?.Invoke(new SessionHelperEventArgs(Sessions)); // Notify indicator of read completion
                    else
                        Show(); // Show UI if no valid sessions are read
                }
                else
                {
                    MessageBox.Show("Previous File Reading Error!");
                    Show();
                }
            }
            else
                Show();
        }

        public void Show()
        {
            var html = "file:///" + HtmlFile.Replace("\\", "/");
            if (window != null)
            {
                if (!File.Exists(HtmlFile))
                    MessageBox.Show($"{HtmlFile} not found");
                window.Show();
                if (webView != null)
                    webView.NavigateAsync(html);
            }
        }

        private void ParseSessions(string txt)
        {
            if (!string.IsNullOrEmpty(txt))
            {
                var strTimes = txt.Replace("\"", "").Split(',');
                if (strTimes.Length > 0)
                {
                    Sessions.Clear();
                    TimeOnly t;
                    for (int i = 0; i < strTimes.Length; i++)
                        if (TimeOnly.TryParse(strTimes[i], out t))
                            Sessions.Add(t);
                }
            }
        }

        public bool IsNewSession(int index)
        {
            if (index > 0)
            {
                var t1 = new TimeOnly(_bars[index].OpenTime.Hour, _bars[index].OpenTime.Minute, 0);
                var t2 = new TimeOnly(_bars[index - 1].OpenTime.Hour, _bars[index - 1].OpenTime.Minute, 0);

                var isNewSession = Sessions.Any(s => s == t1);

                if (!isNewSession && index > 0)
                    isNewSession = Sessions.Any(s => s > t2 && s <= t1);

                if (isNewSession)
                {
                    if (!IsReady)
                        IsReady = true;
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            window = null;
            webView = null;
        }
    }

    public class SessionHelperEventArgs : EventArgs
    {
        public List<TimeOnly> Sessions;
        public SessionHelperEventArgs(List<TimeOnly> sessions)
        {
            Sessions = sessions;
        }
    }

    public enum Direction
    {
        BULL,
        BEAR,
        NONE,
    }
}
