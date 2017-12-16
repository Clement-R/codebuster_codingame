
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;


namespace CodeBuster
{
    class Buster : Entity
    {
        public Vector2 GhostPosition { get; set; }
        public Vector2 EnemyPosition { get; set; }
        public Vector2 TargetPosition { get; set; }
        public Vector2 BasePosition { get; set; }
        public bool IsInDropZone { get; set; }
        // The ghost that I'm carrying
        public Ghost GhostCaptured { get; set; }
        // A ghost that I can capture
        public Ghost GhostInRange { get; set; }
        // The ghost I'm chasing
        public Ghost GhostChased { get; set; }
        public BusterState State { get; set; }
        public BusterState LastState { get; set; }
        public Enemy EnemyChased { get; set; }
        public Enemy EnemyInRange { get; set; }
        public int LastTurnStun { get; set; }
        public bool CanStun { get; set; }
        public bool IsScouting { get; set; }
        public bool IsStunned { get; set; }
        public bool GotAnOrder { get; set; }

        public Buster(int entityId, Vector2 initialPosition, Vector2 basePosition) : base(initialPosition, entityId)
        {
            // Initialize values
            IsInDropZone = false;
            IsScouting = false;
            GhostCaptured = null;
            BasePosition = basePosition;
            GhostInRange = null;
            EnemyInRange = null;
            EnemyChased = null;
            GhostChased = null;
            CanStun = true;
            IsStunned = false;
            GotAnOrder = false;

            // Initialize MoveToPosition
            TargetPosition = initialPosition;

            // Initialize default state
            State = BusterState.MoveState;
        }

        public void ComputeInformations()
        {
            State.ComputeInformations(this);

            Player.print(State.ToString());

            if (State != LastState)
            {
                State.Enter(this);
            }

            LastState = State;
        }

        public string ComputeNextOrder()
        {
            return State.Update(this);
        }

        public bool CanRelease()
        {
            if (GhostCaptured != null && IsInDropZone)
            {
                return true;
            }

            return false;
        }

        public bool CanCapture()
        {
            if (GhostCaptured == null && GhostInRange != null)
            {
                return true;
            }

            return false;
        }

        public bool IsHoldingAGhost()
        {
            if(GhostCaptured == null)
            {
                return false;
            }

            return true;
        }

        public bool CanAttack()
        {
            if(EnemyInRange != null && CanStun)
            {
                return true;
            }

            return false;
        }

        public new void Debug()
        {
            Player.print("Buster " + EntityId + " / position : " + Position.ToString() + " / target : " + TargetPosition.ToString() + " / can capture : " + CanCapture().ToString() + " / is holding : " + IsHoldingAGhost().ToString() + " / can release : " + CanRelease().ToString() + " / can attack : " + CanAttack().ToString() + " / can stun : " + CanStun + " / last turn stun : " + LastTurnStun.ToString() + " / ghost chased : " + ((GhostChased != null) ? GhostChased.EntityId : -1) + " / ghost in range : " + ((GhostInRange != null) ? GhostInRange.EntityId : -1) + " / enemy chased : " + ((EnemyChased != null) ? EnemyChased.EntityId : -1) + " / enemy in range : " + ((EnemyInRange != null) ? EnemyInRange.EntityId : -1) + " / stunned : " + IsStunned + " / busy : " + IsBusy());
        }

        public void MarkGhostAsCaptured()
        {
            if(GhostInRange != null)
            {
                GhostInRange.Captured = true;
            }
        }

        public bool IsBusy()
        {
            if(!IsHoldingAGhost() && GhostInRange == null && !IsStunned)
            {
                return false;
            }

            return true;
        }
    }
}

namespace CodeBuster
{
    class Enemy : Entity
    {
        public int RemainingStunTurns { get; set; }
        public bool IsCarryingAGhost { get; set; }
        public bool IsCapturing { get; set; }
        public bool Locked { get; set; }

        public Enemy(Vector2 initialPosition, int entityId) : base(initialPosition, entityId)
        {
            RemainingStunTurns = -1;
            IsCarryingAGhost = false;
            IsCapturing = false;
        }

        public new void Debug()
        {
            base.Debug();
            Player.print("Remaining stun : " + RemainingStunTurns + " / Capturing : " + IsCapturing + " / Targeted : " + Locked + " / Is carrying : " + IsCarryingAGhost);
        }
    }
}


namespace CodeBuster
{
    class Entity
    {
        public Vector2 Position { get; set; }
        public int EntityId { get; }
        public bool IsVisible { get; set; }

        public Entity(Vector2 initialPosition, int entityId)
        {
            this.Position = initialPosition;
            this.EntityId = entityId;

            this.IsVisible = true;
        }

        public void Debug()
        {
            Player.print("Position : " + Position.ToString() + " / Id : " + EntityId.ToString() + " / Visible : " + IsVisible);
        }
    }
}

namespace CodeBuster
{
    class Ghost : Entity
    {
        public bool Locked { get; set; }
        public bool Captured { get; set; }
        public bool KnownLocation { get; set; }
        public int Life { get; set; }
        public int NumberOfBustersCapturing { get; set; }

