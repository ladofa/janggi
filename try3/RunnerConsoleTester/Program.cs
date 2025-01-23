using Janggi;
using Janggi.Net;
using Janggi.Game;
using System.Runtime.InteropServices;

Mcts mcts = new Mcts();
Node.Evaluate = PythonComm.Evaluate;

List<(Node, Move)> history = new List<(Node, Move)>();

mcts.Init(Board.RandomBoard());

for (int i = 0; i < 10; i++)
{
    mcts.SearchOneNode();
}

while (true)
{
    mcts.root.board.PrintStones();
    Console.WriteLine();
    if (mcts.root.Finished)
    {
        break;
    }

    mcts.StartSearchNodes(12, 4000);
    mcts.WaitSearchNodes();
    (int nextIndex, Move nextMove, _) = mcts.root.BestChoice();
    history.Add((mcts.root, nextMove));
    mcts.Step(nextIndex);
}