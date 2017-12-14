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
        // This attribute contain the number of ghost we've seen in this cell
        // by using the symmetry rule of the game we can predict where the next ghosts are
        public int BaseNumberOfGhosts { get; set; }

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
