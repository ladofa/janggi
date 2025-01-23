using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static Janggi.Node;

namespace Janggi.Net
{
    public class Message
    {
        public string MessageType { get; set; } = "";

        public List<int> PolicyIndices { get; set; } = new();
        public List<float[]> States { get; set; } = new();
        public List<float> Policies { get; set; } = new();
        public List<float> Values { get; set; } = new();

    }

    

    public class PythonComm
    {
        public TcpClient client = null!;
        public NetworkStream stream = null!;
        public System.IO.StreamReader reader = null!;
        public System.IO.StreamWriter writer = null!;

        public bool Connect(string addr="127.0.0.1", int port=4050)
        {
            lock (this)
            {
                try
                {
                    client = new TcpClient(addr, port);
                    
                    stream = client.GetStream();
                    if (stream == null)
                    {
                        Console.WriteLine("???");
                    }
                    reader = new StreamReader(stream);
                    writer = new StreamWriter(stream);
                    if (writer == null)
                    {
                        Console.WriteLine("???");
                    }

                    
                }
                catch (Exception e)
                {
                    return false;
                }

                return true;
            }
        }

        public bool IsConnected
        {
            get => client != null;// && client.Connected;
        }

        public enum Code : byte
        {
            //--req
            Check = 1,
            Evaluate = 5,
            Train = 6,
            //--ack
            Ok = 101,
            Failed = 102
        }

        // float 배열을 바이트 배열로 변환하는 함수

        //[MethodImpl(MethodImplOptions.Synchronized)]
        public static (float[], float[]) Evaluate(List<Node> children)
        {
            
            PythonComm comm = new PythonComm();
            while (!comm.Connect())
            {
                Console.WriteLine("Connection Refused!");
                Thread.Sleep(1000);
            }
            
            //SEND
            Message message = new Message() { MessageType = "evaluate_send" };

            for (int i = 0; i < children.Count; i++)
            {
                float[] state = NeuralNetwork.Flatten(children[i].board);
                message.States.Add(state);
            }

            string jsonString = JsonSerializer.Serialize(message);
            //byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            comm.writer.WriteLine(jsonString);
            comm.writer.Flush();

            //RECEIVE
            string? recvString = comm.reader.ReadLine();

            comm.client.Close();

            if (recvString == null)
            {
                throw new Exception();
            }

            Message? recvMessage = JsonSerializer.Deserialize<Message>(recvString);
            if (recvMessage == null)
            {
                throw new Exception();
            }
            
            //policy는 원래 list of list인데 flatten해서 받는다.
            List<float> p = recvMessage.Policies; 
            List<float> v = recvMessage.Values;
            var r = (p.ToArray(), v.ToArray());
            return r;

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Train()
        {

        }
    }
}
