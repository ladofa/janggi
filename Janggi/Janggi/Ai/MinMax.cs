using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janggi.Ai
{
	public class MinMax
	{
		TensorFlow.TcpCommClient client = new TensorFlow.TcpCommClient();
		string valueNetName;

		public MinMax(TensorFlow.TcpCommClient client = null, string name = "value192")
		{
			valueNetName = name;

			if (client != null)
			{
				this.client = client;
			}
			else
			{
				while (!this.client.Connect("localhost", 9999))
				{
					Console.WriteLine("ConnectionFailed.");
					System.Threading.Thread.Sleep(1000);
				}

				this.client.LoadModel(TensorFlow.NetworkKinds.Value, valueNetName, valueNetName);
			}
		}

		int myFirst;//나부터 시작했으면 0

		const int maxLevel = 2;

		(float eval , int index) GetScore(Board board, int level)
		{
			List<Move> moves = board.GetAllPossibleMoves();
			//제자리 멈춤은 뺀다.
			moves.RemoveAt(moves.Count - 1);

			float[] evals = new float[moves.Count];
			Board[] nextBoards = new Board[moves.Count];

			for (int i = 0; i < evals.Length; i++)
			{
				Board next = board.GetNext(moves[i]);
				nextBoards[i] = next;
			}

			//리프노드만 
			if (level == maxLevel)
			{
				if (board.IsMyTurn)
				{
					for (int i = 0; i < evals.Length; i++)
					{
						float eval = client.EvaluateValue(board, valueNetName);
						evals[i] = eval;
					}
				}
				else
				{
					for (int i = 0; i < evals.Length; i++)
					{
						//상대방이 이길 확률.
						float eval = client.EvaluateValue(board.GetOpposite(), valueNetName);
						evals[i] = 1 - eval;
					}
				}
			}
			else
			{
				for (int i = 0; i < evals.Length; i++)
				{
					var rest = GetScore(nextBoards[i], level + 1);
					evals[i] = rest.eval;
				}
			}

			int bestIndex = 0;
			float bestEval = 0;

			if (board.IsMyTurn)
			{
				bestEval = evals[0];
				bestIndex = 0;
				for (int i = 0; i < evals.Length; i++)
				{
					if (evals[i] > bestEval)
					{
						evals[i] = bestEval;
						bestIndex = i;
					}
				}
			}
			else
			{
				bestEval = evals[0];
				bestIndex = 0;
				for (int i = 0; i < evals.Length; i++)
				{
					if (evals[i] < bestEval)
					{
						evals[i] = bestEval;
						bestIndex = i;
					}
				}
			}


			return (bestEval, bestIndex);
		}

		public Move SearchNext(Board board)
		{
			var ret = GetScore(board, 1);
			List<Move> moves = board.GetAllPossibleMoves();
			return moves[ret.index];			
		}
	}
}
