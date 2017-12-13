using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
