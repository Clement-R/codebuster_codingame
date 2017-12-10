namespace CodeBuster
{
    interface IBusterState
    {
        public MovingState MoveState { get; set; }
        public CaptureState CaptureState { get; set; }
        public ReleaseState ReleaseState { get; set; }

        private static MovingState moveState = null;
        private static CaptureState captureState = null;
        private static ReleaseState releaseState = null;

        public IBusterState() { }

        public virtual string Update(Buster buster) { return ""; }
        public virtual void ComputeInformations(Buster buster) { }
        public virtual void Enter(Buster buster) { }
    }
}
