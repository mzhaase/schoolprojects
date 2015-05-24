// Laden benötigter Bibliotheken zum Programmieren
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace bpache
{
    public class Logger
    {
        /// <summary>
        /// Erstellt ein statisches Objekt aus dieser Klasse selbst, welches Threadunabhängig aufgerufen werden kann
        /// </summary>
        private static volatile Logger instanz = new Logger();

        /// <summary>
        /// Erstellt ein zusätzliches Objekt ohne Inhalt, welches den Zugriff steuert
        /// </summary>
        private static object zugriff = new Object();

        /// <summary>
        /// Deklaration benötigter Variablen für Protokoll- und Dateieinträge
        /// </summary>
        private string log;
        private StreamWriter logFile;
        private bool update;

        /// <summary>
        /// Konstruktor der Klasse
        /// </summary>
        private Logger()
        {
            // Zuweisung der Variablen für Protkoll und Dateizugriff
            //_log = new List<string>();
        	logFile = new StreamWriter("./access.log");
            update = false;
        }
        
        /// <summary>
        /// Eigenschaft der Klasse als Verweis auf eigene Objekt
        /// </summary>
        public static Logger Instanz
        {
            get
            {
         		return instanz;
            }
        }

        /// <summary>
        /// Methode zum Hinzufügen einer Meldung in das Protokoll
        /// </summary>
        /// <param name="type">Meldungsart</param>
        /// <param name="args">Meldung</param>
		public void Add(int type, string args)
		{
            // Erstellen eines Zeichensatzes mit Zeitstempel vor der Meldung
            DateTime now = DateTime.Now;
            string eintrag = now + ": " + args + "\n";

            // Sperren des leeren Objekts für weitere Bearbeitung
			lock (zugriff)
			{
                // Eintragen der Meldung ins Protokoll und Datei
                log += eintrag;
				logFile.WriteLine(eintrag);
                update = true;
			}
		}

        /// <summary>
        /// Methode zum Ausgeben der bisherigen Protokotolleinträge
        /// </summary>
        public string Show()
        {
            lock (zugriff)
            {
                update = false;
                return log;
            }
        }

        /// <summary>
        /// Methode zum Prüfen ob Änderungen im Protokoll vorhanden sind
        /// </summary>
        public bool Update()
        {
            lock (zugriff)
            	return update;
        }

        /// <summary>
        /// Methode zum Stoppen des Dateizugriffs
        /// </summary>
        public void Close()
        {
            logFile.Close();
        }
    } 
}