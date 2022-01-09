using Platformer.Core;
using Platformer.Mechanics;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player performs an Air Jump.
    /// </summary>
    /// <typeparam name="PlayerJumpedAir"></typeparam>
    public class PlayerJumpedAir : Simulation.Event<PlayerJumpedAir>
    {
        public PlayerController player;

        public override void Execute()
        {
            if (player.audioSource && player.jumpAirAudio)
                player.audioSource.PlayOneShot(player.jumpAirAudio);
        }
    }
}