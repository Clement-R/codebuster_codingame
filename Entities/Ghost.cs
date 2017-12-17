using System.Numerics;

namespace CodeBuster
{
    class Ghost : Entity
    {
        public bool Locked { get; set; }
        public bool Captured { get; set; }
        public bool KnownLocation { get; set; }
        public int Life { get; set; }
        public int NumberOfBustersCapturing { get; set; }

        public Ghost(Vector2 initialPosition, int entityId, int life, int numberOfBusterCapturing) : base(initialPosition, entityId)
        {
            KnownLocation = true;
            Locked = false;
            Life = life;
            NumberOfBustersCapturing = numberOfBusterCapturing;
        }

        public new void Debug()
        {
            base.Debug();
            Player.print("Captured : " + Captured + " / Locked : " + Locked + " / Life : " + Life);
        }
    }
}
