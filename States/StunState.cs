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
            return "STUN " + buster.EnemyInRange.ToString();
        }

        public override void ComputeInformations(Buster buster)
        {
            // If we've just attack an enemy and we've no more enemy to stun go to scout again
            if (!buster.CanAttack())
            {
                buster.State = BusterState.MoveState;
            }
        }
    }
}
