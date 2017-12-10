namespace CodeBuster
{
    class CaptureState : BusterState
    {
        public CaptureState()
        {
        }

        public override void Enter(Buster buster)
        {

        }

        public override string Update(Buster buster)
        {
            return "BUST id";
            // BUST id
        }

        public override void ComputeInformations(Buster buster)
        {
            // TODO : define possible transitions
        }
    }
}
