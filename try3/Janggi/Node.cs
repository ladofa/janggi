using Janggi.Game;
using Janggi.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using static Janggi.Game.MovingHelper;

namespace Janggi
{
    public class Node
    {
        static float c_puct = 1.0f;
        static float temp = 1.0f;

        public Board board;
        float side_value; //1 또는 -1 ... 한 플레이어일 경우 q-value를 반대로 적용

        public Node? parent;
        int children_count = 0;
        
        //null이면 아직 자식들 생각 안 해봄
        public List<Node> children = null!;

        float[] w = null!;
        float[] p = null!;
        float[] n = null!;
        //float[] q = null!;  //계산할 때마다 n으로 나누므로 필요없음
        //float[] ucb = null!; //즉석에서 계산하므로 필요없음

        float[] pre_policy = null!;
        int total_n = 0;

        bool finished = false;
        public bool Finished => finished;

        float winner = 0;
        public float Winner => winner;

        public int TotalVisited
        {
            get => total_n;
        }

        

        Node(Board board)
        {
            this.board = board;
            side_value = Stones.SIDE_VALUE(board.player);

            if (board.winner != 0)
            {
                finished = true;
                winner = board.winner;
            }
        }

        public delegate (float[], float[]) EvaluateDelegate(List<Node> children);

        public static EvaluateDelegate Evaluate = null!;

        public static Node CreateRoot(Board board)
        {
            Node node = new Node(board);
            return node;
        }

        /// <summary>
        /// 탐색을 중지하고 끝낸다.
        /// </summary>
        public void RollUp()
        {
            finished = true;
            winner = board.score > 0 ? 1 : -1;
        }

        /// <summary>
        /// 하나의 노드씩 방문하는 게 아니라 펼처지는 자식 노드를 한꺼번에 방문
        /// 
        /// </summary>
        /// <returns></returns>
        public (int, float) Expand()
        {
            if (children_count != 0) return (0, 0);

            board.CalcMoves();

            children_count = board.moves.Count;

            children = new List<Node>();
            List<Move> moves = board.moves;
            for (int i = 0; i < children_count; i++)
            {
                Board next_board = new Board(board, moves[i]);
                Node next_node = new Node(next_board);
                next_node.parent = this;
                children.Add(next_node);
            }

            w = new float[children_count];
            n = new float[children_count];
            p = new float[children_count];

            total_n = children_count;
            //p, w(v) q 계산... 딥러닝 네트워크에 쌓아놓는다.

            List<Board> boards = children.Select(x => x.board).ToList();
            //자식 노드에 대해 딥러닝 pytorch로 돌림
            
            (float[] next_policy, w) = Evaluate(children);
            //w는 그냥 쓰면 된다. 각 자식노드에서 나온 w를 1회 업데이트한다는 개념

            //자식을 한 번씩 방문한 것으로 생각한다.
            Array.Fill(n, 1);

            //policy 아래쪽으로 전파
            for (int i = 0; i < children_count; i++)
            {
                float[] next_pre_policy = new float[total_moving_count];
                Array.Copy(next_policy, i * total_moving_count, next_pre_policy, 0, total_moving_count);
                children[i].pre_policy = next_pre_policy;
            }

            //자기 policy 완성
            //root인 경우
            if (pre_policy == null)
            {
                (pre_policy, _) = Evaluate(new List<Node> { this });
            }

            for (int i = 0; i < children_count; i++)
            {
                p[i] = pre_policy[move2idx[moves[i]]];
                //logit을 받은 다음에 다시 softmax를 할 것인가
                //아니면 그냥 확률 값을 받은 뒤 nomalize 안 하고 냅둘 것인가 (이걸로)
            }

            //softmax
            Softmax(p);


            pre_policy = null!;

            return (total_n, w.Sum());
        }

        public static void Softmax(float[] p)
        {
            float max = p.Max();
            float total = 0;
            for (int i = 0; i < p.Length; i++)
            {
                p[i] = (float)Math.Exp(p[i] - max);
                total += p[i];
            }

            for (int i = 0; i < p.Length; i++)
            {
                p[i] /= total;
            }
        }


        //ucb연산 중에 다른 스레드에 의해 값이 바뀔 수 있음. (무시함)
        internal (int index, Node node) NextNode()
        {
            float max_ucb = float.MinValue;
            int max_index = -1;
            for (int i = 0; i < children_count; i++)
            {
                float ucb = side_value * w[i] / n[i] + c_puct * p[i] * (float)Math.Sqrt(total_n) / (n[i] + 1);
                if (ucb > max_ucb)
                {
                    max_ucb = ucb;
                    max_index = i;
                }
            }

            return (max_index, children[max_index]);
        }

        public (int index, Move move, Node node) BestChoice()
        {
            float max_visited = float.MinValue;
            int max_index = -1;

            for (int i = 0; i < children_count; i++)
            {
                if (n[i] > max_visited)
                {
                    max_visited = n[i];
                    max_index = i;
                }
            }

            

            return (max_index, board.moves[max_index], children[max_index]);
        }


        /// <summary>
        /// 메모리 절약을 위해 children을 없앤다.
        /// </summary>
        public void Clear()
        {
            children = null!;
        }

        public void ClearBut(int i)
        {
            if (parent != null)
            {
                parent.children = new List<Node>() { this };
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i">업데이트할 자식 인덱스/param>
        /// <param name="adding_n">방문 횟수</param>
        /// <param name="adding_w">얻어낸 w 크기</param>
        internal void Update(int i, int adding_n, float adding_w)
        {
            total_n += adding_n;
            n[i] += adding_n;
            w[i] += adding_w;
        }


        static Node()
        {
            Evaluate = PythonComm.Evaluate;
        }
    }
}
