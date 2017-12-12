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
        public Buster[] busters ;
        public Ghost[] ghosts;
        public Vector2 basePosition;

        public Brain(int numberOfBusters, int numberOfGhosts, int teamId)
        {
            TeamInitialized = false;
            busters = new Buster[numberOfBusters];
            ghosts = new Ghost[numberOfGhosts];
            TeamId = teamId;

            // Initialize game infos
            if (TeamId == 0)
            {
                basePosition = new Vector2(0, 0);
            }
            else
            {
                basePosition = new Vector2(16000, 9000);
            }
        }

        public void AddBuster(int entityId, Vector2 position)
        {
            // TODO : Remove base position from the Buster constructor
            busters[entityId] = new Buster(entityId, position, basePosition);
        }

        public void AddGhost(int entityId, Vector2 position)
        {
            ghosts[entityId] = new Ghost(position, entityId);
        }

        public int GetGhostId(int entityId)
        {
            return ghosts.ToList().FindIndex(e => e.EntityId == entityId);
        }

        public void CreateOrUpdateGhost(int entityId, Vector2 position, bool isVisible)
        {
            int foundId = GetGhostId(entityId);
            if (foundId == -1)
            {
                AddGhost(entityId, position);
            }
            else
            {
                ghosts[foundId].Position = position;
                ghosts[foundId].IsVisible = isVisible;
            }
        }
    }
}
