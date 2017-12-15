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
        public int Columns = 6;
        public int Rows = 4;
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
                baseX = FirstColumnPosition;
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

        public void Draw()
        {
            for (int i = 0; i < Rows; i++)
            {
                string row = "";
                for (int j = 0; j < Columns; j++)
                {
                    row += j.ToString() + ":" + i.ToString() + "* " + cells[i, j].LastTurnExplored.ToString() + " |";
                }
                Player.print(row);
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

        public void UnlockCell(Vector2 worldPosition)
        {
            Vector2 gridPosition = WorldToGridPosition(worldPosition);
            cells[(int)gridPosition.Y, (int)gridPosition.X].IsLocked = false;
        }

        public Vector2 GetOldestUnexploredPosition()
        {
            // Search the cell with the lowest LastTurnExplored
            int oldestCellValue = 999;
            Cell oldestCell = null;
            foreach (var cell in cells)
            {
                if(cell.LastTurnExplored < oldestCellValue && !cell.IsLocked)
                {
                    oldestCellValue = cell.LastTurnExplored;
                    oldestCell = cell;
                }
            }

            List<Cell> rndCells = new List<Cell>();
            foreach (var cell in cells)
            {
                if (cell.LastTurnExplored == oldestCellValue && !cell.IsLocked)
                {
                    rndCells.Add(cell);
                }
            }

            rndCells.First().IsLocked = true;
            Vector2 gridPosition = WorldToGridPosition(rndCells.First().Position);

            // TODO : Foreach cells get their position and calculate distance
            
            foreach (var cell in cells)
            {
                cell.Debug();
            }

            return GridToWorldPosition(gridPosition);
        }
        
        public Vector2 GridToWorldPosition(Vector2 gridPosition)
        {
            Vector2 worldPosition = new Vector2();

            worldPosition.Y = FirstColumnPosition + (gridPosition.Y * DistanceBetweenColumns);
            worldPosition.X = FirstRowPosition + (gridPosition.X * DistanceBetweenRows);

            if(worldPosition.X > 16000)
            {
                worldPosition.X = 16000;
            }

            if(worldPosition.Y > 9000)
            {
                worldPosition.Y = 9000;
            }

            return worldPosition;
        }
        
        /// <summary>
        /// Given a world position and the actual turn we check if the buster is on the middle of a cell and update its informations
        /// </summary>
        /// <param name="busterPosition"></param>
        /// <param name="turn"></param>
        public void UpdateMap(Vector2 busterPosition, int turn)
        {
            Vector2 gridPosition = WorldToGridPosition(busterPosition);
            Player.print(gridPosition.ToString());
            Vector2 worldPosition = GridToWorldPosition(gridPosition);
            Player.print(worldPosition.ToString());

            // If the buster is around the center of a cell, update it
            if ((worldPosition.X - 100 < busterPosition.X || busterPosition.X < worldPosition.X + 100) && (worldPosition.Y - 100 < busterPosition.Y || busterPosition.Y < worldPosition.Y + 100))
            {
                cells[(int)gridPosition.Y, (int)gridPosition.X].IsLocked = false;
                cells[(int)gridPosition.Y, (int)gridPosition.X].LastTurnExplored = turn;
            }

            /*
            if (busterPosition == GridToWorldPosition(gridPosition))
            {
                cells[(int)gridPosition.Y, (int)gridPosition.X].IsLocked = false;
                cells[(int)gridPosition.Y, (int)gridPosition.X].LastTurnExplored = turn;
            }
            */
        }
    }
}
