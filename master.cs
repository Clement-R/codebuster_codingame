
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
        public Vector2 TargetPosition { get; set; }
        public Vector2 BasePosition { get; set; }
        public int EntityId { get; }
        public bool IsInDropZone { get; set; }
        public bool GhostCaptured { get; set; }
        public int GhostInRange { get; set; }
        public BusterState State { get; set; }
        public BusterState LastState { get; set; }
        public int EnemyInRange { get; set; }

        public Buster(int entityId, Vector2 initialPosition, Vector2 basePosition)
        {
            Position = initialPosition;
            EntityId = entityId;

            // Initialize values
            IsInDropZone = false;
            GhostCaptured = false;
            BasePosition = basePosition;
            GhostInRange = -1;
            EnemyInRange = -1;

            // Initialize MoveToPosition
            TargetPosition = initialPosition;

            // Initialize default state
            State = BusterState.MoveState;
        }

        public void ComputeInformations()
        {
            // TODO : Check if in drop zone
            // TODO : Check if a ghost is in range
            // TODO : Check if ghost captured
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
            if (GhostCaptured && IsInDropZone)
            {
                return true;
            }

            return false;
        }

        public bool CanCapture()
        {
            if (!GhostCaptured && GhostInRange != -1)
            {
                return true;
            }

            return false;
        }

        public bool IsHoldingAGhost()
        {
            return GhostCaptured;
        }

        public bool CanAttack()
        {
            if(EnemyInRange == -1)
            {
                return false;
            }

            return true;
        }

        public void Debug()
        {
            Player.print("Buster " + EntityId + " : " + "Can capture : " + CanCapture().ToString() + " / is holding : " + IsHoldingAGhost().ToString() + " / can release : " + CanRelease().ToString() + " / can attack : " + CanAttack().ToString());
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

        public Enemy(Vector2 initialPosition, int entityId) : base(initialPosition, entityId)
        {
            RemainingStunTurns = -1;
            IsCarryingAGhost = false;
            IsCapturing = false;
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
    }
}


namespace CodeBuster
{
    class Cell
    {
        Vector2 Position { get; set; }
        int LastTurnExplored { get; set; }

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

        public void GetClosestUnexploredCell()
        {
            // TODO : To implement
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
            Ghost ghost = GetGhost(entityId);
            if (ghost == null)
            {
                AddGhost(entityId, position);
            }
            else
            {
                ghost.Position = position;
                ghost.IsVisible = isVisible;
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
        /// capturedGhost : id of the captured ghost, else -1
        /// TODO : Each turn give infos to the busters about the strategy chosen by the multi-agent system
        /// </summary>
        public void UpdateBusterInformations(int entityId, Vector2 position, int capturedGhost)
        {
            // Find the buster
            Buster buster = Busters.Find(e => e.EntityId == entityId);

            // Update its ghost captured value
            if (capturedGhost != -1)
            {
                buster.GhostCaptured = true;
                try
                {
                    Ghosts.Find(e => e.EntityId == capturedGhost).Captured = true;
                }
                catch
                {
                    Player.print("ERROR NULL REF GHOST");
                }
                
                // TODO : Mark this ghost as captured so we can't capture it again
            }
            else
            {
                buster.GhostCaptured = false;
                buster.GhostInRange = -1;
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
            foreach (Entity ghost in Ghosts)
            {
                ghost.IsVisible = false;
            }
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
            for (int i = 0; i < Busters.Count; i++)
            {
                // Check if this buster is not busy // TODO : This should not be handled by the MAS, this look like agent responsibility
                if (Busters[i].State == BusterState.MoveState && !Busters[i].CanCapture())
                {
                    int lowest = 9999;
                    int ghostId = -1;
                    // Get the closest ghost
                    foreach (var item in busterToGhost.FindAll(e => e.Item1 == Busters[i].EntityId))
                    {
                        if (item.Item3 < lowest)
                        {
                            ghostId = item.Item2;
                            lowest = item.Item3;
                        }
                    }
                    Busters[i].GhostInRange = ghostId;

                    // If no ghost can be captured we want to chase the known ghosts
                    if (ghostId == -1)
                    {
                        Vector2 nextPos = new Vector2(0, 0);

                        float smallest = 999999;
                        foreach (Entity ghost in Ghosts.FindAll(e => e.Captured == false))
                        {
                            float dist = Vector2.Distance(Busters[i].Position, ghost.Position);
                            if (dist < smallest)
                            {
                                nextPos = ghost.Position;
                                smallest = dist;
                            }
                        }

                        if (nextPos != new Vector2(0, 0))
                        {
                            Busters[i].TargetPosition = nextPos;
                        }
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
                if (Vector2.Distance(Busters[i].Position, BasePosition) <= 1760)
                {
                    Player.print("Is in drop zone");
                    Busters[i].IsInDropZone = true;
                }
                else
                {
                    Busters[i].IsInDropZone = false;
                }
            }
        }

        public void GiveOrders()
        {
            foreach (var buster in Busters)
            {
                buster.Debug();
                buster.ComputeInformations();

                Console.WriteLine(buster.ComputeNextOrder());
            }
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
                        brain.UpdateBusterInformations(entityId, new Vector2(x, y), value);
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
            return "BUST " + buster.GhostInRange.ToString();
        }

        public override void ComputeInformations(Buster buster)
        {
            // If we were capturing and the ghost flew away, or we capture a ghost but we're not in range to drop it
            if(!buster.CanCapture() || (buster.GhostCaptured && !buster.IsHoldingAGhost()))
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
            if(rng == null)
            {
                rng = new System.Random();
            }
        }

        public override void Enter(Buster buster)
        {
            // TODO : Change this value
            buster.TargetPosition = new Vector2(8000, 4500);
        }

        public override string Update(Buster buster)
        {
            if(buster.IsHoldingAGhost())
            {
                // TODO : From actual position get the vector to the base and calculate the point that is in radius of the base (1600)
                buster.TargetPosition = buster.BasePosition;
            }
            
            // TODO : Remove the random movement !
            if(buster.Position == buster.TargetPosition)
            {
                buster.TargetPosition = new Vector2(buster.Position.X + rng.Next(-8000, 8000), buster.Position.X + rng.Next(-3000, 3000));
                if (buster.TargetPosition.X <= 0)
                {
                    buster.TargetPosition = new Vector2(0, buster.TargetPosition.Y);
                }
                if (buster.TargetPosition.X >= 16000)
                {
                    buster.TargetPosition = new Vector2(16000, buster.TargetPosition.Y);
                }

                if (buster.TargetPosition.Y <= 0)
                {
                    buster.TargetPosition = new Vector2(buster.TargetPosition.X, 0);
                }
                if (buster.TargetPosition.Y >= 9000)
                {
                    buster.TargetPosition = new Vector2(buster.TargetPosition.X, 9000);
                }
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
                Player.print(buster.EntityId + " is going to capture");
                buster.State = BusterState.CaptureState;
            }

            // If we're just scouting and we can attack an enemy
            if (buster.CanAttack() && !buster.GhostCaptured)
            {
                Player.print("WAHOU");
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

