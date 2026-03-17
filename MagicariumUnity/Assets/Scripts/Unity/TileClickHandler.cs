using UnityEngine;

namespace Magicarium.Unity
{
    /// <summary>
    /// Component attached to each map tile button.
    /// Relays click events to the GameManager with the tile's grid coordinates.
    /// </summary>
    public class TileClickHandler : MonoBehaviour
    {
        public int TileX { get; set; }
        public int TileY { get; set; }

        /// <summary>Called by the Button's onClick event.</summary>
        public void OnClick()
        {
            GameManager.Instance?.OnTileClicked(TileX, TileY);
        }
    }
}
