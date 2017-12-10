namespace CodeBuster
{
    class Buster
    {
        public Vector2 Position { get; set; }
        public Vector2 BasePosition { get; set; }
        public int EntityId { get; }
        public bool IsInDropZone { get; set; }
        public bool GhostCaptured { get; set; }
        public int GhostInRange { get; set; }
        public BusterState State { get; set; }

        public Buster(Vector2 initialPosition, int entityId, Vector2 basePosition)
        {
            this.Position = initialPosition;
            this.EntityId = entityId;

            // Initialize values
            IsInDropZone = false;
            GhostCaptured = false;
            BasePosition = basePosition;
            GhostInRange = -1;

            // Initialize default state
            State = BusterState.MoveState;
        }

        public void ComputeInformations()
        {
            // TODO : Check if in drop zone
            // TODO : Check if a ghost is in range
            // TODO : Check if ghost captured
            Player.print(State.ToString());
            State.ComputeInformations(this);
        }

        public string ComputeNextOrder()
        {
            return State.Update(this);
        }

        public bool CanRelease()
        {
            if (GhostCaptured && IsInDropZone)
            {
                return true;
            }

            return false;
        }

        public bool CanCapture()
        {
            Player.print(GhostInRange.ToString());

            if (GhostInRange != -1)
            {
                return true;
            }

            return false;
        }

        public bool IsHoldingAGhost()
        {
            return GhostCaptured;
        }
    }
}