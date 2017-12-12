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
        public List<Ghost> Ghosts;
        public Vector2 BasePosition;

        public Brain(int numberOfBusters, int numberOfGhosts, int teamId)
        {
            TeamInitialized = false;
            Busters = new List<Buster>();
            Ghosts = new List<Ghost>();
            TeamId = teamId;

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

        /// <summary> UpdateBusterInformations give informations to the buster according to its role
        /// entityId : id of the buster
        /// capturedGhost : id of the captured ghost, else -1
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
    }
}
