using System.Numerics;

namespace CodeBuster
{
    class Buster
    {
        public Vector2 Position { get; set; }
        public Vector2 GhostPosition { get; set; }
        public Vector2 EnemyPosition { get; set; }
        public Vector2 TargetPosition { get; set; }
        public Vector2 BasePosition { get; set; }
        public int EntityId { get; }
        public bool IsInDropZone { get; set; }
        // The ghost that I'm carrying
        public Ghost GhostCaptured { get; set; }
        // A ghost that I can capture
        public Ghost GhostInRange { get; set; }
        // The ghost I'm chasing
        public Ghost GhostChased { get; set; }
        public BusterState State { get; set; }
        public BusterState LastState { get; set; }
        public Enemy EnemyChased { get; set; }
        public Enemy EnemyInRange { get; set; }
        public int LastTurnStun { get; set; }
        public bool CanStun { get; set; }
        public bool IsScouting { get; set; }
        public bool IsStunned { get; set; }

        public Buster(int entityId, Vector2 initialPosition, Vector2 basePosition)
        {
            Position = initialPosition;
            EntityId = entityId;

            // Initialize values
            IsInDropZone = false;
            IsScouting = false;
            GhostCaptured = null;
            BasePosition = basePosition;
            GhostInRange = null;
            EnemyInRange = null;
            EnemyChased = null;
            GhostChased = null;
            CanStun = true;
            IsStunned = false;

            // Initialize MoveToPosition
            TargetPosition = initialPosition;

            // Initialize default state
            State = BusterState.MoveState;
        }

        public void ComputeInformations()
        {
            State.ComputeInformations(this);

            Player.print(State.ToString());

            if (State != LastState)
            {
                State.Enter(this);
            }

            LastState = State;
        }

        public string ComputeNextOrder()
        {
            return State.Update(this);
        }

        public bool CanRelease()
        {
            if (GhostCaptured != null && IsInDropZone)
            {
                return true;
            }

            return false;
        }

        public bool CanCapture()
        {
            if (GhostCaptured == null && GhostInRange != null)
            {
                return true;
            }

            return false;
        }

        public bool IsHoldingAGhost()
        {
            if(GhostCaptured == null)
            {
                return false;
            }

            return true;
        }

        public bool CanAttack()
        {
            if(EnemyInRange != null && CanStun)
            {
                return true;
            }

            return false;
        }

        public void Debug()
        {
            Player.print("Buster " + EntityId + " / position : " + Position.ToString() + " / target : " + TargetPosition.ToString() + " / can capture : " + CanCapture().ToString() + " / is holding : " + IsHoldingAGhost().ToString() + " / can release : " + CanRelease().ToString() + " / can attack : " + CanAttack().ToString() + " / can stun : " + CanStun + " / last turn stun : " + LastTurnStun.ToString() + " / ghost chased : " + ((GhostChased != null) ? GhostChased.EntityId : -1) + " / ghost in range : " + ((GhostInRange != null) ? GhostInRange.EntityId : -1) + " / enemy chased : " + ((EnemyChased != null) ? EnemyChased.EntityId : -1) + " / enemy in range : " + ((EnemyInRange != null) ? EnemyInRange.EntityId : -1));
        }

        public void MarkGhostAsCaptured()
        {
            if(GhostInRange != null)
            {
                GhostInRange.Captured = true;
            }
        }

        public bool IsBusy()
        {
            if(!IsHoldingAGhost() && GhostInRange == null && !IsStunned)
            {
                return false;
            }

            return true;
        }
    }
}