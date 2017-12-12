﻿using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

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
            
            // Initialize multi-agent system
            Brain brain = new Brain(bustersPerPlayer, ghostCount, myTeamId);

            // Initialize game infos
            if (myTeamId == 0)
            {
                basePosition = new Vector2(0, 0);
            }
            else
            {
                basePosition = new Vector2(16000, 9000);
            }

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
                    if(!brain.TeamInitialized)
                    {
                        if (entityType == brain.TeamId)
                        {
                            brain.AddBuster(entityId, new Vector2(x, y));
                        }
                    }

                    if (!_teamInitialized)
                    {
                        if (entityType == myTeamId)
                        {
                            busters.Add(new Buster(entityId, new Vector2(x, y), basePosition));
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

                    // If the current entity is a ghost
                    if (entityType == -1)
                    {
                        brain.CreateOrUpdateGhost(entityId, new Vector2(x, y), true);
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