namespace CodeBuster
{
    class Buster
    {
        Vector2 Position;

        private Vector2 Position { get; set; }
        public int EntityId { get; }
        public bool IsInDropZone { get; set; }
        public bool GhostCaptured { get; set; }
        public bool GhostInRange { get; set; }
        public IBusterState State { get; set; }

        public Buster(Vector2 initialPosition, int entityId)
        {
            this.Position = initialPosition;
            this.EntityId = entityId;

            // Initialize values
            IsInDropZone = false;
            GhostCaptured = false;
            GhostInRange = false;

            // Initialize default state
            State = IBusterState.moveState;
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