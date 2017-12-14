
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;


namespace CodeBuster
{
    class Buster
    {
        public Vector2 Position { get; set; }
        public Vector2 GhostPosition { get; set; }
        public Vector2 EnemyPosition { get; set; }
        public Vector2 TargetPosition { get; set; }
        public Vector2 BasePosition { get; set; }
        public int EntityId { get; }
        public bool IsInDropZone { get; set; }
        public Ghost GhostCaptured { get; set; }
        public Ghost GhostInRange { get; set; }
        public BusterState State { get; set; }
        public BusterState LastState { get; set; }
        public int EnemyInRange { get; set; }
        public int LastTurnStun { get; set; }
        public bool CanStun { get; set; }

        public Buster(int entityId, Vector2 initialPosition, Vector2 basePosition)
        {
            Position = initialPosition;
            EntityId = entityId;

            // Initialize values
            IsInDropZone = false;
            GhostCaptured = null;
            BasePosition = basePosition;
            GhostInRange = null;
            EnemyInRange = -1;

            // Initialize MoveToPosition
            TargetPosition = initialPosition;

            // Initialize default state
            State = BusterState.MoveState;
        }

        public void ComputeInformations()
        {
            Player.print(State.ToString());
            State.ComputeInformations(this);

            if(State != LastState)
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
            if(EnemyInRange != -1 && CanStun)
            {
                return true;
            }

            return false;
        }

        public void Debug()
        {
            Player.print("Buster " + EntityId + " / position : " + Position.ToString() + " / target : " + TargetPosition.ToString() + " / can capture : " + CanCapture().ToString() + " / is holding : " + IsHoldingAGhost().ToString() + " / can release : " + CanRelease().ToString() + " / can attack : " + CanAttack().ToString() + " / last turn stun : " + LastTurnStun.ToString());
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
            if(State == BusterState.MoveState && !CanCapture())
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
        public bool Targeted { get; set; }

        public Enemy(Vector2 initialPosition, int entityId) : base(initialPosition, entityId)
        {
            RemainingStunTurns = -1;
            IsCarryingAGhost = false;
            IsCapturing = false;
        }

        public new void Debug()
        {
            base.Debug();
            Player.print("Remaining stun : " + RemainingStunTurns + " / Capturing : " + IsCapturing + " / Targeted : " + Targeted);
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
        public bool Captured { get; set; }

        public Ghost(Vector2 initialPosition, int entityId) : base(initialPosition, entityId)
        {
        }

        public new void Debug()
        {
            base.Debug();
            Player.print("Captured : " + Captured);
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
            Player.print(Position.ToString() + " | LastTurnExplored : " + LastTurnExplored.ToString());
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
                    oldestCell.Debug();
                }
            }

            oldestCell.IsLocked = true;
            Vector2 gridPosition = WorldToGridPosition(oldestCell.Position);

            // TODO : Foreach cells get their position and calculate distance

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
    }
}


namespace CodeBuster
{
    // TODO : Give a Role class and the busters will get informations
    // based on their roles
    class Brain
    {
        public bool TeamInitialized { get; set; }
        public int TeamId { get; set; }
        public List<Buster> Busters ;
        public List<Enemy> Enemies;
        public List<Ghost> Ghosts;
        public Vector2 BasePosition;
        public Map GridMap;
        public int Turn { get; set; }

        int x = 0;
        int y = 0;

        public Brain(int numberOfBusters, int numberOfGhosts, int teamId)
        {
            TeamInitialized = false;
            Busters = new List<Buster>();
            Ghosts = new List<Ghost>();
            Enemies = new List<Enemy>();
            TeamId = teamId;

            // Initialize map
            GridMap = new Map();

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
            // TODO : Remove base position from the Buster constructor
            Busters.Add(new Buster(entityId, position, BasePosition));
        }

        public void AddGhost(int entityId, Vector2 position)
        {
            Ghosts.Add(new Ghost(position, entityId));
        }

        public Ghost GetGhost(int entityId)
        {
            return Ghosts.Find(e => e.EntityId == entityId);
        }

