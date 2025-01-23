using Janggi.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Janggi
{
    public class Mcts
    {
        public Node root = null!;


        //초기 셋팅되어야 할 것이
        public void Init(Board board)
        {
            root = Node.CreateRoot(board);
            root.Expand();
        }

        public void SearchOneNode()
        {
            //하나의 노드를 방문하는 것 같지만
            //리프 노드로부터 이어지는 전체 자식 노드를 한번에 방문
            List<Node> visited = new List<Node>();
            List<int> visited_index = new List<int>();
            Node node = root;
            int adding_n = 1;
            float adding_w = 0;
            while (true)
            {
                Node? nextNode = null;
                lock (node)
                {
                    //게임이 끝났는지 보고
                    if (node.Finished)
                    {
                        //roll up
                        adding_n = 1;
                        adding_w = node.Winner;
                        break;
                    }
                    
                    //최대 수순이면 게임 끝내기.
                    if (node.board.turn >= 100)
                    {
                        node.RollUp();
                        adding_n = 1;
                        adding_w = node.Winner;
                        break;
                    }
                    //최대 깊이까지 들어왔다면
                    //else if (visited.Count >= max_visited_depth)
                    //{
                    //    //roll up
                    //}
                    //expend를 안 했으면(처음 방문했다면)
                    else if (node.children == null)
                    {
                        (adding_n, adding_w) = node.Expand();
                        //node(리프노드)는 update가 끝났음.
                        break;
                    }
                
                
                    visited.Add(node);
                    (int index, nextNode) = node.NextNode();
                    visited_index.Add(index);
                }

                if (nextNode != null)
                {
                    node = nextNode;
                }


                //expend되어있으면 다음 노드로
            }

            //부모 노드에 대한 업데이트
            for (int lev = 0; lev < visited.Count; lev++)
            {
                lock (visited[lev])
                {
                    visited[lev].Update(visited_index[lev], adding_n, adding_w);
                }
            }

            
        }


        bool isSearching = false;
        Task[] tasks = new Task[0];

        public bool IsSearching => isSearching;

        public delegate void ProgressUpdatedHandler(Mcts mcts, int visit, double rate);
        public event ProgressUpdatedHandler? ProgressUpdated;


        public void StartSearchNodes(int maxSearch = 400, int n=0)
        {
            if (n == 0)
            {
                //ThreadPool.GetMinThreads(out int minWorkerThreads, out int minCompletionPortThreads);
                //n = minWorkerThreads * 4;
                n = 8;
            }

            isSearching = true;
            // 실행할 람다 함수
            Action lambdaFunction = () =>
            {
                while (isSearching)
                {
                    //Console.WriteLine(root.TotalVisited);
                    if (root.TotalVisited >= maxSearch)
                        break;
                    SearchOneNode();
                    ProgressUpdated?.Invoke(this, root.TotalVisited, root.TotalVisited / maxSearch);
                }
            };

            // N개의 Task 생성 및 실행
            tasks = new Task[n];
            for (int i = 0; i < n; i++)
            {
                tasks[i] = Task.Run(lambdaFunction);
            }
        }

        public async Task AsyncWaitSearchNodes()
        {
            await Task.WhenAll(tasks);
        }

        public void WaitSearchNodes()
        {
            foreach (var task in tasks)
            {
                task.Wait();
            }
        }

        public void StopAndWaitSearchNodes()
        {
            isSearching = false;
            foreach (var task in tasks)
            {
                task.Wait();
            }
        }

        public void StopSearchNodes()
        {
            isSearching = false;
        }

        public void Step(int i)
        {
            Node next = root.children[i];

            root.ClearBut(i);

            root = next;
            root.Expand();
        }

        public void Step(Move move)
        {
            int index = root.board.moves.IndexOf(move);
            if (index == -1)
            {
                throw new Exception("BAD MOVE");
            }
            Step(index);
        }
    }
}
