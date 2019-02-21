using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Janggi.TensorFlow
{

	public enum NetworkKinds : byte
	{
		Policy = 1,
		Value = 2,
		Null = 3
	}

	/// <summary>
	/// Python 구현체와 통신하기 위한 모듈
	/// </summary>
	public class TcpCommClient
	{
		TcpClient client;
		NetworkStream stream;
		System.IO.BinaryReader reader;
		System.IO.BinaryWriter writer;

		public bool IsConnected
		{
			get => client != null;// && client.Connected;
		}

		public void ConfirmConnection()
		{
			if (!IsConnected)
			{
				Console.WriteLine("Try to connect ..");
				while (!Connect("localhost", 9999))
				{
					Console.WriteLine("ConnectionFailed.");
					System.Threading.Thread.Sleep(1000);
				}
			}
		}

		public enum Code : byte
		{
			//--req
			Check = 1,
			Load = 3,
			Save = 4,
			Evaluate = 5,
			Train = 6,
			//--ack
			Ok = 101,
			Failed = 102
		}

		

		#region encoder and writer

		public void Encode(Move move, byte[] data, ref int index)
		{
			data[index++] = move.From.Byte;
			data[index++] = move.To.Byte;
		}

		public byte[] GetBytes(Code code, NetworkKinds kinds)
		{
			byte[] data = new byte[2];
			data[0] = (byte)code;
			data[1] = (byte)kinds;
			return data;
		}

		public byte[] GetBytes(string str)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(str);
			byte[] data = new byte[bytes.Length + 1];
			data[0] = (byte)bytes.Length;
			for (int i = 0; i < bytes.Length; i++)
			{
				data[i + 1] = bytes[i];
			}

			return data;
		}

		

		public byte[] GetBytes(List<Move> moves)
		{
			byte[] data = new byte[moves.Count * 2 + 1];
			int index = 0;
			data[index++] = (byte)moves.Count;

			foreach (Move move in moves)
			{
				Encode(move, data, ref index);
			}

			return data;
		}

		public byte[] GetBytes(Move move)
		{
			byte[] data = new byte[2];
			data[0] = move.From.Byte;
			data[1] = move.To.Byte;
			return data;
		}

		void write(byte[] data)
		{
			stream.Write(data, 0, data.Length);
		}

		void write(Code code, NetworkKinds kinds)
		{
			write(GetBytes(code, kinds));
		}

		void write(string str)
		{
			write(GetBytes(str));
		}



		void write(List<Move> moves)
		{
			write(GetBytes(moves));
		}

		void write(Move move)
		{
			write(GetBytes(move));
		}

		bool readOk()
		{
			byte[] data = new byte[2];
			stream.Read(data, 0, 2);
			return (data[0] == (byte)Code.Ok);
		}

		byte[] readByteArray()
		{
			byte size = reader.ReadByte();
			byte[] data = reader.ReadBytes(size);
			return data;
		}

		byte[] readByteArray(int length)
		{
			byte[] data = reader.ReadBytes(length);
			return data;
		}

		#endregion

		public bool Connect(string addr, int port)
		{
			lock (this)
			{
				try
				{
					client = new TcpClient(addr, port);
					stream = client.GetStream();
					reader = new System.IO.BinaryReader(stream);
					writer = new System.IO.BinaryWriter(stream);
				}
				catch (Exception e)
				{
					return false;
				}

				return true;
			}
		}

		public void Disconnect()
		{
			lock (this)
			{
				if (!IsConnected) throw new Exception("Is Not Connected.");

				reader.Close();
				stream.Close();
				client.Close();

				stream = null;
				client = null;
				reader = null;
			}
		}

		public bool CheckConnection()
		{
			lock (this)
			{
				if (!IsConnected) throw new Exception("Is Not Connected.");

				write(Code.Check, NetworkKinds.Null);
				return readOk();
			}
		}

		public bool SaveModel(NetworkKinds kinds)
		{
			lock (this)
			{
				ConfirmConnection();

				write(Code.Save, kinds);
				return readOk();
			}
		}

		public float[] EvaluatePolicy(Board board, List<Move> moves)
		{
			lock (this)
			{
				ConfirmConnection();

				write(Code.Evaluate, NetworkKinds.Policy);
				write(board.GetBytes());

				if (readOk())
				{
					byte[] byteArrFrom = readByteArray(90 * sizeof(float));
					byte[] byteArrTo = readByteArray(90 * sizeof(float));

					float[] arrFrom = new float[90];
					float[] arrTo = new float[90];
					Buffer.BlockCopy(byteArrFrom, 0, arrFrom, 0, byteArrFrom.Length);
					Buffer.BlockCopy(byteArrTo, 0, arrTo, 0, byteArrTo.Length);


					float[] proms = new float[moves.Count];
					for (int i = 0; i < moves.Count; i++)
					{
						Move move = moves[i];
						if (move.IsEmpty)
						{
							proms[i] = 0;
						}
						else
						{
							float prom = arrFrom[move.From.Byte] * arrTo[move.To.Byte];
							proms[i] = prom;
						}
					}

					
					

					return proms;
				}
				else
				{
					throw new Exception("read failed");
				}
			}
		}

		public float EvaluateValue(Board board)
		{
			lock (this)
			{
				ConfirmConnection();

				write(Code.Evaluate, NetworkKinds.Value);
				write(board.GetBytes());

				if (readOk())
				{
					return reader.ReadSingle();
				}
				else
				{
					return -1;
				}
			}
		}

		public bool TrainPolicy(List<Tuple<Board, Move>> list)
		{
			lock (this)
			{
				ConfirmConnection();

				write(Code.Train, NetworkKinds.Policy);
				int size = list.Count;
				writer.Write(size);

				for (int i = 0; i < size; i++)
				{
					var tuple = list[i];
					var boardBytes = tuple.Item1.GetBytes();
					write(boardBytes);
					write(tuple.Item2);
				}

				//나머지는 버린다.
				return readOk();
			}
		}

		public bool TrainValue(List<Tuple<Board, float>> list)
		{
			lock (this)
			{
				ConfirmConnection();

				write(Code.Train, NetworkKinds.Value);

				int size = list.Count;
				writer.Write(size);
				foreach (var tuple in list)
				{
					write(tuple.Item1.GetBytes());
					writer.Write(tuple.Item2);
				}

				return readOk();
			}
		}
	}
}