        public void CreateOrUpdateGhost(int entityId, Vector2 position, bool isVisible)
        {
            Player.print("CREATE OR UPDATE GHOST : " + entityId.ToString());
            Ghost ghost = GetGhost(entityId);
            if (ghost == null)
            {
                AddGhost(entityId, position);
            }
            else
            {
                ghost.Position = position;
                ghost.IsVisible = isVisible;
                ghost.Captured = false;
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

        /// <summary> UpdateBusterInformations give informations to the buster according to its role
        /// entityId : id of the buster
        /// value : Ghost id being carried, number of turns before stun goes away
        /// state : 0=idle, 1=carrying a ghost, 2=stuned buster
        /// TODO : Each turn give infos to the busters about the strategy chosen by the multi-agent system
        /// </summary>
        public void UpdateBusterInformations(int entityId, Vector2 position, int value, int state)
        {
            // Find the buster
            Buster buster = Busters.Find(e => e.EntityId == entityId);
            
            // We got stunned
            if(state == 2)
            {
                // If we were carrying a ghost and we drop it outside of the base we mark it as catchable
                if(!buster.IsInDropZone && buster.GhostCaptured != null)
                {
                    buster.GhostCaptured.Captured = false;
                }

                // Reset capture variables
                buster.GhostCaptured = null;
                buster.GhostInRange = null;
            }

            // Update its ghost captured value
            if (state == 1)
            {
                Ghost ghost = Ghosts.Find(e => e.EntityId == value);
                // Mark this ghost as captured so we can't capture it again
                buster.GhostCaptured = ghost;
                try
                {
                    Ghosts.Find(e => e.EntityId == value).Captured = true;
                }
                catch
                {
                    Player.print("ERROR NULL REF GHOST");
                    // TODO : handle error case
                    buster.GhostCaptured = null;
                    buster.GhostInRange = null;
                }
            }
            else
            {
                buster.GhostCaptured = null;
                buster.GhostInRange = null;
            }

            // Update its position
            buster.Position = position;
        }

        /// <summary>
        /// Method called at each new turn to refresh following informations :
        /// - We set each ghost as non-visible and update their visibility when we get the information about what our busters can see
        /// </summary>
        public void ResetTurnInformations()
        {
            foreach (Ghost ghost in Ghosts)
            {
                ghost.IsVisible = false;
            }

            foreach (Buster buster in Busters)
            {
                // If our last state was Stun and that we've stuned an enemy
                if(!buster.CanStun && buster.LastState == BusterState.StunState)
                {
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

                buster.EnemyInRange = -1;
            }

            foreach (Enemy enemy in Enemies)
            {
                enemy.IsVisible = false;
                enemy.Targeted = false;
            }

            Turn++;
        }

        /// <summary>
        /// Compute the distance between each ghosts and each busters
        /// </summary>
        /// <returns></returns>
        public List<Tuple<int, int, int>> ComputeDistancesBetweenEachBusterAndGhosts()
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
        /// Given all the game informations we update our Busters if last intels
        /// </summary>
        public void ComputeInformations()
        {
            List<Tuple<int, int, int>> busterToGhost = ComputeDistancesBetweenEachBusterAndGhosts();

            /// TODO : need to move this into giveinformations
            /// // TODO : Opti - If two buster have the same ghost change to number 2 for one of them
            /*
            Vector2 pos = new Vector2(0, 0);
            if (Busters[0].Position == Busters[0].TargetPosition)
            {

                pos = GridMap.GridToWorldPosition(new Vector2(x, y));

                // Debug map variables
                x++;
                if (x >= GridMap.Columns)
                {
                    x = 0;
                    y++;
                    if (y >= GridMap.Rows)
                    {
                        y = 0;
                    }
                }
            }
            */

            for (int i = 0; i < Busters.Count; i++)
            {
                Player.print("Buster : " + i.ToString());

                /*
                if(pos != new Vector2(0, 0))
                {
                    Busters[i].TargetPosition = pos;
                }
                */
                
                // Check if this buster is not busy
                if (!Busters[i].IsBusy())
                {
                    // Get the closest ghost
                    int lowest = 9999;
                    Ghost ghost = null;
                    foreach (var item in busterToGhost.FindAll(e => e.Item1 == Busters[i].EntityId))
                    {
                        if (item.Item3 < lowest)
                        {
                            ghost = Ghosts.Find(e => e.EntityId  == item.Item2);
                            lowest = item.Item3;
                            Player.print("ONE GHOST FOUND IN AREA");
                        }
                    }
                    Busters[i].GhostInRange = ghost;

                    // If no ghost can be captured we want to chase the known ghosts
                    if (ghost == null)
                    {
                        Vector2 nextPos = new Vector2(0, 0);
                        Ghost targetGhost = null;
                        float smallest = 999999;
                        foreach (Ghost freeGhost in Ghosts.FindAll(e => e.Captured == false))
                        {
                            float dist = Vector2.Distance(Busters[i].Position, freeGhost.Position);
                            if (dist < smallest)
                            {
                                nextPos = freeGhost.Position;
                                smallest = dist;
                                targetGhost = freeGhost;
                            }
                        }

                        // TODO : Now that our enemies capture a lot of ghosts we must stop derping on a known location, just go and if there is no ghost ask the map for a cell to discover

                        if (nextPos != new Vector2(0, 0))
                        {
                            Player.print("CHASING AN OLD GHOST : " + targetGhost.EntityId.ToString());
                            Busters[i].TargetPosition = nextPos;
                        }
                    }
                    else
                    {
                        Busters[i].MarkGhostAsCaptured();
                    }
                }

                // Check for each buster if they are in base range
                if (Vector2.Distance(Busters[i].Position, BasePosition) <= 1600)
                {
                    Player.print("Is in drop zone");
                    Busters[i].IsInDropZone = true;
                }
                else
                {
                    Busters[i].IsInDropZone = false;
                }

                // Check for each enemy if we are in range to stun one
                float lowestDistance = 99999f;
                int closestEntity = -1;
                foreach (var enemy in Enemies.FindAll(e => e.IsVisible == true && e.RemainingStunTurns == -1 && e.Targeted == false))
                {
                    float distanceToEnemy = Vector2.Distance(Busters[i].Position, enemy.Position);
                    if (distanceToEnemy <= 1760)
                    {
                        if(distanceToEnemy < lowestDistance)
                        {
                            lowestDistance = distanceToEnemy;
                            closestEntity = enemy.EntityId;
                            enemy.Targeted = true;

                            Player.print("GOING TO ATTACK");
                        }
                    }
                }
                Busters[i].EnemyInRange = closestEntity;

                // If an enemy is visible and he's carrying a ghost : ATTACK HIM !
                lowestDistance = 99999f;
                foreach (var enemy in Enemies.FindAll(e => e.IsVisible == true && e.IsCarryingAGhost == true))
                {
                    float distanceToEnemy = Vector2.Distance(Busters[i].Position, enemy.Position);
                    if (distanceToEnemy < lowestDistance)
                    {
                        lowestDistance = distanceToEnemy;
                        Busters[i].TargetPosition = enemy.Position;
                        Player.print("CHASING AN ENEMY : " + enemy.EntityId.ToString());
                    }
                }

                // If we are at the wanted position find a new cell to explore
                if (Busters[i].Position == Busters[i].TargetPosition && Busters[i].GhostInRange == null)
                {
                    Player.print("SCOUTING");
                    GridMap.UnlockCell(Busters[i].Position);
                    GridMap.SetCellAge(Busters[i].Position, Turn);
                    Busters[i].TargetPosition = GetNextPosition();
                }
            }
            // GridMap.Debug();
        }

        public Vector2 GetNextPosition()
        {
            Player.print("X : " + x.ToString() + " - Y : " + y.ToString());
            // Vector2 nextPosition = GridMap.GridToWorldPosition(new Vector2(x, y));
            Vector2 nextPosition = GridMap.GetOldestUnexploredPosition();
            return nextPosition;
        }

        public void GiveOrders()
        {
            Player.print("-------------------------");
            Debug();

            foreach (var buster in Busters)
            {
                buster.Debug();
                buster.ComputeInformations();

                Console.WriteLine(buster.ComputeNextOrder());
            }

            GridMap.Draw();
        }

        public void Debug()
        {
            Player.print("-- ENEMIES --");
            foreach (var enemy in Enemies)
            {
                enemy.Debug();
            }

            Player.print("-- GHOSTS --");
            foreach (var ghost in Ghosts)
            {
                ghost.Debug();
            }

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
                    int state = int.Parse(inputs[4]); // For busters: 0=idle, 1=carrying a ghost, 2=stuned buster.
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
                        brain.CreateOrUpdateGhost(entityId, new Vector2(x, y), true);
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
        }

        public override string Update(Buster buster)
        {
            if(buster.IsHoldingAGhost())
            {
                // TODO : From actual position get the vector to the base and calculate the point that is in radius of the base (1600)
                buster.TargetPosition = buster.BasePosition;
            }

            // Go to the target position
            return "MOVE " + buster.TargetPosition.X + " " + buster.TargetPosition.Y;
        }

        public override void ComputeInformations(Buster buster)
        {
            // If we were running to the base with a ghost and that we can now drop it
            if(buster.CanRelease())
            {
                buster.State = BusterState.ReleaseState;
            }

            // If we've moved to a ghost and we're in range to capture it
            if(buster.CanCapture())
            {
                buster.State = BusterState.CaptureState;
            }

            // If we're just scouting and we can attack an enemy
            if (buster.CanAttack() && buster.GhostCaptured == null)
            {
                buster.State = BusterState.StunState;
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

        }

        public override string Update(Buster buster)
        {
            return "RELEASE";
        }

        public override void ComputeInformations(Buster buster)
        {
            // If we've drop our ghost and a ghost is in range
            if (!buster.IsHoldingAGhost() && buster.CanCapture())
            {
                buster.State = BusterState.CaptureState;
            }

            // If we've drop our ghost we can now move
            if (!buster.IsHoldingAGhost())
            {
                buster.State = BusterState.MoveState;
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
            // STUN id
            buster.CanStun = false;
            return "STUN " + buster.EnemyInRange.ToString();
        }

        public override void ComputeInformations(Buster buster)
        {
            // If we've just attack an enemy and we've no more enemy to stun go to scout again
            if (!buster.CanAttack())
            {
                buster.State = BusterState.MoveState;
            }
        }
    }
}

