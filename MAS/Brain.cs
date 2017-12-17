using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace CodeBuster
{
    class Brain
    {
        public bool TeamInitialized { get; set; }
        public int TeamId { get; }
        public int NumberOfGhosts { get; }
        public List<Buster> Busters ;
        public List<Enemy> Enemies;
        public List<Ghost> Ghosts;
        public List<int> CapturedGhost;
        public Vector2 BasePosition;
        public Vector2 EnemyBasePosition;
        public Vector2 BlockerPosition;
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
            NumberOfGhosts = numberOfGhosts;

            // Initialize map
            GridMap = new Map();
            GridMap.Debug();

            // Initialize game infos
            // Initialize game infos
            if (TeamId == 0)
            {
                BasePosition = new Vector2(0, 0);
                EnemyBasePosition = new Vector2(16000, 9000);
                BlockerPosition = new Vector2(13600, 7000);
            }
            else
            {
                BasePosition = new Vector2(16000, 9000);
                EnemyBasePosition = new Vector2(0, 0);
                BlockerPosition = new Vector2(2000, 2000);
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
                ghost.NumberOfBustersCapturing = numberOfBusterCapturing;
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

            // Check for buster if they are in base range
            if (Vector2.Distance(buster.Position, BasePosition) <= 1600)
            {
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
                ghost.NumberOfBustersCapturing = 0;
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
                        capturableGhosts.Add(new Tuple<int, int>(ghost.EntityId, ghost.Life));
                    }
                }
            }

            capturableGhosts = capturableGhosts.OrderBy(e => e.Item2).ToList();

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
            foreach (var ghost in Ghosts.FindAll(e => e.IsVisible == true))
            {
                // If this ghost is not already captured
                if (!CapturedGhost.Contains(ghost.EntityId))
                {
                    int distanceToGhost = (int)Vector2.Distance(buster.Position, ghost.Position);
                    if((GetNumberOfGhostsCaptured() * 2) >= (int)Math.Floor((NumberOfGhosts / 4f) * 2))
                    {
                        // If half of the ghosts are captured we chase whatever we can
                        chasableGhosts.Add(new Tuple<int, int>(ghost.EntityId, distanceToGhost));
                    }
                    else if (distanceToGhost < 6000 || ghost.Life <= 15)
                    {
                        // And we're not too far
                        chasableGhosts.Add(new Tuple<int, int>(ghost.EntityId, distanceToGhost));
                    }
                }
            }

            chasableGhosts = chasableGhosts.OrderBy(e => e.Item2).ToList();

            return chasableGhosts;
        }

        public List<Tuple<int, int>> GetAllVisibleEnemiesCarrying(Buster buster)
        {
            List<Tuple<int, int>> attackableEnemies = new List<Tuple<int, int>>();
            foreach (var enemy in Enemies.FindAll(e => e.IsCarryingAGhost == true && e.IsVisible == true))
            {
                int distanceToEnemy = (int)Vector2.Distance(buster.Position, enemy.Position);
                attackableEnemies.Add(new Tuple<int, int>(enemy.EntityId, distanceToEnemy));
            }

            return attackableEnemies;
        }

        public List<Tuple<int, int>> GetInRangeEnemiesCarrying(Buster buster)
        {
            List<Tuple<int, int>> attackableEnemies = new List<Tuple<int, int>>();
            foreach (var enemy in Enemies.FindAll(e => e.IsCarryingAGhost == true && e.IsVisible == true))
            {
                int distanceToEnemy = (int)Vector2.Distance(buster.Position, enemy.Position);
                if(distanceToEnemy < 1760)
                {
                    attackableEnemies.Add(new Tuple<int, int>(enemy.EntityId, distanceToEnemy));
                }
            }

            attackableEnemies = attackableEnemies.OrderBy(e => e.Item2).ToList();

            return attackableEnemies;
        }

        public int GetNumberOfAllyCapturing(Buster busterAsking, Ghost ghost)
        {
            int number = 0;
            foreach (var buster in Busters.FindAll(e => e.EntityId != busterAsking.EntityId))
            {
                if (buster.GhostInRange != null && buster.GhostInRange.EntityId == ghost.EntityId)
                {
                    number++;
                }
            }
            return number;
        }

        public int GetNumberOfGhostsCaptured()
        {
            int i = 0;
            foreach (var buster in Busters)
            {
                if(buster.GhostCaptured != null) { i++; }
            }
            return CapturedGhost.Count + i;
        }

        public List<Tuple<int, int>> GetAttackableEnemies(Buster buster)
        {
            List<Tuple<int, int>> attackableEnemies = new List<Tuple<int, int>>();
            foreach (var enemy in Enemies.FindAll(e => e.IsVisible == true))
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
            // First we check if our buster is already doing a task or is stunned and skip all the task planning for them
            foreach (var buster in Busters)
            {
                if(buster.IsHoldingAGhost() || buster.IsStunned)
                {
                    buster.GotAnOrder = true;
                }
            }
            
            // Try to steal a ghost
            foreach (var buster in Busters)
            {
                Player.print("Buster " + buster.EntityId + " search to chase");
                // Find distance to all visible enemies carrying a ghost
                List<Tuple<int, int>> enemiesInRange = GetInRangeEnemiesCarrying(buster);
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

                    if(foundGhost.Life > 15)
                    {
                        // If half of the ghosts remains, don't catch them yet
                        if (!((GetNumberOfGhostsCaptured() * 2) >= (int)Math.Floor((NumberOfGhosts / 3f) * 2)))
                        {
                            continue;
                        }
                    }

                    // If we're in the first 25% of the game and half of our busters are already on this ghost, search a new one
                    if ((GetNumberOfGhostsCaptured() * 2) <= (int)Math.Floor((NumberOfGhosts / 4f) * 1))
                    {
                        Player.print("We're here");
                        if (foundGhost.NumberOfBustersCapturing != 0 && foundGhost.NumberOfBustersCapturing <= ((int)Math.Floor(Busters.Count / 2f)) && GetNumberOfAllyCapturing(buster, foundGhost) == foundGhost.NumberOfBustersCapturing)
                        {
                            Player.print("And here");
                            continue;
                        }
                    }
                    
                    // Do we already have found a ghost to capture ? If not we search
                    if (buster.GhostInRange == null)
                    {
                        // We get the distance between the ghost and all busters
                        List<Tuple<int, int>> bustersByDistance = GetDistanceToBusters(foundGhost);
                        foreach (var busterByDistance in bustersByDistance)
                        {
                            int closestBusterId = busterByDistance.Item1;
                            Player.print("Closest buster is " + closestBusterId);

                            if (closestBusterId == buster.EntityId)
                            {
                                // If we're the closest, we capture it
                                buster.GhostInRange = foundGhost;
                                buster.GhostInRange.NumberOfBustersCapturing++;
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
                                    buster.GhostInRange.NumberOfBustersCapturing++;
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
                    // Get all ghosts in close distance ( < 900 ) and wait to capture on next turn
                    List<Tuple<int, int>> closeRangeGhosts = GetCloseRangeGhosts(buster);
                    foreach (var ghost in closeRangeGhosts)
                    {
                        // Retrieve the actual ghost
                        Ghost foundGhost = Ghosts.Find(e => e.EntityId == ghost.Item1);
                        
                        Vector2 direction = foundGhost.Position - buster.Position;

                        Vector2 newPosition = new Vector2();
                        if(foundGhost.Position.X > buster.Position.X)
                        {
                            newPosition.X = buster.Position.X - 400;
                        }
                        else
                        {
                            newPosition.X = buster.Position.X + 400;
                        }

                        if (foundGhost.Position.Y > buster.Position.Y)
                        {
                            newPosition.Y = buster.Position.Y - 200;
                        }
                        else
                        {
                            newPosition.Y = buster.Position.Y + 200;
                        }

                        buster.TargetPosition = new Vector2((int)newPosition.X, (int)newPosition.Y);

                        //if(direction.Length() <= 100f)
                        //{
                        //    Vector2 newPosition = new Vector2(buster.Position.X - 800, buster.Position.Y - 800);
                        //    buster.TargetPosition = new Vector2((int)newPosition.X, (int)newPosition.Y);
                        //}
                        //else
                        //{
                        //    Vector2 newPosition = buster.Position + Vector2.Normalize(direction) * 400;
                        //    buster.TargetPosition = new Vector2((int)newPosition.X, (int)newPosition.Y);
                        //}

                        buster.GotAnOrder = true;
                        Player.print("Too close, moving");
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
                    // if there is 33% left or less of ghosts we stop stunning and keep for an enemy thay carry
                    Player.print("Chase infos : " + GetNumberOfGhostsCaptured() * 2 + " " + (int)Math.Floor((NumberOfGhosts / 3f) * 2));
                    if((GetNumberOfGhostsCaptured() * 2) <= (int)Math.Floor((NumberOfGhosts / 3f) * 2))
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
                // Search a ghost to chase
                List<Tuple<int, int>> chasableGhosts = GetChasableGhosts(buster);
                foreach (var ghost in chasableGhosts)
                {
                    // Retrieve the actual ghost
                    Ghost foundGhost = Ghosts.Find(e => e.EntityId == ghost.Item1);

                    if (foundGhost.Life > 15)
                    {
                        // If half of the ghosts remains, don't catch them yet
                        if (!((GetNumberOfGhostsCaptured() * 2) >= (int)Math.Floor((NumberOfGhosts / 3f) * 2)))
                        {
                            continue;
                        }
                    }

                    if ((GetNumberOfGhostsCaptured() * 2) <= (int)Math.Floor((NumberOfGhosts / 4f) * 1))
                    {
                        // If we're in the first 25% of the game and half of our busters are already on this ghost, search a new one

                        Player.print("Ghost chase infos " + foundGhost.NumberOfBustersCapturing + " " + ((int)Math.Floor(Busters.Count / 2f)) + " " + GetNumberOfAllyCapturing(buster, foundGhost));

                        if (foundGhost.NumberOfBustersCapturing != 0 && foundGhost.NumberOfBustersCapturing <= ((int)Math.Floor(Busters.Count / 2f)) && GetNumberOfAllyCapturing(buster, foundGhost) == foundGhost.NumberOfBustersCapturing)
                        {
                            continue;
                        }
                    }

                    // Do we already have found a ghost to chase ? If not we search
                    if (buster.GhostChased == null)
                    {
                        // We get the distance between the ghost and all busters
                        List<Tuple<int, int>> bustersByDistance = GetDistanceToBusters(foundGhost);
                        foreach (var busterByDistance in bustersByDistance)
                        {
                            Player.print("Buster " + busterByDistance.Item1 + " - distance : " + busterByDistance.Item2 + " - busy : " + Busters.Find(e => e.EntityId == busterByDistance.Item1).IsBusy());
                            int closestBusterId = busterByDistance.Item1;

                            if (buster.EntityId == closestBusterId)
                            {
                                // If we're the closest, we chase them
                                buster.GhostChased = foundGhost;
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

            foreach (var buster in Busters)
            {
                // If we're getting in a brawl where N of our busters and N+1 of enemy busters are capturing, we stun one of them
                if (buster.State == BusterState.MoveState && buster.GhostInRange != null)
                {
                    Player.print("Analysing a brawl by " + buster.EntityId);
                    int num = GetNumberOfAllyCapturing(buster, buster.GhostInRange);
                    if ((buster.GhostInRange.NumberOfBustersCapturing - num) == (num + 1) && buster.CanStun)
                    {
                        // Find all enemies in range
                        List<Tuple<int, int>> enemiesInRange = GetAttackableEnemies(buster);

                        if (enemiesInRange.Count > 0)
                        {
                            Enemy foundEnemy = Enemies.Find(e => e.EntityId == enemiesInRange.First().Item1);
                            Player.print("Not this time !");
                            buster.GhostInRange.NumberOfBustersCapturing--;
                            buster.GhostInRange = null;
                            buster.EnemyInRange = foundEnemy;
                            buster.GotAnOrder = true;
                        }
                    }
                }

                // Experiment one new role : Blocker
                //if (buster.Role == "Blocker" && !buster.IsStunned)
                //{
                //    // If around 80% of the ghosts are captured, we go and camp the enemy base
                //    if ((GetNumberOfGhostsCaptured() * 2) >= (int)Math.Floor((NumberOfGhosts / 10f) * 8))
                //    {
                //        Player.print("I'M A BLOCKER");
                //        // We detect an enemy not carrying ? Ignore
                //        if ((buster.EnemyChased != null && !buster.EnemyChased.IsCarryingAGhost) || (buster.EnemyInRange != null && !buster.EnemyInRange.IsCarryingAGhost))
                //        {
                //            buster.EnemyInRange = null;
                //            buster.TargetPosition = BlockerPosition;
                //            buster.GotAnOrder = true;
                //        }
                //        else
                //        {
                //            // If the enemy is not in range, but we know his direction and can predict if we could stun him on next turn
                //            if (buster.EnemyChased != null)
                //            {
                //                Vector2 direction = EnemyBasePosition - buster.EnemyChased.Position;
                //                Vector2 nextEnemyPosition = buster.EnemyChased.Position + (Vector2.Normalize(direction) * 800);
                //                Player.print("Next enemy position must be : " + nextEnemyPosition);

                //                if (Vector2.Distance(nextEnemyPosition, buster.Position) < 1700)
                //                {
                //                    buster.EnemyInRange = buster.EnemyChased;
                //                    buster.EnemyChased = null;
                //                    buster.GotAnOrder = true;
                //                }
                //            }
                //        }

                //        if (buster.EnemyChased == null && buster.EnemyInRange == null)
                //        {

                //            if(buster.GhostCaptured != null)
                //            {
                //                // I'm running away !
                //                buster.GotAnOrder = true;
                //            }
                //            else
                //            {
                //                // We've stunned an enemy, get the ghost and run away
                //                List<Tuple<int, int>> ghostsInRange = GetCapturableGhosts(buster);
                //                foreach (var ghost in ghostsInRange)
                //                {
                //                    // Retrieve the actual ghost
                //                    Ghost foundGhost = Ghosts.Find(e => e.EntityId == ghost.Item1);
                //                    // Do we already have found a ghost to capture ? If not we search
                //                    if (buster.GhostInRange == null)
                //                    {
                //                        buster.GhostInRange = foundGhost;
                //                        buster.GotAnOrder = true;
                //                        break;
                //                    }
                //                }

                //                if (buster.GhostInRange == null)
                //                {
                //                    ghostsInRange = GetChasableGhosts(buster);
                //                    foreach (var ghost in ghostsInRange)
                //                    {
                //                        // Retrieve the actual ghost
                //                        Ghost foundGhost = Ghosts.Find(e => e.EntityId == ghost.Item1);
                //                        // Do we already have found a ghost to capture ? If not we search
                //                        if (buster.GhostInRange == null)
                //                        {
                //                            buster.GhostChased = foundGhost;
                //                            buster.GotAnOrder = true;
                //                            break;
                //                        }
                //                    }
                //                }
                //            }

                //            if (buster.Position == BlockerPosition && !buster.GotAnOrder)
                //            {
                //                buster.TargetPosition = new Vector2(BlockerPosition.X, BlockerPosition.Y - 50);
                //                buster.GotAnOrder = true;
                //            }
                //            else
                //            {
                //                buster.EnemyInRange = null;
                //                buster.GhostInRange = null;
                //                buster.GhostChased = null;
                //                buster.TargetPosition = BlockerPosition;
                //                buster.GotAnOrder = true;
                //            }
                //        }
                //    }
                //}
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

                // if the buster was chasing an enemy and he now stop moving we remove it's chased enemy reference
                if (buster.EnemyChased != null && buster.State != BusterState.MoveState)
                {
                    if(buster.EnemyChased.IsCarryingAGhost)
                    {
                        buster.TargetPosition = buster.EnemyChased.Position;
                        buster.GotAnOrder = true;
                    }
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
                if (buster.EnemyChased == null && buster.GhostChased == null && buster.State == BusterState.MoveState && !buster.IsHoldingAGhost() && !buster.GotAnOrder)
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
                        if (buster.Position == buster.GhostChased.Position || buster.GhostChased.Captured)
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
            // GridMap.Draw();
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

        public void SetRoles()
        {
            if (Busters.Count > 2)
            {
                // One of our buster is going to be a blocker : go to enemy base and attack every enemy that has a ghost
                Busters[0].Role = "Blocker";
            }
        }
    }
}