        public Ghost(Vector2 initialPosition, int entityId, int life, int numberOfBusterCapturing) : base(initialPosition, entityId)
        {
            KnownLocation = true;
            Locked = false;
            Life = life;
            NumberOfBustersCapturing = numberOfBusterCapturing;
        }

        public new void Debug()
        {
            base.Debug();
            Player.print("Captured : " + Captured + " / Locked : " + Locked);
        }
    }
}


namespace CodeBuster
{
    class Cell
    {
        public Vector2 Position { get; set; }
        public int LastTurnExplored { get; set; }
        // This attribute contain the number of ghost we've seen in this cell
        public int BaseNumberOfGhosts { get; set; }
        public bool IsLocked;

        public Cell(Vector2 position)
        {
            Position = position;
            LastTurnExplored = -1;
            IsLocked = false;
        }

        public void Debug()
        {
            Player.print(Position + " | LastTurnExplored : " + LastTurnExplored.ToString() + " | Locked : " + IsLocked);
        }
    }
}


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
            cellsToExplore.Add(0, new Vector2(2, 2));
            cellsToExplore.Add(1, new Vector2(3, 1));
            cellsToExplore.Add(2, new Vector2(1, 3));
            cellsToExplore.Add(3, new Vector2(4, 0));
            cellsToExplore.Add(4, new Vector2(5, 0));
            cellsToExplore.Add(5, new Vector2(0, 3));
            // Level 2 priority 
            cellsToExplore.Add(6, new Vector2(2, 0));
            cellsToExplore.Add(7, new Vector2(3, 0));
            cellsToExplore.Add(8, new Vector2(2, 1));
            cellsToExplore.Add(9, new Vector2(4, 1));
            cellsToExplore.Add(10, new Vector2(5, 1));
            cellsToExplore.Add(11, new Vector2(0, 2));
            cellsToExplore.Add(12, new Vector2(1, 2));
            cellsToExplore.Add(13, new Vector2(3, 2));
            cellsToExplore.Add(14, new Vector2(2, 3));
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

            Player.print("Cell " + gridPosition.Y + " " + gridPosition.X + " has been marked as visited on turn " + age.ToString());
        }

        public int GetOldestCellValue()
        {
            int minLastTurnExplored = 999;
            foreach (var cell in cells)
            {
                if(cell.LastTurnExplored < minLastTurnExplored)
                {
                    minLastTurnExplored = cell.LastTurnExplored; 
                }
            }

            return minLastTurnExplored;
        }

        public Vector2 GetNextCell()
        {
            Cell nextCell = null;
            // We search the cell that has the lowest LastTurnExplored value
            int minLastTurnExplored = GetOldestCellValue();
            Player.print("Minimum LastTurnExplored : " + minLastTurnExplored);
            for (int i = 0; i < cellsToExplore.Count; i++)
            {
                Vector2 cell = (Vector2) cellsToExplore.GetByIndex(i);
                Cell cellFound = cells[(int)cell.Y, (int)cell.X];
                // If the actual cell has the same LastTurnExplored value we return it
                if (cellFound.LastTurnExplored == minLastTurnExplored && !cellFound.IsLocked)
                {
                    Player.print("Cell found : " + cellFound.Position);
                    nextCell = cellFound;
                    break;
                }
            }

            // Lock cell and return its position
            nextCell.IsLocked = true;
            return nextCell.Position;
        }

        public Vector2 WorldToGridPosition(Vector2 position)
        {
            return new Vector2((float)Math.Floor(position.X / DistanceBetweenColumns), (float)Math.Floor(position.Y / (float)DistanceBetweenRows));
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


namespace CodeBuster
{
    class Brain
    {
        public bool TeamInitialized { get; set; }
        public int TeamId { get; set; }
        public List<Buster> Busters ;
        public List<Enemy> Enemies;
        public List<Ghost> Ghosts;
        public List<int> CapturedGhost;
        public Vector2 BasePosition;
        public Map GridMap;
        public int Turn { get; set; }

        public Brain(int numberOfBusters, int numberOfGhosts, int teamId)
        {
            TeamInitialized = false;
            Busters = new List<Buster>();
            Ghosts = new List<Ghost>();
            Enemies = new List<Enemy>();
            CapturedGhost = new List<int>();
            TeamId = teamId;

            // Initialize map
            GridMap = new Map();
            GridMap.Debug();

            // Initialize game infos
            if (TeamId == 0)
            {
                BasePosition = new Vector2(0, 0);
            }
            else
            {
                BasePosition = new Vector2(16000, 9000);
            }
        }

        public void AddBuster(int entityId, Vector2 position)
        {
            Busters.Add(new Buster(entityId, position, BasePosition));
        }

        public Ghost GetGhost(int entityId)
        {
            return Ghosts.Find(e => e.EntityId == entityId);
        }

        public void CreateOrUpdateGhost(int entityId, Vector2 position, bool isVisible, int life, int numberOfBusterCapturing)
        {
            Ghost ghost = GetGhost(entityId);
            
            if (ghost == null)
            {
                Player.print("Create ghost : " + entityId);
                Ghosts.Add(new Ghost(position, entityId, life, numberOfBusterCapturing));
                Ghosts.Last().Debug();
            }
            else
            {
                Player.print("Update ghost : " + entityId);
                ghost.Position = position;
                ghost.IsVisible = isVisible;
                ghost.KnownLocation = isVisible;
                ghost.Captured = false;
                ghost.Life = life;
                ghost.Debug();
            }
        }

        public void AddEnemy(int entityId, Vector2 position)
        {
            Enemies.Add(new Enemy(position, entityId));
        }

        public Enemy GetEnemy(int entityId)
        {
            return Enemies.Find(e => e.EntityId == entityId);
        }

        public void CreateOrUpdateEnemy(int entityId, Vector2 position, bool isVisible, int state, int value)
        {
            // Compute informations
            int remainingStunTurns = -1;
            bool isCarryingAGhost = false;
            bool isCapturing = false;

            // State: 0=idle, 1=carrying a ghost, 2=stuned buster
            // Value: Ghost id being carried, number of turns before stun goes away
            switch (state)
            {
                case 1:
                    // carrying
                    isCarryingAGhost = true;
                    break;
                case 2:
                    // stun
                    remainingStunTurns = value;
                    break;
                case 3:
                    // capturing
                    isCapturing = true;
                    break;
            }
            
            // Search the enemy
            Enemy enemy = GetEnemy(entityId);
            if (enemy == null)
            {
                // Create the enemy and get the created object
                AddEnemy(entityId, position);
                enemy = GetEnemy(entityId);
                // Update its informations
                enemy.RemainingStunTurns = remainingStunTurns;
                enemy.IsCarryingAGhost = isCarryingAGhost;
                enemy.IsCapturing = isCapturing;
            }
            else
            {
                // Update its informations
                enemy.Position = position;
                enemy.IsVisible = isVisible;
                enemy.RemainingStunTurns = remainingStunTurns;
                enemy.IsCarryingAGhost = isCarryingAGhost;
                enemy.IsCapturing = isCapturing;
            }
        }

        /// <summary>
        /// Update the buser with the turn's informations
        /// </summary>
        /// <param name="entityId">id of the buster</param>
        /// <param name="position"></param>
        /// <param name="value">Ghost id being carried, number of turns before stun goes away</param>
        /// <param name="state">0=idle, 1=carrying a ghost, 2=stuned buster</param>
        public void UpdateBusterInformations(int entityId, Vector2 position, int value, int state)
        {
            // Find the buster
            Buster buster = Busters.Find(e => e.EntityId == entityId);
            
            switch(state)
            {
                case 0:
                    Player.print("Buster : " + entityId.ToString() + " / Idle");
                    break;

                case 1:
                    Player.print("Buster : " + entityId.ToString() + " / Carrying");
                    // Search the ghost
                    Ghost ghost = Ghosts.Find(e => e.EntityId == value);
                    // Set the captured ghost for the current buster
                    buster.GhostCaptured = ghost;
                    // Set the ghost as captured
                    buster.GhostCaptured.Captured = true;
                    break;

                case 2:
                    Player.print("Buster : " + entityId.ToString() + " / Stunned");

                    // We got stunned, release our ghost if needed
                    // If we were carrying a ghost and we drop it outside of the base we mark it as catchable
                    if (!buster.IsInDropZone && buster.GhostCaptured != null)
                    {
                        buster.GhostCaptured.Captured = false;
                    }
                    // Reset capture and scout variables
                    buster.GhostCaptured = null;
                    buster.GhostChased = null;
                    buster.EnemyChased = null;
                    buster.TargetPosition = buster.Position;
                    StopScouting(buster);

                    buster.IsStunned = true;
                    
                    break;
            }

            // Update its position
            buster.Position = position;
            Player.print("Buster is at map position : " + GridMap.WorldToGridPosition(buster.Position));

            // Check for buster if they are in base range
            if (Vector2.Distance(buster.Position, BasePosition) <= 1600)
            {
                Player.print("Buster " + buster.EntityId.ToString() + " is in drop zone");
                buster.IsInDropZone = true;
            }
            else
            {
                buster.IsInDropZone = false;
            }

            // Set target position when we've to release
            if(buster.LastState == BusterState.ReleaseState)
            {
                buster.TargetPosition = buster.Position;
            }

            // Update map with the position of the buster
            GridMap.UpdateMap(buster.Position, Turn);
        }

        /// <summary>
        /// Method called at each new turn to refresh informations that we get each turn
        /// </summary>
        public void ResetTurnInformations()
        {
            // Set all ghost to not visible and we will set them to visible during the turn
            foreach (Ghost ghost in Ghosts)
            {
                ghost.IsVisible = false;
                ghost.Locked = false;
            }

            foreach (Buster buster in Busters)
            {
                // Update stun variables
                buster.IsStunned = false;
                if (!buster.CanStun && buster.LastState == BusterState.StunState)
                {
                    // If our last state was Stun and that we've stuned an enemy
                    buster.LastTurnStun = Turn;
                }
                else if(!buster.CanStun)
                {
                    // If last stun was 20 turns before, we can now stun
                    if(Turn >= buster.LastTurnStun + 20)
                    {
                        buster.CanStun = true;
                    }
                }

                // TODO : remove dirty fix
                foreach (var ghost in Ghosts)
                {
                    if (ghost.Position == buster.Position)
                    {
                        ghost.KnownLocation = false;
                    }
                }
                
                // We will set these values later when we will get all game informations
                buster.GhostInRange = null;
                buster.EnemyInRange = null;
                buster.GhostCaptured = null;
                buster.GotAnOrder = false;
            }

            foreach (Enemy enemy in Enemies)
            {
                enemy.IsVisible = false;
                enemy.Locked = false;
            }

            Turn++;
        }

        /// <summary>
        /// Compute the distance between each ghosts and each busters and keep only the ghost that can be captured
        /// </summary>
        /// <returns></returns>
        public List<Tuple<int, int, int>> GetEachGhostInZoneForBusters()
        {
            // buster id, ghost id, distance between them
            List<Tuple<int, int, int>> busterToGhost = new List<Tuple<int, int, int>>();
            // Foreach known ghost get distance to each buster if in range of capture
            foreach (Ghost ghost in Ghosts.FindAll(e => e.IsVisible == true && e.Captured == false))
            {
                for (int i = 0; i < Busters.Count; i++)
                {
                    int distanceToGhost = (int)Vector2.Distance(Busters[i].Position, ghost.Position);

                    // Check if we can capture a ghost
                    if (distanceToGhost > 900 && distanceToGhost < 1760)
                    {
                        busterToGhost.Add(new Tuple<int, int, int>(Busters[i].EntityId, ghost.EntityId, distanceToGhost));
                    }
                }
            }

            return busterToGhost;
        }

        /// <summary>
        /// Compute the distance between each ghosts and each busters
        /// </summary>
        /// <returns></returns>
        public List<Tuple<int, int, int>> GetDistanceBetweenBustersAndGosts()
        {
            // buster id, ghost id, distance between them
            List<Tuple<int, int, int>> distanceBetweenBusterAndGhosts = new List<Tuple<int, int, int>>();

            // Foreach known ghost get distance to each buster if in range of capture
            foreach (Ghost ghost in Ghosts)
            {
                // If the ghost is not already in base
                if(!CapturedGhost.Contains(ghost.EntityId))
                {
                    for (int i = 0; i < Busters.Count; i++)
                    {
                        int distanceToGhost = (int)Vector2.Distance(Busters[i].Position, ghost.Position);
                        distanceBetweenBusterAndGhosts.Add(new Tuple<int, int, int>(Busters[i].EntityId, ghost.EntityId, distanceToGhost));
                    }
                }
            }

            return distanceBetweenBusterAndGhosts;
        }

        public List<Tuple<int, int>> GetDistanceToBusters(Entity entity)
        {
            List<Tuple<int, int>> distances = new List<Tuple<int, int>>();
            foreach (var buster in Busters)
            {
                int distanceToBuster = (int)Vector2.Distance(buster.Position, entity.Position);
                distances.Add(new Tuple<int, int>(buster.EntityId, distanceToBuster));
            }

            distances = distances.OrderBy(e => e.Item2).ToList();

            return distances;
        }

        public List<Tuple<int, int>> GetCapturableGhosts(Buster buster)
        {
            List<Tuple<int, int>> capturableGhosts = new List<Tuple<int, int>>();
            foreach (var ghost in Ghosts.FindAll(e => e.IsVisible == true))
            {
                if (!CapturedGhost.Contains(ghost.EntityId))
                {
                    int distanceToGhost = (int)Vector2.Distance(buster.Position, ghost.Position);
                    if(distanceToGhost > 900 && distanceToGhost < 1760)
                    {
                        capturableGhosts.Add(new Tuple<int, int>(ghost.EntityId, distanceToGhost));
                    }
                }
            }

            return capturableGhosts;
        }

        public List<Tuple<int, int>> GetCloseRangeGhosts(Buster buster)
        {
            List<Tuple<int, int>> ghostsInCloseRange = new List<Tuple<int, int>>();
            foreach (var ghost in Ghosts.FindAll(e => e.IsVisible == true))
            {
                if (!CapturedGhost.Contains(ghost.EntityId))
                {
                    int distanceToGhost = (int)Vector2.Distance(buster.Position, ghost.Position);
                    if (distanceToGhost < 900)
                    {
                        ghostsInCloseRange.Add(new Tuple<int, int>(ghost.EntityId, distanceToGhost));
                    }
                }
            }

            return ghostsInCloseRange;
        }

        public List<Tuple<int, int>> GetChasableGhosts(Buster buster)
        {
            List<Tuple<int, int>> chasableGhosts = new List<Tuple<int, int>>();
            foreach (var ghost in Ghosts.FindAll(e => e.Locked == false && e.IsVisible == true))
            {
                // If this ghost is not already captured
                if (!CapturedGhost.Contains(ghost.EntityId))
                {
                    int distanceToGhost = (int)Vector2.Distance(buster.Position, ghost.Position);
                    chasableGhosts.Add(new Tuple<int, int>(ghost.EntityId, distanceToGhost));
                }
            }

            chasableGhosts = chasableGhosts.OrderBy(e => e.Item2).ToList();

            return chasableGhosts;
        }

        public List<Tuple<int, int>> GetAllVisibleEnemiesCarrying(Buster buster)
        {
            List<Tuple<int, int>> attackableEnemies = new List<Tuple<int, int>>();
            foreach (var enemy in Enemies.FindAll(e => e.IsCarryingAGhost == true && e.IsVisible == true && e.Locked == false))
            {
                int distanceToEnemy = (int)Vector2.Distance(buster.Position, enemy.Position);
                attackableEnemies.Add(new Tuple<int, int>(enemy.EntityId, distanceToEnemy));
            }

            return attackableEnemies;
        }

        public List<Tuple<int, int>> GetAttackableEnemies(Buster buster)
        {
            List<Tuple<int, int>> attackableEnemies = new List<Tuple<int, int>>();
            foreach (var enemy in Enemies.FindAll(e => e.Locked == false && e.IsVisible == true))
            {
                // If this ghost is not already captured
                int distanceToEnemy = (int)Vector2.Distance(buster.Position, enemy.Position);
                if (distanceToEnemy < 1760)
                {
                    attackableEnemies.Add(new Tuple<int, int>(enemy.EntityId, distanceToEnemy));
                }
            }

            return attackableEnemies;
        }

        /// <summary>
        /// Given all the game informations we update our Busters with last intels
        /// </summary>
        public void ComputeInformations()
        {
            // First we check if our buster is already doing a task 
            foreach (var buster in Busters)
            {
                if(buster.IsHoldingAGhost() || buster.IsStunned)
                {
                    buster.GotAnOrder = true;
                }
            }

            // Search a catchable ghost for each buster
            foreach (var buster in Busters.FindAll(e => e.GotAnOrder == false))
            {
                Player.print("Buster " + buster.EntityId + " search to capture");
                
                // First action : Try to capture a ghost
                // We retrieve all ghosts in capture range
                List<Tuple<int, int>> ghostsInRange = GetCapturableGhosts(buster);
                foreach (var ghost in ghostsInRange)
                {
                    // Retrieve the actual ghost
                    Ghost foundGhost = Ghosts.Find(e => e.EntityId == ghost.Item1);

                    // Do we already have found a ghost to capture ? If not we search
                    if (buster.GhostInRange == null)
                    {
                        // We get the distance between the ghost and all busters
                        List<Tuple<int, int>> bustersByDistance = GetDistanceToBusters(foundGhost);
                        foreach (var busterByDistance in bustersByDistance)
                        {
                            int closestBusterId = busterByDistance.Item1;

                            if (closestBusterId == buster.EntityId)
                            {
                                // If we're the closest, we capture it
                                buster.GhostInRange = foundGhost;
                                buster.GotAnOrder = true;
                                break;
                            }
                            else
                            {
                                // If we're not the closest, does the other buster is busy ?
                                Buster otherBuster = Busters.Find(e => e.EntityId == closestBusterId);
                                if (otherBuster.IsBusy())
                                {
                                    buster.GhostInRange = foundGhost;
                                    buster.GotAnOrder = true;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                // If no ghost is easy to capture we want to check if we're not just too close from one
                if (buster.GhostInRange == null)
                {
                    // TODO : Get all ghosts in too close distance ( < 600 ) and move in the opposite direction to the vector between the buster and the ghost to capture it at the next turn
                    List<Tuple<int, int>> closeRangeGhosts = GetCloseRangeGhosts(buster);
                    foreach (var ghost in closeRangeGhosts)
                    {
                        // Retrieve the actual ghost
                        Ghost foundGhost = Ghosts.Find(e => e.EntityId == ghost.Item1);

                        if(!foundGhost.Locked)
                        {
                            buster.GotAnOrder = true;
                        }
                        // DO THINGS PLEASE
                        // 
                        //
                        //
                        //
                    }
                }
            }

            // Second action if no capturable ghost : Chase an enemy or attack an enemy in range (if we can stun)
            foreach (var buster in Busters.FindAll(e => e.GotAnOrder == false && e.CanStun == true))
            {
                Player.print("Buster " + buster.EntityId + " search to chase");
                // Find distance to all visible enemies carrying a ghost
                List<Tuple<int, int>> enemiesInRange = GetAllVisibleEnemiesCarrying(buster);
                foreach (var enemy in enemiesInRange)
                {
                    // Retrieve the actual enemy
                    Enemy foundEnemy = Enemies.Find(e => e.EntityId == enemy.Item1);

                    // Do we already have found an enemy to chase ? If not we search
                    if (buster.EnemyChased == null)
                    {
                        // We get the distance between the enemy and all busters
                        List<Tuple<int, int>> bustersByDistance = GetDistanceToBusters(foundEnemy);
                        foreach (var busterByDistance in bustersByDistance)
                        {
                            int closestBusterId = busterByDistance.Item1;

                            if (closestBusterId == buster.EntityId)
                            {
                                // If we're the closest, we chase them
                                buster.EnemyChased = foundEnemy;
                                buster.EnemyChased.Locked = true;
                                buster.GotAnOrder = true;
                                break;
                            }
                            else
                            {
                                // If we're not the closest, does the other buster is busy ?
                                Buster otherBuster = Busters.Find(e => e.EntityId == closestBusterId);
                                if (otherBuster.IsBusy())
                                {
                                    buster.EnemyChased = foundEnemy;
                                    buster.EnemyChased.Locked = true;
                                    buster.GotAnOrder = true;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                    
                // Third action if no enemy with a ghost : Attack a close enemy
                if (buster.EnemyChased == null)
                {
                    Player.print("Buster " + buster.EntityId + " search an enemy to attack");
                    // Find all enemies in range
                    enemiesInRange = GetAttackableEnemies(buster);
                    foreach (var enemy in enemiesInRange)
                    {
                        // Retrieve the actual enemy
                        Enemy foundEnemy = Enemies.Find(e => e.EntityId == enemy.Item1);
                        // Get the distance between this enemy and all busters
                        List<Tuple<int, int>> bustersByDistance = GetDistanceToBusters(foundEnemy);
                        foreach (var busterByDistance in bustersByDistance)
                        {
                            int closestBusterId = busterByDistance.Item1;

                            if (closestBusterId == buster.EntityId)
                            {
                                // If we're the closest, we chase them
                                buster.EnemyInRange = foundEnemy;
                                buster.EnemyInRange.Locked = true;
                                buster.GotAnOrder = true;
                                break;
                            }
                            else
                            {
                                // If we're not the closest, does the other buster is busy ?
                                Buster otherBuster = Busters.Find(e => e.EntityId == closestBusterId);
                                if (otherBuster.IsBusy())
                                {
                                    buster.EnemyInRange = foundEnemy;
                                    buster.EnemyInRange.Locked = true;
                                    buster.GotAnOrder = true;
                                    break;
                                }
                                else
                                {
                                    // The closest non-busy buster will chase this enemy
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // If we are in range to attack our chased enemy
                    int distanceToEnemy = (int)Vector2.Distance(buster.Position, buster.EnemyChased.Position);
                    if (distanceToEnemy < 1760)
                    {
                        buster.EnemyInRange = buster.EnemyChased;
                        buster.GotAnOrder = true;
                    }
                }
            }

            // If no enemy in range: Get a ghost to chase
            foreach (var buster in Busters.FindAll(e => e.GotAnOrder == false))
            {
                Player.print("Buster " + buster.EntityId + " search to chase a ghost");
                // Fourth action if no enemy in range : Chase a ghost
                if (buster.EnemyChased == null && buster.EnemyInRange == null)
                {
                    // Search a ghost to chase
                    List<Tuple<int, int>> chasableGhosts = GetChasableGhosts(buster);
                    foreach (var ghost in chasableGhosts)
                    {
                        // Retrieve the actual ghost
                        Ghost foundGhost = Ghosts.Find(e => e.EntityId == ghost.Item1);

                        // Do we already have found a ghost to chase ? If not we search
                        if (buster.GhostChased == null)
                        {
                            // We get the distance between the enemy and all busters
                            List<Tuple<int, int>> bustersByDistance = GetDistanceToBusters(foundGhost);
                            foreach (var busterByDistance in bustersByDistance)
                            {
                                Player.print("Buster " + busterByDistance.Item1 + " - distance : " + busterByDistance.Item2 + " - busy : " + Busters.Find(e => e.EntityId == busterByDistance.Item1).IsBusy());
                                int closestBusterId = busterByDistance.Item1;

                                if (buster.EntityId == closestBusterId)
                                {
                                    // If we're the closest, we chase them
                                    buster.GhostChased = foundGhost;
                                    buster.GhostChased.Locked = true;
                                    buster.GotAnOrder = true;
                                    break;
                                }
                                else
                                {
                                    // If we're not the closest, does the other buster is busy ?
                                    Buster otherBuster = Busters.Find(e => e.EntityId == closestBusterId);
                                    if (otherBuster.IsBusy())
                                    {
                                        buster.GhostChased = foundGhost;
                                        buster.GhostChased.Locked = true;
                                        buster.GotAnOrder = true;
                                        break;
                                    }
                                    else
                                    {
                                        // The closest non-busy buster will chase this ghost
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // The buster use all the informations given by the brain to take a decision
            foreach (var buster in Busters)
            {
                buster.ComputeInformations();

                // if the buster was chasing a ghost and he now stop moving we remove it's captured ghost reference
                if(buster.GhostChased != null && buster.State != BusterState.MoveState)
                {
                    Player.print("Buster " + buster.EntityId + " : " + buster.State.ToString());
                    buster.GhostChased = null;
                }

                // if the buster was chasing a ghost and he now stop moving we remove it's chased enemy reference
                if (buster.EnemyChased != null && buster.State != BusterState.MoveState)
                {
                    buster.EnemyChased = null;
                }
            }

            // With all the informations gathered we can set the TargetPosition value of our busters according to their choosed action
            foreach (var buster in Busters.FindAll(e => e.IsStunned == false))
            {
                // If the buster is holding a ghost we tell them to go to the base
                if(buster.IsHoldingAGhost())
                {
                    buster.TargetPosition = BasePosition;
                }

                // If the buster is moving but not chasing something and not going to release a ghost, then he's actually scouting
                if (buster.EnemyChased == null && buster.GhostChased == null && buster.State == BusterState.MoveState && !buster.IsHoldingAGhost())
                {
                    Player.print("Buster " + buster.EntityId + " is scouting " + GridMap.WorldToGridPosition(buster.TargetPosition).ToString());
                    buster.IsScouting = true;

                    // I'm scouting, and I'm at my target position, I need a new cell to explore
                    if (buster.TargetPosition == buster.Position)
                    {
                        buster.TargetPosition = GetNextPosition(buster);
                        Player.print("Buster " + buster.EntityId + " is taking a new scout target " + GridMap.WorldToGridPosition(buster.TargetPosition).ToString());
                    }
                }
                else
                {
                    
                    if (buster.IsScouting)
                    {
                        // If I was scouting and I now have a new order, I cancel my scout order
                        Player.print("Buster " + buster.EntityId + " stop scouting");
                        StopScouting(buster);
                    }
                    else if(buster.EnemyChased != null)
                    {
                        Player.print("Buster " + buster.EntityId + " continue chasing enemy " + buster.EnemyChased.EntityId);
                        // If I'm chasing an enemy and he's not visible anymore or there is nothing at his position, we cancel and scout
                        if (buster.TargetPosition == buster.EnemyChased.Position || !buster.EnemyChased.IsVisible)
                        {
                            buster.EnemyChased = null;
                            buster.IsScouting = true;
                            Player.print("Buster " + buster.EntityId + " is taking a new scout target");
                            buster.TargetPosition = GetNextPosition(buster);
                        }
                    }
                    else if(buster.GhostChased != null)
                    {
                        Player.print("Buster " + buster.EntityId + " continue chasing ghost " + buster.GhostChased.EntityId);
                        // If I'm chasing a ghost and there is nothing at his position, we cancel and scout
                        if (buster.Position == buster.GhostChased.Position)
                        {
                            buster.GhostChased = null;
                            buster.IsScouting = true;
                            Player.print("Buster " + buster.EntityId + " is taking a new scout target");
                            buster.TargetPosition = GetNextPosition(buster);
                        }
                    }
                }
            }

            // With all the informations we can get the target position that the buster must follow
            foreach (var buster in Busters.FindAll(e => e.IsStunned == false))
            {
                if (buster.GhostChased != null)
                {
                    buster.TargetPosition = buster.GhostChased.Position;
                }

                if (buster.EnemyChased != null)
                {
                    buster.TargetPosition = buster.EnemyChased.Position;
                }

                if (buster.IsHoldingAGhost() && !buster.IsInDropZone)
                {
                    buster.TargetPosition = BasePosition;
                }


                if (buster.LastState == BusterState.ReleaseState)
                {
                    CapturedGhost.Add(buster.GhostCaptured.EntityId);
                }
            }
        }

        public void StopScouting(Buster buster)
        {
            buster.IsScouting = false;
            GridMap.UnlockCell(buster.TargetPosition);
        }

        public Vector2 GetNextPosition(Buster buster)
        {
            GridMap.UnlockCell(buster.TargetPosition);
            
            // Vector2 nextPosition = GridMap.GetOldestUnexploredPosition();
            Vector2 nextPosition = GridMap.GetNextCell();

            return nextPosition;
        }

        public void GiveOrders()
        {
            Player.print("-------------------------");
            Player.print("GIVING ORDERS");
            Player.print("-------------------------");
            foreach (var buster in Busters)
            {
                buster.Debug();
            }
            Debug();
            GridMap.Draw();
            Player.print("-------------------------");

            foreach (var buster in Busters)
            {
                Console.WriteLine(buster.ComputeNextOrder());
            }
        }

        public void Debug()
        {
            Player.print("-- ENEMIES --");
            foreach (var enemy in Enemies)
            {
                enemy.Debug();
            }
            Player.print("-- ************** --");

            Player.print("-- GHOSTS --");
            foreach (var ghost in Ghosts)
            {
                if(!CapturedGhost.Contains(ghost.EntityId))
                {
                    ghost.Debug();
                }
            }
            Player.print("-- ************** --");

            Player.print("-- CAPTURED GHOSTS --");
            foreach (var ghost in Ghosts)
            {
                if(CapturedGhost.Contains(ghost.EntityId))
                {
                    ghost.Debug();
                }
            }
            Player.print("-- ************** --");

            Player.print("Turn : " + Turn.ToString());
        }
    }
}


namespace CodeBuster
{
    class Player
    {
        static void Main(string[] args)
        {
            int bustersPerPlayer = int.Parse(Console.ReadLine()); // the amount of busters you control
            int ghostCount = int.Parse(Console.ReadLine()); // the amount of ghosts on the map
            int myTeamId = int.Parse(Console.ReadLine()); // if this is 0, your base is on the top left of the map, if it is one, on the bottom right
            
            // Initialize multi-agent system
            Brain brain = new Brain(bustersPerPlayer, ghostCount, myTeamId);

            // Initialize FSM
            InitializeFSM();

            // Game loop
            while (true)
            {
                int entities = int.Parse(Console.ReadLine());

                // Reset ghost in range
                brain.ResetTurnInformations();

                for (int i = 0; i < entities; i++)
                {
                    string[] inputs = Console.ReadLine().Split(' ');
                    int entityId = int.Parse(inputs[0]); // buster id or ghost id
                    int x = int.Parse(inputs[1]);
                    int y = int.Parse(inputs[2]); // position of this buster / ghost
                    int entityType = int.Parse(inputs[3]); // the team id if it is a buster, -1 if it is a ghost.
                    int state = int.Parse(inputs[4]); // For busters: 0=idle, 1=carrying a ghost, 2=stuned buster, 3=buster capturing. For ghosts : life.
                    int value = int.Parse(inputs[5]); // For busters: Ghost id being carried, number of turns before stun goes away. For ghosts: number of busters attempting to trap this ghost.

                    // If this is the first turn, we initialize our Busters with their position, id and the base position
                    if(!brain.TeamInitialized)
                    {
                        if (entityType == brain.TeamId)
                        {
                            brain.AddBuster(entityId, new Vector2(x, y));
                        }
                    }
                    
                    if (entityType == -1)
                    {
                        // If the current entity is a ghost
                        brain.CreateOrUpdateGhost(entityId, new Vector2(x, y), true, state, value);
                    }
                    else if (entityType == brain.TeamId)
                    {
                        // Update busters informations
                        brain.UpdateBusterInformations(entityId, new Vector2(x, y), value, state);
                    }
                    else
                    {
                        // It's an enemy
                        brain.CreateOrUpdateEnemy(entityId, new Vector2(x, y), true, state, value);
                    }
                }

                if (!brain.TeamInitialized)
                {
                    brain.TeamInitialized = true;
                }

                brain.ComputeInformations();
                brain.GiveOrders();
            }
        }

        public static void print(string message)
        {
            Console.Error.WriteLine(message);
        }

        public static void InitializeFSM()
        {
            // Initialize FSM
            // TODO : Introspection ?
            if (BusterState.MoveState == null)
            {
                BusterState.MoveState = new MoveState();
            }

            if (BusterState.CaptureState == null)
            {
                BusterState.CaptureState = new CaptureState();
            }

            if (BusterState.ReleaseState == null)
            {
                BusterState.ReleaseState = new ReleaseState();
            }

            if (BusterState.StunState == null)
            {
                BusterState.StunState = new StunState();
            }
        }
    }
}
namespace CodeBuster
{
    class BusterState
    {
        public static MoveState MoveState { get; set; }
        public static CaptureState CaptureState { get; set; }
        public static ReleaseState ReleaseState { get; set; }
        public static StunState StunState { get; set; }

        public virtual string Update(Buster buster) { return ""; }
        public virtual void ComputeInformations(Buster buster) { }
        public virtual void Enter(Buster buster) { }
    }
}

namespace CodeBuster
{
    class CaptureState : BusterState
    {
        public CaptureState()
        {
        }

        public override void Enter(Buster buster)
        {
        }

        public override string Update(Buster buster)
        {
            // BUST id
            return "BUST " + buster.GhostInRange.EntityId.ToString();
        }

        public override void ComputeInformations(Buster buster)
        {
            // If we were capturing and the ghost flew away, or we capture a ghost but we're not in range to drop it
            if(!buster.CanCapture() || (buster.GhostCaptured != null && !buster.IsHoldingAGhost()))
            {
                buster.State = BusterState.MoveState;
            }

            // If we were capturing and we're already in range for drop
            if (buster.CanRelease())
            {
                buster.State = BusterState.ReleaseState;
            }
        }
    }
}


namespace CodeBuster
{
    class MoveState : BusterState
    {
        public static System.Random rng = null;

        public MoveState()
        {
        }

        public override void Enter(Buster buster)
        {
            if (buster.IsHoldingAGhost())
            {
                // TODO : From actual position get the vector to the base and calculate the point that is in radius of the base (1600)
                buster.TargetPosition = buster.BasePosition;
            }
        }

        public override string Update(Buster buster)
        {
            // Go to the target position
            return "MOVE " + buster.TargetPosition.X + " " + buster.TargetPosition.Y;
        }

        public override void ComputeInformations(Buster buster)
        {
            // If we're just scouting and we can attack an enemy
            if (buster.CanAttack() && buster.GhostCaptured == null)
            {
                buster.State = BusterState.StunState;
            }

            // If we've moved to a ghost and we're in range to capture it
            if (buster.CanCapture())
            {
                buster.State = BusterState.CaptureState;
            }

            // If we were running to the base with a ghost and that we can now drop it
            if (buster.CanRelease())
            {
                buster.State = BusterState.ReleaseState;
            }
        }
    }
}
 
namespace CodeBuster
{
    class ReleaseState : BusterState
    {
        public ReleaseState()
        {
        }

        public override void Enter(Buster buster)
        {
            buster.TargetPosition = buster.Position;
        }

        public override string Update(Buster buster)
        {
            return "RELEASE";
        }

        public override void ComputeInformations(Buster buster)
        {
            // If we've drop our ghost we can now move
            if (!buster.IsHoldingAGhost())
            {
                buster.State = BusterState.MoveState;
            }

            // If we've drop our ghost and a ghost is in range
            if (!buster.IsHoldingAGhost() && buster.CanCapture())
            {
                buster.State = BusterState.CaptureState;
            }
        }
    }
}

namespace CodeBuster
{
    class StunState : BusterState
    {
        public StunState()
        {
        }

        public override void Enter(Buster buster)
        {

        }

        public override string Update(Buster buster)
        {
            buster.CanStun = false;
            buster.TargetPosition = buster.Position;
            // STUN id
            return "STUN " + buster.EnemyInRange.EntityId.ToString();
        }

        public override void ComputeInformations(Buster buster)
        {
            // If we've just attacked an enemy go to scout again
            if (!buster.CanAttack())
            {
                buster.State = BusterState.MoveState;
            }
        }
    }
}

