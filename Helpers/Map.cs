using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace CodeBuster
{
    class Map
    {
        int Columns = 6;
        int Rows = 4;
        Cell[,] cells;

        int FirstColumnPosition = 1555;
        int DistanceBetweenColumns = 3111;
        int FirstRowPosition = 1555;
        int DistanceBetweenRows = 3111;

        public Map()
        {
            cells = new Cell[Rows, Columns];

            int baseX = FirstColumnPosition;
            int baseY = DistanceBetweenRows;

            for (int i = 0; i < Rows; i++)
            {
                baseX = DistanceBetweenColumns;
                for (int j = 0; j < Columns; j++)
                {
                    cells[i, j] = new Cell(new Vector2(baseX, baseY));

                    baseX += DistanceBetweenColumns;
                    if(baseX > 16000)
                    {
                        baseX = 16000;
                    }
                }

                baseY += DistanceBetweenRows;
                if(baseY > 9000)
                {
                    baseY = 9000;
                }
            }
        }

        public void Debug()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    cells[i, j].Debug();
                }
            }
        }

        public Vector2 WorldToGridPosition(Vector2 position)
        {
            return new Vector2((float)Math.Floor(position.X / DistanceBetweenColumns), (float)Math.Floor(position.Y / (float)DistanceBetweenRows));
        }

        public void SetCellAge(Vector2 worldPosition, int age)
        {
            Vector2 gridPosition = WorldToGridPosition(worldPosition);
            Player.print("SET CELL AS : " + age.ToString());
            cells[(int)gridPosition.Y, (int)gridPosition.X].LastTurnExplored = age;
        }

        public Vector2 GetOldestUnexploredPosition()
        {
            // Search the cell with the lowest LastTurnExplored
            int oldestCellValue = 999;
            Cell oldestCell = null;
            foreach (var cell in cells)
            {
                if(cell.LastTurnExplored < oldestCellValue)
                {
                    oldestCellValue = cell.LastTurnExplored;
                    oldestCell = cell;
                }
            }

            Vector2 gridPosition = WorldToGridPosition(oldestCell.Position);

            // TODO : Foreach cells get their position and calculate distance
            // Player.print(cells[(int)gridPosition.X, (int)gridPosition.Y].Position.ToString());

            return GridToWorldPosition(gridPosition);
        }

        public Vector2 GridToWorldPosition(Vector2 gridPosition)
        {
            Vector2 worldPosition = new Vector2();

            worldPosition.X = FirstColumnPosition + (gridPosition.Y * DistanceBetweenColumns);
            worldPosition.Y = FirstRowPosition + (gridPosition.X * DistanceBetweenRows);

            return worldPosition;
        }
    }
}
