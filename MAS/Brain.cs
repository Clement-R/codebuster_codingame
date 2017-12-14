﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

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
