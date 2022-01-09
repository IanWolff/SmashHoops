using Platformer.Core;
using Platformer.Mechanics;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player performs a Short Jump.
    /// </summary>
    /// <typeparam name="PlayerShortJumped"></typeparam>
    public class PlayerShortJumped : Simulation.Event<PlayerShortJumped>
    {
        public PlayerController player;

        public override void Execute()
        {
            if (player.audioSource && player.jumpGroundAudio)
                player.audioSource.PlayOneShot(player.jumpGroundAudio);
        }
    }
}