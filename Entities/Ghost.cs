using System.Numerics;

namespace CodeBuster
{
    class Ghost : Entity
    {
        public bool Locked { get; set; }
        public bool Captured { get; set; }
        public bool KnownLocation { get; set; }

        public Ghost(Vector2 initialPosition, int entityId) : base(initialPosition, entityId)
        {
            KnownLocation = true;
            Locked = false;
        }

        public new void Debug()
        {
            base.Debug();
            Player.print("Captured : " + Captured + " / Locked : " + Locked);
        }
    }
}
