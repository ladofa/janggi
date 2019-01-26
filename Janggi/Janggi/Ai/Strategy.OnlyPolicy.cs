using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Janggi.StoneHelper;

namespace Janggi.Ai
{
	public class OnlyPolicy : Mcts.Strategy
	{
		TensorFlow.TcpCommClient client = new TensorFlow.TcpCommClient();
		string networkName;
		public OnlyPolicy(TensorFlow.TcpCommClient client = null, string networkName = "policy192")
		{
			this.networkName = networkName;
			if (client == null)
			{
				while (!this.client.Connect("localhost", 9999))
				{
					Console.WriteLine("ConnectionFailed.");
					System.Threading.Thread.Sleep(1000);
				}
			}
			else
			{
				this.client = client;
			}
		}

		public int MaxRolloutDepth
		{
			set;
			get;
		}

		public double ExplorationRate
		{
			set;
			get;
		}

		public float Alpha
		{
			set;
			get;
		}

		

		public override void CalcPolicyWeights(Node node)
		{
			node.PreparePolicyWeights();

			if (node.board.IsMyTurn)
			{
				node.policyWeights = client.EvaluatePolicy(node.board, node.moves);
			}
			else
			{
				Board opBoard = node.board.GetOpposite();
				
				List<Move> opMoves = Move.GetOpposite(node.moves);
				node.policyWeights = client.EvaluatePolicy(opBoard, opMoves);
			}
			float[] proms = node.policyWeights;

			float total = 0;

			for (int i = 0; i < proms.Length; i++)
			{
				total += proms[i];
			}

			if (total == 0)
			{
				proms[Global.Rand.Next(proms.Length)] = 1;
			}
			else
			{
				//이런거 안 해도 된다.
				//for (int i = 0; i < proms.Length; i++)
				//{
				//	proms[i] /= total;
				//}
			}
			
		}

		public override float[] CalcScores(Node node)
		{
			//그냥 policyWeights 그대로 리턴해도 된다.
			return node.policyWeights;
		}

		public override float CalcLeafEvaluation(Node node)
		{
			//rollout
			return 1;
		}
	}
}
