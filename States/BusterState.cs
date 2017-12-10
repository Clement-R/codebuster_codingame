namespace CodeBuster
{
    class BusterState
    {
        public static MovingState MoveState { get; set; }
        public static CaptureState CaptureState { get; set; }
        public static ReleaseState ReleaseState { get; set; }

        public virtual string Update(Buster buster) { return ""; }
        public virtual void ComputeInformations(Buster buster) { }
        public virtual void Enter(Buster buster) { }
    }
}
