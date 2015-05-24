using System;
using System.Linq;
using System.Net;
using System.Windows;

namespace bpache
{
    /// <summary>
    /// Interaktionslogik für Einstellungen.xaml
    /// </summary>
    public partial class Einstellungen : Window
    {
        /// <summary>
        /// Konstruktur des WPF Fensters
        /// </summary>
        public Einstellungen()
        {
            InitializeComponent();
            LadeEinstellungen();
        }

        /// <summary>
        /// Methode zum Laden der Einstellungen aus Properties.Settings.settings
        /// </summary>
        private void LadeEinstellungen()
        {
            textBoxAdresse.Text = Properties.Settings.Default.host;
            textBoxPort.Text = Convert.ToString(Properties.Settings.Default.port);
            textBoxPfad.Text = Properties.Settings.Default.rootPath;
            textBoxIndex.Text = Properties.Settings.Default.indexFile;
            comboBoxThreads.SelectedIndex = Properties.Settings.Default.maxThreads - 1;
        }

        /// <summary>
        /// Methode zum Setzen der Einstellungen in Properties.Settings.settings
        /// </summary>
        private void SetzeEinstellungen()
        {
            Properties.Settings.Default.host = textBoxAdresse.Text;
            Properties.Settings.Default.port = Convert.ToInt32(textBoxPort.Text);
            Properties.Settings.Default.rootPath = textBoxPfad.Text;
            Properties.Settings.Default.indexFile = textBoxIndex.Text;
            Properties.Settings.Default.maxThreads = comboBoxThreads.SelectedIndex + 1;
        }

        /// <summary>
        /// Clickevent zum Übernehmen geänderter Einstellungen aus dem Einstellungsfenster
        /// </summary>
        void Click_buttonAccept(object sender, RoutedEventArgs e)
        {
            MessageBoxResult ergebnis = MessageBox.Show("Einstellungen wirklich übernehmen?", "Einstellungen", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (ergebnis == MessageBoxResult.OK)
            {
                SetzeEinstellungen();
                this.Close();
            }
        }

        /// <summary>
        /// Clickevent zum Zurücksetzen der Werte im Einstellungsfenster
        /// </summary>
		void Click_buttonReset(object sender, RoutedEventArgs e)
		{
            LadeEinstellungen();
		}
    }
}
