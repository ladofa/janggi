using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Janggi.TensorFlow
{
	

	/// <summary>
	/// Python 구현체와 통신하기 위한 모듈
	/// </summary>
	public class TcpCommClient
	{
		TcpClient client;
		NetworkStream stream;
		System.IO.BinaryReader reader;
		System.IO.BinaryWriter writer;

		public enum Code : byte
		{
			//--req
			Check = 1,
			Create = 2,
			Load = 3,
			Save = 4,
			Evaluate = 5,
			Train = 6,
			//--ack
			Ok = 101,
			Failed = 102
		}

		public enum NetworkKinds : byte
		{
			Policy = 1,
			Value = 2,
			Null = 3
		}

		#region encoder and writer

		public void Encode(Pos pos, out byte code)
		{
			code = (byte)(pos.Y * Board.Width + pos.X);
		}

		public void Encode(Move move, ref byte[] data, ref int index)
		{
			Encode(move.From, out data[index++]);
			Encode(move.To, out data[index++]);
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

		public byte[] GetBytes(Board board)
		{
			byte[] data = new byte[32];
			int index = 0;
			Pos[] positions = board.positions;
			for (int i = 1; i <= 32; i++)
			{
				Encode(positions[i], out data[index++]);
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
				Encode(move, ref data, ref index);
			}

			return data;
		}

		public byte[] GetBytes(Move move)
		{
			byte[] data = new byte[2];
			Encode(move.From, out data[0]);
			Encode(move.To, out data[1]);
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

		void write(Board board)
		{
			write(GetBytes(board));
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

		#endregion

		public bool Connect(string addr, int port)
		{
			try
			{
				client = new TcpClient(addr, port);
				stream = client.GetStream();
				reader = new System.IO.BinaryReader(stream);
				writer = new System.IO.BinaryWriter(stream);
			}
			catch
			{
				return false;
			}

			return true;
		}

		public void Disconnect()
		{
			reader.Close();
			stream.Close();
			client.Close();

			stream = null;
			client = null;
			reader = null;
		}

		public bool CheckConnection()
		{
			byte[] dataCheck = GetBytes(Code.Check, NetworkKinds.Null);
			return readOk();
		}

		public static int FixedLength = 64;
		public bool CreateModel(NetworkKinds kinds, string name)
		{
			write(Code.Create, kinds);
			write(name);
			return readOk();
		}

		public bool LoadModel(NetworkKinds kinds, string name)
		{
			write(Code.Load, kinds);
			write(name);
			return readOk();
		}

		public bool SaveModel(NetworkKinds kinds, string oldName, string newName)
		{
			write(Code.Save, kinds);
			write(oldName);
			write(newName);
			return readOk();
		}

		public float[] EvaluatePolicy(Board board, string name)
		{
			write(Code.Evaluate, NetworkKinds.Policy);
			write(name);
			write(board);
			List<Move> moves = board.GetAllPossibleMoves();
			write(moves);

			if (readOk())
			{
				byte[] arr = readByteArray();
				float[] probs = new float[arr.Length];
				for (int i = 0; i < arr.Length; i++)
				{
					probs[i] = arr[i] / 255.0f;
				}

				return probs;
			}
			else
			{
				return null;
			}
		}

		public float EvaluateValue(Board board, string name)
		{
			write(Code.Evaluate, NetworkKinds.Value);
			write(name);
			write(board);

			if (readOk())
			{
				return reader.ReadByte() / 255.0f;
			}
			else
			{
				return -1;
			}
		}

		public bool TrainPolicy(List<Tuple<Board, Move>> list, string name)
		{
			write(Code.Train, NetworkKinds.Policy);
			write(name);

			byte size = (byte)(list.Count / 255);
			writer.Write(size);
			foreach (var tuple in list)
			{
				write(tuple.Item1);
				write(tuple.Item2);
			}

			return readOk();
		}

		public bool TrainValue(List<Tuple<Board, float>> list, string name)
		{
			write(Code.Train, NetworkKinds.Policy);
			write(name);

			byte size = (byte)(list.Count / 255);
			writer.Write(size);
			foreach (var tuple in list)
			{
				write(tuple.Item1);
				writer.Write((byte)(tuple.Item2 * 255));
			}

			return readOk();
		}


	}
}
