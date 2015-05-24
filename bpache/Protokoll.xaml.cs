using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace bpache
{
    /// <summary>
    /// Interaktionslogik für Protokoll.xaml
    /// </summary>
    public partial class Protokoll : Window
    {
        /// <summary>
        /// Konstruktur des WPF Fensters
        /// </summary>
        public Protokoll()
        {
            InitializeComponent();
            Update();
            Timer();
        }

        /// <summary>
        /// Methode zum aktualisieren des Protokolls
        /// </summary>
        private void Update()
        {
            textBox1.Text = Logger.Instanz.Show();
            textBox1.Focus();
            textBox1.CaretIndex = textBox1.Text.Length;
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
        /// Timerevent für Aktualisierung des Fensters
        /// </summary>
        private void Tick(object sender, EventArgs e)
        {
            if (Logger.Instanz.Update())
            {
                Update();
            }
        }
    }
}
