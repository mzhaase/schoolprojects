// Reciever class. Part of bpache webserver
// Opens TCP listener object and puts open connections
// Into ThreadPool.Queue 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace bpache
{
    public class Recv
    {
        //Initialisiere Variablen
        public TcpListener listener = null;
        public TcpClient client;

        private int port;
        private IPAddress host;
        private int maxThreads;
        CancellationTokenSource cts = new CancellationTokenSource();
          
        /// <summary>
        /// Startet Server
        /// </summary>
        public void Start()
        {   
            //Setze Einstellungen
            port = Properties.Settings.Default.port;
            host = IPAddress.Parse(Properties.Settings.Default.host);
            maxThreads = Properties.Settings.Default.maxThreads;
            ThreadPool.SetMaxThreads(maxThreads, maxThreads);  
    
            //Öffnet TCP Listener
            listener = new TcpListener(host, port);
            listener.Start();
            Logger.Instanz.Add(1, "Starte Webserver auf " + host + ":" + port);
            Thread thread = new Thread(new ThreadStart(Server));
            thread.Start();
        }

        /// <summary>
        /// Stoppt Server
        /// </summary>
        public void Stop()
        {
            //Stoppt TCP Listener
            //cts.Cancel();
            listener.Stop();
            Logger.Instanz.Add(1, "Stoppe Webserver auf " + host + ":" + port);

        }

        /// <summary>
		/// Stellt MimeType aus Dateinamen fest.
		/// </summary>
		/// <param name="filename">Dateiname im Format AbCd14512.ext. Darf nur einen Punkt enthalten</param>
		/// <returns>Mime Type</returns>
        private string MimeType(string filename)
        {            
            StreamReader sr;
            String line;
			String mimeType = "";
			String fileExtension;
			String mimeExtension = "";
			
			filename = filename.ToLower();

            // Sollte die Datei nicht existieren gibt es auch keinen MimeType
            if (filename == "") return "";

            // '.' ist der Start der Extension.
			int extPos = filename.IndexOf(".");
			fileExtension = filename.Substring(extPos);
			
			try
			{
				//Alle vorhandenen Mime-Typen befinden sich in der mimeTypes.dat.
				sr = new StreamReader("mimeTypes.dat");

				while ((line = sr.ReadLine().ToLower()) != null)
				{
					line.Trim();
					if (line.Length > 0)
					{
						//In der mimetypes.dat ist der Seperator ';'
						extPos = line.IndexOf(";");												
						mimeExtension = line.Substring(0, extPos);
						mimeType = line.Substring(extPos + 1);
						
                        // Wenn die Dateiendung mit der in der mimeTypes.dat übereinstimmt
                        // Schleife unterbrechen, mimeType ist dann korrekt.
						if (mimeExtension == fileExtension)
							break;
					}
				}
			}
			catch (Exception exc)
			{
                
                Logger.Instanz.Add(3, "Fehler beim Lesen des MimeTypes!" + exc);				
			}

            if (mimeExtension == fileExtension) return mimeType;
            else return "text/html";
		}

        /// <summary>
        /// Konstruiert Header String.
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns>string Header mit allen Header Informationen</returns>
        private string BuildHeader(string filename, int length) 
        {
            String header;
            String statusCode;
            String httpVersion = "HTTP/1.1";
            String mimeType = MimeType(filename);            
            String date = DateTime.Now.ToString("ddd, dd mmm yyyy hh:mm:ss K");
            String path = Properties.Settings.Default.rootPath + filename; ;
            String contentLength = length.ToString();

            if (filename != "")
            {
                statusCode = "200 OK";
            }
            else
            { 
                statusCode = "404 NOT FOUND";
            }

            header = httpVersion + " " + statusCode + "\r\n";
            header += "Date: " + date + "\r\n";
            header += "Content-Type: " + mimeType + "\r\n";
            header += "Content-Length: " + contentLength + "\r\n";

            return header;
        }

        /// <summary>
        /// Parsed den request.
        /// </summary>
        /// <param name="request">HTTP Request</param>
        /// <returns>string[] = (Methode, URI, Host)</returns>
        private string[] ParseRequest(string request)
        {
            // Parses request string. Gibt Array der Form
            // [methode, URI, Host] zurück
            request = request.ToLower();
            // matches GET URI http/1.1 oder POST URI http/1.1
            String methodURIPattern = @"(get|post)\s*(\S*)\s*http\/1.1";
            Regex reMethodURI = new Regex(methodURIPattern);
            // matches Host: URI
            String hostPattern = @"host: (\S*)";
            Regex reHost = new Regex(hostPattern);

            String requestMethod;
            String requestURI;
            String requestHost;

            Match methodMatch = reMethodURI.Match(request);
            Match hostMatch = reHost.Match(request);

            requestMethod = methodMatch.Groups[1].Value;
            requestURI = methodMatch.Groups[2].Value;
            requestHost = hostMatch.Groups[1].Value;
            //Console.WriteLine("Method: " + requestMethod);
            //Console.WriteLine("URI: " + requestURI);
            //Console.WriteLine("Host: " + requestHost);
            string[] requestArray = { requestMethod, requestURI, requestHost };
            return (requestArray);
        }

        /// <summary>
        /// Server. Sendet Daten an den Client
        /// </summary>
        /// <param name="obj">Thread</param>
        public void Server()
        {
            //var client = (TcpClient)obj;
            // Erstelle Netzwerk-Stream
            //NetworkStream stream = client.GetStream();
            //MainWindow.Event.LogIn("Eingehende Verbindung...");
            // Lesebuffer 8196 default auf den meisten Webservern
            //byte[] recvBuffer = new byte[8196];
            // Kompletter Request
            //StringBuilder request = new StringBuilder();
            //int bytesRead = 0;

            // Eingehende Verbindung evtl. größer als Lesebuffer. 
            while(true)
            {
                Socket webSocket = listener.AcceptSocket();
                
                if (webSocket.Connected)
                {
                    Byte[] recvBuff = new Byte[8196];
                    int it = webSocket.Receive(recvBuff, recvBuff.Length, 0);

                    String recvBuffString = Encoding.ASCII.GetString(recvBuff);


                    // Liefert das geparste Array
                    String[] requestArray = ParseRequest(recvBuffString);
                    String filename = requestArray[1].Replace("/", "\\");
                    String httpResponse;
                    String path = Properties.Settings.Default.rootPath + filename;
                    FileInfo f = new FileInfo(path);
                  
                    if (File.Exists(path) == true)
                    {

                        FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                        BinaryReader br = new BinaryReader(fs);
                        byte[] bytes = new byte[fs.Length];
                        int read;
                        httpResponse = "";
                        httpResponse = BuildHeader(filename, bytes.Length);
                        httpResponse += "\r\n\r\n";
                        Byte[] sendData = Encoding.ASCII.GetBytes(httpResponse);
                        webSocket.Send(sendData);

                        if ((read = br.Read(bytes, 0, bytes.Length)) != 0)
                        {                            
                            httpResponse += Encoding.ASCII.GetString(bytes, 0, read);
                        }
                        br.Close();
                        fs.Close();
                                              
                        webSocket.Send(bytes);

                        Logger.Instanz.Add(2, "Datei " + path + " gelesen.");
                    }
                    else
                    {
                        Logger.Instanz.Add(2, "Datei " + path + " nicht gefunden. 404.");
                        filename = "";
                        httpResponse = BuildHeader(filename, 0);
                        httpResponse += "\r\n\r\n";
                        Byte[] sendData = Encoding.ASCII.GetBytes(httpResponse);
                        webSocket.Send(sendData);
                    }
                    
                    
                    //stream.Write(System.Text.Encoding.UTF8.GetBytes(httpResponse), 0, System.Text.Encoding.UTF8.GetBytes(httpResponse).Length);
                    //stream.Flush();
                    //client.Close();
                    webSocket.Close();    
                }
                
            }
        }
    }
}