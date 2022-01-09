using Platformer.Core;
using Platformer.Mechanics;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player performs a Ground Jump.
    /// </summary>
    /// <typeparam name="PlayerJumpedGround"></typeparam>
    public class PlayerJumpedGround : Simulation.Event<PlayerJumpedGround>
    {
        public PlayerController player;

        public override void Execute()
        {
            if (player.audioSource && player.jumpGroundAudio)
                player.audioSource.PlayOneShot(player.jumpGroundAudio);
        }
    }
}