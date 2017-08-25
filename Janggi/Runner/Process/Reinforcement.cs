using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Runner.Process
{
	public class Reinforcement
	{
		async void openServerAsync()
		{
			TcpListener tcpListener = new TcpListener(System.Net.IPAddress.Any, 9998);
			tcpListener.Start();
			Task<TcpClient> t = tcpListener.AcceptTcpClientAsync();
			await t;
			TcpClient server = t.Result;
			NetworkStream stream = server.GetStream();

			while (true)
			{
				byte[] data = new byte[256];
				string responseData = string.Empty;
				Console.WriteLine("wait message...");
				int bytes = stream.Read(data, 0, data.Length);
				responseData = System.Text.Encoding.BigEndianUnicode.GetString(data, 0, bytes);
				Console.WriteLine($"Received : {responseData}");
			}
		}

		public Reinforcement()
		{
			//openServerAsync();
			Janggi.TensorFlow.TcpCommClient tcpCommClient = new Janggi.TensorFlow.TcpCommClient();
			tcpCommClient.Connect("localhost", 9999);
		
		}
	}
}
