namespace CodeBuster
{
    class MovingState : IBusterState
    {
        public MovingState()
        {
        }

        public override void Enter(Buster buster)
        {

        }

        public override string Update(Buster buster)
        {
            return "MOVE 8000 4500";
            // MOVE x y
        }

        public override void ComputeInformations(Buster buster)
        {
            // TODO : define possible transitions
            // TODO : Is buster in range to capture a ghost
        }
    }
}