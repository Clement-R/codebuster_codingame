using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBuster
{
    class StunState : BusterState
    {
        public StunState()
        {
        }

        public override void Enter(Buster buster)
        {

        }

        public override string Update(Buster buster)
        {
            // STUN id
            buster.CanStun = false;
            return "STUN " + buster.EnemyInRange.EntityId.ToString();
        }

        public override void ComputeInformations(Buster buster)
        {
            // If we've just attacked an enemy go to scout again
            if (!buster.CanAttack())
            {
                buster.State = BusterState.MoveState;
            }
        }
    }
}
