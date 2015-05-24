//  Laden aller benötigten Bibliotheken
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading;

namespace bpache
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Erstellen eines Objektes aus der Klasse Recv für den Webdienst
        /// </summary>
        public static Recv listener = new Recv();
        public static Thread webserver;    

        /// <summary>
        /// Konstruktor des WPF Fensters
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Update();
            Timer();
            Logger.Instanz.Add(0, "Programmstart...");
        }

        /// <summary>
        /// Methode zur Prüfung ob Webdienst läuft und setzen der Bedienung und Anzeige
        /// </summary>
        private void Update()
        {
            labelAddressStatus.Content = Properties.Settings.Default.host.ToString();
            labelPortStatus.Content = Properties.Settings.Default.port.ToString();

            if (webserver == null)
            {
                labelWebserverStatus.Content = "Läuft nicht";
                labelWebserverStatus.Foreground = new SolidColorBrush(Colors.Red);
                buttonStart.IsEnabled = true;
                buttonStop.IsEnabled = false;
            }
            else
            {
                labelWebserverStatus.Content = "Läuft";
                labelWebserverStatus.Foreground = new SolidColorBrush(Colors.Green);
                buttonStart.IsEnabled = false;
                buttonStop.IsEnabled = true;
            }
        }

        /// <summary>
        /// Methode zum Start eines Timers für regelmäßige Aktualisierung des Fensters
        /// </summary>
        private void Timer()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Tick;
            timer.Start();
        }

        /// <summary>
        /// Aktualisierung des Fensters
        /// </summary>
        private void Tick(object sender, EventArgs e)
        {
            Update();
        }

        /// <summary>
        /// Starten des Webdienstes
        /// </summary>
        private void Click_buttonStart(object sender, RoutedEventArgs e)
        {
            try
            {
                webserver = new Thread(new ThreadStart(listener.Start));
                webserver.Start();
            }
            catch
            {
                Logger.Instanz.Add(3, "Fehler beim Starten des Webdienstes!");
            }                
        }

        /// <summary>
        /// Stoppen des Webdienstes
        /// </summary>
        private void Click_buttonStop(object sender, RoutedEventArgs e)
        {
            try
            {
                webserver.Abort();
                webserver.Join();
                webserver = null;
            }
            catch
            {
                Logger.Instanz.Add(3, "Fehler beim Stoppen des Webdienstes!");
            }
        }

        /// <summary>
        /// Aufruf eines zusätzlichen Fensters für die Einstellungen
        /// </summary> 
        private void Click_buttonEinstellungen(object sender, RoutedEventArgs e)
        {
            try
            {
                Einstellungen einstellungen = new Einstellungen();
                einstellungen.ShowDialog();
            }
            catch
            {
                Logger.Instanz.Add(3, "Fehler beim Öffnen des Einstellungsfensters!");
            }
        }

        /// <summary>
        /// Aufruf eines zusätzlichen Fensters für das Ereignissprotokoll
        /// </summary> 
        private void Click_buttonProtokoll(object sender, RoutedEventArgs e)
        {
            try
            {
                Protokoll protokoll = new Protokoll();
                protokoll.ShowDialog();
            }
            catch
            {
                Logger.Instanz.Add(3, "Fehler beim Öffnen des Protokollfensters!");
            }
        }

        /// <summary>
        /// Event wenn dieses Hauptfenster geschlossen wird
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (webserver != null)
                {
                    listener.Stop();
                    webserver = null;
                    Logger.Instanz.Close();
                }
                Environment.Exit(-1);
            }
            catch
            {
                Logger.Instanz.Add(3, "Fehler beim Beenden des Programms!");
            }
        }
    }
}
