using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Collections;

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

        // Second attempt at creating a map system
        SortedList cellsToExplore = new SortedList();

        public Map()
        {
            cells = new Cell[Rows, Columns];

            int baseX = FirstColumnPosition;
            int baseY = FirstRowPosition;

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

            // Populate cells to explore with cells given by priority
            // Y, X
            // Level 1 priority
            cellsToExplore.Add(0, new Vector2(1, 2));
            cellsToExplore.Add(1, new Vector2(3, 1));
            cellsToExplore.Add(2, new Vector2(2, 2));
            cellsToExplore.Add(3, new Vector2(2, 3));
            cellsToExplore.Add(4, new Vector2(1, 3));
            cellsToExplore.Add(5, new Vector2(4, 0));
            cellsToExplore.Add(6, new Vector2(5, 0));
            cellsToExplore.Add(7, new Vector2(0, 3));
            // Level 2 priority 
            cellsToExplore.Add(8, new Vector2(2, 0));
            cellsToExplore.Add(9, new Vector2(3, 0));
            cellsToExplore.Add(10, new Vector2(2, 1));
            cellsToExplore.Add(11, new Vector2(4, 1));
            cellsToExplore.Add(12, new Vector2(5, 1));
            cellsToExplore.Add(13, new Vector2(0, 2));
            cellsToExplore.Add(14, new Vector2(3, 2));
            cellsToExplore.Add(15, new Vector2(3, 3));
            // Level 3 priority 
            cellsToExplore.Add(16, new Vector2(1, 0));
            cellsToExplore.Add(17, new Vector2(0, 1));
            cellsToExplore.Add(18, new Vector2(1, 1));
            cellsToExplore.Add(19, new Vector2(4, 2));
            cellsToExplore.Add(20, new Vector2(5, 2));
            cellsToExplore.Add(21, new Vector2(4, 3));
        }

        public void Debug()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    Player.print(j + " : " + i);
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
                    row += j.ToString() + ":" + i.ToString() + " / " + cells[i, j].LastTurnExplored + " / "  + (cells[i, j].IsLocked ? "True  ": "False") + " |";
                }
                Player.print(row);
            }
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

        public void MarkCellAsVisited(Vector2 worldPosition, int age)
        {
            Vector2 gridPosition = WorldToGridPosition(worldPosition);
            cells[(int)gridPosition.Y, (int)gridPosition.X].IsLocked = false;
            cells[(int)gridPosition.Y, (int)gridPosition.X].LastTurnExplored = age;

            // Player.print("Cell " + gridPosition.Y + " " + gridPosition.X + " has been marked as visited on turn " + age.ToString());
        }

        public List<int> GetOldestCellValues()
        {
            List<int> oldestCellValues = new List<int>();

            foreach (var cell in cells)
            {
                if (!cell.IsLocked && !oldestCellValues.Contains(cell.LastTurnExplored))
                {
                    oldestCellValues.Add(cell.LastTurnExplored);
                }
            }

            oldestCellValues.Sort();

            return oldestCellValues;
        }

        public Vector2 GetNextCell()
        {
            Cell nextCell = null;
            // We search a list of old values in ascending order
            foreach (var oldValue in GetOldestCellValues())
            {
                Player.print("LastTurnExplored value : " + oldValue);
                for (int i = 0; i < cellsToExplore.Count; i++)
                {
                    Vector2 cell = (Vector2)cellsToExplore.GetByIndex(i);
                    Cell cellFound = cells[(int)cell.Y, (int)cell.X];

                    // If the actual cell has the same LastTurnExplored value we return it
                    if (cellFound.LastTurnExplored == oldValue && !cellFound.IsLocked)
                    {
                        Player.print("Cell found : " + cellFound.Position);
                        nextCell = cellFound;
                        break;
                    }
                }

                if (nextCell != null)
                {
                    break;
                }
            }

            // Lock cell and return its position
            nextCell.IsLocked = true;
            return nextCell.Position;
        }

        public Vector2 WorldToGridPosition(Vector2 position)
        {
            return new Vector2((float)Math.Floor(((float)FirstRowPosition + position.X) / DistanceBetweenColumns), (float)Math.Floor(((float) FirstColumnPosition + position.Y) / (float)DistanceBetweenRows));
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
        /// Given a world position and the actual turn we check if the buster is around the middle of a cell and update its informations
        /// </summary>
        /// <param name="busterPosition"></param>
        /// <param name="turn"></param>
        public void UpdateMap(Vector2 busterPosition, int turn)
        {
            Vector2 gridPosition = WorldToGridPosition(busterPosition);
            Vector2 worldPosition = GridToWorldPosition(gridPosition);

            // If the buster is around the center of a cell, update it
            int aroundValue = 150;
            if ((worldPosition.X - aroundValue < busterPosition.X || busterPosition.X < worldPosition.X + aroundValue) && (worldPosition.Y - aroundValue < busterPosition.Y || busterPosition.Y < worldPosition.Y + aroundValue))
            {
                MarkCellAsVisited(worldPosition, turn);
            }
        }
    }
}
