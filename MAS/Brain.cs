using System;
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
                ghost.KnownLocation = isVisible;
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
                    // Reset capture variables
                    buster.GhostCaptured = null;
                    break;
            }

            // Update its position
            buster.Position = position;

            // Check for each buster if they are in base range
            if (Vector2.Distance(buster.Position, BasePosition) <= 1600)
            {
                Player.print("Is in drop zone");
                buster.IsInDropZone = true;
            }
            else
            {
                buster.IsInDropZone = false;
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

                foreach (var ghost in Ghosts)
                {
                    if (ghost.Position == buster.Position)
                    {
                        ghost.KnownLocation = false;
                        ghost.Locked = false;
                    }
                }
                

                // We will set these values later when we will get all game informations
                buster.GhostInRange = null;
                buster.EnemyInRange = null;
                buster.GhostCaptured = null;
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
        /// Given all the game informations we update our Busters with last intels
        /// </summary>
        public void ComputeInformations()
        {
            List<Tuple<int, int, int>> busterToGhost = GetEachGhostInZoneForBusters();
            
            foreach (var buster in Busters)
            {
                // Get the closest ghost that we can capture
                int lowest = 9999;
                foreach (var item in busterToGhost.FindAll(e => e.Item1 == buster.EntityId))
                {
                    if (item.Item3 < lowest && !Ghosts.Find(e => e.EntityId == item.Item2).Locked)
                    {
                        lowest = item.Item3;
                        buster.GhostInRange = Ghosts.Find(e => e.EntityId == item.Item2);
                        buster.GhostInRange.Locked = true;
                        Player.print("ONE GHOST FOUND IN AREA");
                    }
                }

                // If an enemy is visible and he's carrying a ghost : ATTACK HIM !
                float lowestDistance = 99999f;
                foreach (var enemy in Enemies.FindAll(e => e.IsVisible == true && e.IsCarryingAGhost == true))
                {
                    float distanceToEnemy = Vector2.Distance(buster.Position, enemy.Position);
                    if (distanceToEnemy < lowestDistance)
                    {
                        lowestDistance = distanceToEnemy;
                        buster.EnemyInRange = enemy;
                        // TODO : Maybe remove this attribute, it's likely to cause bugs
                        buster.TargetPosition = enemy.Position;
                        Player.print("CHASING AN ENEMY : " + enemy.EntityId.ToString());
                    }
                }

                // If there was no enemy carrying a ghost, check if we've an enemy at stun range
                if (buster.EnemyInRange == null)
                {
                    // Check for each enemy if we are in range to stun one
                    lowestDistance = 99999f;
                    foreach (var enemy in Enemies.FindAll(e => e.IsVisible == true && e.RemainingStunTurns == -1 && e.Targeted == false))
                    {
                        float distanceToEnemy = Vector2.Distance(buster.Position, enemy.Position);
                        if (distanceToEnemy <= 1760)
                        {
                            if (distanceToEnemy < lowestDistance)
                            {
                                lowestDistance = distanceToEnemy;
                                buster.EnemyInRange = enemy;
                                enemy.Targeted = true;

                                Player.print("GOING TO ATTACK");
                            }
                        }
                    }
                }

                // If we can't capture or attack, we've to move
                // First : Check for known ghost positions and chase them
                if (buster.GhostInRange == null)
                {
                    Vector2 nextPos = new Vector2(0, 0);
                    Ghost targetGhost = null;
                    float smallest = 999999;
                    foreach (Ghost freeGhost in Ghosts.FindAll(e => e.Captured == false && e.KnownLocation == true))
                    {
                        float dist = Vector2.Distance(buster.Position, freeGhost.Position);
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
                        Player.print("CHASING A GHOST : " + targetGhost.EntityId.ToString());
                        buster.TargetPosition = nextPos;
                    }
                }
                // Second : Just scout
                if (buster.Position == buster.TargetPosition)
                {
                    Player.print("I'm SCOUTING !");
                    buster.TargetPosition = GetNextPosition();
                }
            }
            
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
            /*
            List<Tuple<int, int, int>> busterToGhost = ComputeDistancesBetweenEachBusterAndGhosts();
            for (int i = 0; i < Busters.Count; i++)
            {
                Player.print("Buster : " + i.ToString());

                if(pos != new Vector2(0, 0))
                {
                    Busters[i].TargetPosition = pos;
                }

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
            */
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
