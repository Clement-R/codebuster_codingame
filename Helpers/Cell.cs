using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace CodeBuster
{
    class Cell
    {
        public Vector2 Position { get; set; }
        public int LastTurnExplored { get; set; }

        public Cell(Vector2 position)
        {
            Position = position;
            LastTurnExplored = -1;
        }

        public void Debug()
        {
            Player.print(Position.ToString() + " | LastTurnExplored : " + LastTurnExplored.ToString());
        }
    }
}
