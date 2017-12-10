
    using System;
    using System.Linq;
    using System.IO;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;

        ﻿namespace CodeBuster
{
    class Buster
    {
        public Vector2 Position { get; set; }
        public int EntityId { get; }
        public bool IsInDropZone { get; set; }
        public bool GhostCaptured { get; set; }
        public bool GhostInRange { get; set; }
        public BusterState State { get; set; }

        public Buster(Vector2 initialPosition, int entityId)
        {
            this.Position = initialPosition;
            this.EntityId = entityId;

            // Initialize values
            IsInDropZone = false;
            GhostCaptured = false;
            GhostInRange = false;

            // Initialize default state
            State = BusterState.MoveState;
        }

        public void ComputeInformations()
        {
            // TODO : Check if in drop zone
            // TODO : Check if a ghost is in range
            // TODO : Check if ghost captured
            State.ComputeInformations(this);
        }

        public string ComputeNextOrder()
        {
            return State.Update(this);
        }
    }
}
﻿namespace CodeBuster
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
﻿namespace CodeBuster
{
    class Ghost : Entity
    {
        public Ghost(Vector2 initialPosition, int entityId) : base(initialPosition, entityId)
        {
        }
    }
}


namespace CodeBuster
{
    class Vector2
    {
        public int x { get; set; }
        public int y { get; set; }

        public Vector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static int Distance(Vector2 from, Vector2 to)
        {
            return (int)Math.Ceiling(Math.Sqrt(Math.Pow((to.x - from.x), 2) + Math.Pow((to.y - from.y), 2)));
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
            Buster[] busters = new Buster[bustersPerPlayer];
            List<Ghost> ghosts = new List<Ghost>();
            Vector2 basePosition;

            // Initialize FSM
            if (BusterState.MoveState == null)
            {
                BusterState.MoveState = new MovingState();
            }

            if (BusterState.CaptureState == null)
            {
                BusterState.CaptureState = new CaptureState();
            }

            if (BusterState.ReleaseState == null)
            {
                BusterState.ReleaseState = new ReleaseState();
            }

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

                    // If this is the first turn, we initialize our Busters with their position and id
                    if (!_teamInitialized)
                    {
                        if (entityType == myTeamId)
                        {
                            busters[i] = new Buster(new Vector2(x, y), entityId);
                        }
                    }

                    // TODO : Store informations in way to be more usable later

                    // If the current entity is a ghost
                    int foundId = ghosts.FindIndex(e => e.EntityId == entityId);
                    if (foundId == -1)
                    {
                        ghosts.Add(new Ghost(new Vector2(x, y), entityId));
                    }
                    else
                    {
                        ghosts[foundId].IsVisible = true;
                    }

                    // Check if our buster is holding a ghost
                    if (entityType == myTeamId)
                    {
                        if (value != -1)
                        {
                            busters[i].GhostCaptured = true;
                        }
                        else
                        {
                            busters[i].GhostCaptured = false;
                        }
                    }
                }

                if (!_teamInitialized)
                {
                    _teamInitialized = true;
                }

                // TODO : Each turn give infos to the busters about the strategy chosen by the multi-agent system

                // Foreach known ghost get distance to each buster
                // TODO : Keep a list of each ghost a buster can capture
                // TODO : Get closest ghost for each buster
                // TODO : If two buster have the same ghost change to number 2 for one of them

                // buster id, ghost id, distance between them
                List<Tuple<int, int, int>> busterToGhost = new List<Tuple<int, int, int>>();

                foreach (Entity ghost in ghosts)
                {
                    for (int i = 0; i < bustersPerPlayer; i++)
                    {
                        int distanceToGhost = Vector2.Distance(busters[i].Position, ghost.Position);

                        if (distanceToGhost > 900 && distanceToGhost < 1760)
                        {
                            // busterToGhost.Add(new Tuple<int, int, int>(  ));
                            // TODO : Get closest buster
                            // TODO : Check if this buster is not busy
                            // TODO : Tell this buster to capture the ghost
                        }
                    }
                }
                // TODO : Store this distance for this frame (needs to be recompute at each frame)

                for (int i = 0; i < bustersPerPlayer; i++)
                {
                    // Check if in base range
                    if (Vector2.Distance(busters[i].Position, new Vector2(0, 0)) <= 1600)
                    {
                        busters[i].IsInDropZone = true;
                    }
                    else
                    {
                        busters[i].IsInDropZone = false;
                    }

                    /*
                    // TODO : Check if a ghost is in range
                    foreach (Entity ghost in ghosts)
                    {
                        // TODO : opti - space separation : voronoi, quadtree
                        int distanceToGhost = Vector2.Distance(busters[i].Position, ghost.Position);
                        if (distanceToGhost > 900 && distanceToGhost < 1760)
                        {
                            print("GHOST IN RANGE");
                            // TODO : Get closest buster
                            // TODO : Check if this buster is not busy
                            // TODO : Tell this buster to capture the ghost
                        }
                        else
                        {
                            print("NO GHOST IN RANGE");
                        }
                    }
                    */

                    busters[i].ComputeInformations();
                    Console.WriteLine(busters[i].ComputeNextOrder());
                }
            }
        }

        public static void print(string message)
        {
            Console.Error.WriteLine(message);
        }
    }
}
﻿namespace CodeBuster
{
    class BusterState
    {
        public static MovingState MoveState { get; set; }
        public static CaptureState CaptureState { get; set; }
        public static ReleaseState ReleaseState { get; set; }

        public virtual string Update(Buster buster) { return ""; }
        public virtual void ComputeInformations(Buster buster) { }
        public virtual void Enter(Buster buster) { }
    }
}

﻿namespace CodeBuster
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
            return "BUST id";
            // BUST id
        }

        public override void ComputeInformations(Buster buster)
        {
            // TODO : define possible transitions
        }
    }
}

﻿namespace CodeBuster
{
    class MovingState : BusterState
    {
        public MovingState()
        {
        }

        public override void Enter(Buster buster)
        {

        }

        public override string Update(Buster buster)
        {
            return "MOVE 8000 4500";
            // MOVE x y
        }

        public override void ComputeInformations(Buster buster)
        {
            // TODO : define possible transitions
            // TODO : Is buster in range to capture a ghost
        }
    }
}
﻿namespace CodeBuster
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
            // TODO : define possible transitions
            // If buster has no ghost switch to move
            if (!buster.GhostCaptured)
            {
                buster.State = BusterState.MoveState;
            }
        }
    }
}