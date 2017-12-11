
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

        public Buster(Vector2 initialPosition, int entityId, Vector2 basePosition)
        {
            Position = initialPosition;
            EntityId = entityId;

            // Initialize values
            IsInDropZone = false;
            GhostCaptured = false;
            BasePosition = basePosition;
            GhostInRange = -1;

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

        public void Debug()
        {
            Player.print("Buster " + EntityId + " : " + "Can capture : " + CanCapture().ToString() + " / is holding : " + IsHoldingAGhost().ToString() + " / can release : " + CanRelease().ToString());
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
    class Player
    {
        static void Main(string[] args)
        {
            bool _teamInitialized = false;
            int bustersPerPlayer = int.Parse(Console.ReadLine()); // the amount of busters you control
            int ghostCount = int.Parse(Console.ReadLine()); // the amount of ghosts on the map
            int myTeamId = int.Parse(Console.ReadLine()); // if this is 0, your base is on the top left of the map, if it is one, on the bottom right
            List<Buster> busters = new List<Buster>();
            List<Ghost> ghosts = new List<Ghost>();
            Vector2 basePosition;

            // Initialize FSM
            InitializeFSM();

            // Initialize game infos
            if (myTeamId == 0)
            {
                basePosition = new Vector2(0, 0);
            }
            else
            {
                basePosition = new Vector2(16000, 9000);
            }

            // game loop
            while (true)
            {
                int entities = int.Parse(Console.ReadLine()); // the number of busters and ghosts visible to you

                // Reset ghost in range
                foreach (Entity ghost in ghosts)
                {
                    ghost.IsVisible = false;
                }

                for (int i = 0; i < entities; i++)
                {
                    string[] inputs = Console.ReadLine().Split(' ');
                    int entityId = int.Parse(inputs[0]); // buster id or ghost id
                    int x = int.Parse(inputs[1]);
                    int y = int.Parse(inputs[2]); // position of this buster / ghost
                    int entityType = int.Parse(inputs[3]); // the team id if it is a buster, -1 if it is a ghost.
                    int state = int.Parse(inputs[4]); // For busters: 0=idle, 1=carrying a ghost.
                    int value = int.Parse(inputs[5]); // For busters: Ghost id being carried. For ghosts: number of busters attempting to trap this ghost.

                    // If this is the first turn, we initialize our Busters with their position, id and the base position
                    if (!_teamInitialized)
                    {
                        if (entityType == myTeamId)
                        {
                            busters.Add(new Buster(new Vector2(x, y), entityId, basePosition));
                        }
                    }
                    
                    // If the current entity is a ghost
                    if (entityType == -1)
                    {
                        int foundId = ghosts.FindIndex(e => e.EntityId == entityId);
                        if(foundId == -1)
                        {
                            ghosts.Add(new Ghost(new Vector2(x, y), entityId));
                        }
                        else
                        {
                            ghosts[foundId].Position = new Vector2(x, y);
                            ghosts[foundId].IsVisible = true;
                        }
                    }
                    
                    // Update busters informations
                    if (entityType == myTeamId)
                    {
                        Buster buster = busters.Find(e => e.EntityId == entityId);
                        // Check if our buster is holding a ghost and update this information
                        if (value != -1)
                        {
                            buster.GhostCaptured = true;
                            ghosts.Find(e => e.EntityId == value).Captured = true;
                            // TODO : Mark this ghost as captured so we can't capture it again
                        }
                        else
                        {
                            buster.GhostCaptured = false;
                            buster.GhostInRange = -1;
                        }

                        // Update its position
                        buster.Position = new Vector2(x, y);
                    }
                }

                if (!_teamInitialized)
                {
                    _teamInitialized = true;
                }

                // TODO : Each turn give infos to the busters about the strategy chosen by the multi-agent system
                
                /* CAPTURE TIME ! */
                // buster id, ghost id, distance between them
                List<Tuple<int, int, int>> busterToGhost = new List<Tuple<int, int, int>>();

                // Foreach known ghost get distance to each buster if in range of capture
                foreach (Ghost ghost in ghosts.FindAll(e => e.IsVisible == true && e.Captured == false))
                {
                    for (int i = 0; i < bustersPerPlayer; i++)
                    {
                        int distanceToGhost = (int) Vector2.Distance(busters[i].Position, ghost.Position);

                        // Check if we can capture a ghost
                        if (distanceToGhost > 900 && distanceToGhost < 1760)
                        {
                            busterToGhost.Add(new Tuple<int, int, int>(busters[i].EntityId, ghost.EntityId, distanceToGhost));
                        }
                    }
                }

                // Get closest ghost
                for (int i = 0; i < bustersPerPlayer; i++)
                {
                    // TODO : This should not be handled by the MAS, this look like agent responsibility
                    // Check if this buster is not busy
                    if (busters[i].State == BusterState.MoveState && !busters[i].CanCapture())
                    {
                        int lowest = 9999;
                        int ghostId = -1;
                        // Get the closest ghost
                        foreach (var item in busterToGhost.FindAll(e => e.Item1 == busters[i].EntityId))
                        {
                            if(item.Item3 < lowest)
                            {
                                ghostId = item.Item2;
                                lowest = item.Item3;
                            }
                        }
                        busters[i].GhostInRange = ghostId;

                        // If no ghost can be captured we want to chase the known ghosts
                        if(ghostId == -1)
                        {
                            Vector2 nextPos = new Vector2(0, 0);

                            float smallest = 999999;
                            foreach (Entity ghost in ghosts.FindAll(e => e.Captured == false))
                            {
                                float dist = Vector2.Distance(busters[i].Position, ghost.Position);
                                if(dist < smallest)
                                {
                                    nextPos = ghost.Position;
                                    smallest = dist;
                                }
                            }

                            if (nextPos != new Vector2(0, 0))
                            {
                                busters[i].TargetPosition = nextPos;
                            }
                        }
                    }
                }

                // TODO : Opti - If two buster have the same ghost change to number 2 for one of them

                // Check for each buster if they are in base range
                for (int i = 0; i < bustersPerPlayer; i++)
                {
                    if (Vector2.Distance(busters[i].Position, basePosition) <= 1600)
                    {
                        print("Is in drop zone");
                        busters[i].IsInDropZone = true;
                    }
                    else
                    {
                        busters[i].IsInDropZone = false;
                    }

                    busters[i].Debug();
                    busters[i].ComputeInformations();
                    Console.WriteLine(busters[i].ComputeNextOrder());
                }
            }
        }

        public static void print(string message)
        {
            Console.Error.WriteLine(message);
        }

        public static void InitializeFSM()
        {
            // Initialize FSM
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
            Player.print("ENTER STATE CALLED");
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

                // buster.TargetPosition = new Vector2(8000, 4500);
                // buster.TargetPosition = new Vector2(4000, 2250);
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
