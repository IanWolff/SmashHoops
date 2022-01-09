using Platformer.Core;
using Platformer.Mechanics;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player performs a Full Jump.
    /// </summary>
    /// <typeparam name="PlayerFullJumped"></typeparam>
    public class PlayerFullJumped : Simulation.Event<PlayerFullJumped>
    {
        public PlayerController player;

        public override void Execute()
        {
            if (player.audioSource && player.jumpGroundAudio)
                player.audioSource.PlayOneShot(player.jumpGroundAudio);
        }
    }
}