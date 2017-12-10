namespace CodeBuster
{
    class Buster
    {
        public Vector2 Position { get; set; }
        public int EntityId { get; }
        public bool IsInDropZone { get; set; }
        public bool GhostCaptured { get; set; }
        public int GhostInRange { get; set; }
        public BusterState State { get; set; }

        public Buster(Vector2 initialPosition, int entityId)
        {
            this.Position = initialPosition;
            this.EntityId = entityId;

            // Initialize values
            IsInDropZone = false;
            GhostCaptured = false;
            GhostInRange = -1;

            // Initialize default state
            State = BusterState.MoveState;
        }

        public void ComputeInformations()
        {
            // TODO : Check if in drop zone
            // TODO : Check if a ghost is in range
            // TODO : Check if ghost captured
            State.ComputeInformations(this);
        }

        public string ComputeNextOrder()
        {
            return State.Update(this);
        }
    }
}