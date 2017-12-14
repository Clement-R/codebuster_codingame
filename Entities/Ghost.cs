﻿using System.Numerics;

namespace CodeBuster
{
    class Ghost : Entity
    {
        public bool Captured { get; set; }

        public Ghost(Vector2 initialPosition, int entityId) : base(initialPosition, entityId)
        {
        }

        public new void Debug()
        {
            base.Debug();
            Player.print("Captured : " + Captured);
        }
    }
}
