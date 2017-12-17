using System;
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
                    brain.SetRoles();
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