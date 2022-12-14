using andywiecko.PBD2D.Core;
using UnityEngine;

namespace andywiecko.PBD2D.Examples
{
    public class IncreaseDensity : MonoBehaviour
    {
        [SerializeField]
        private GameObject target;

        [SerializeField]
        private float factor;

        [SerializeField]
        private SpriteRenderer spriteRenderer;

        public void Start()
        {
            var target = this.target.GetComponent<IPointsProvider>();
            var weights = target.Weights.Value.AsSpan();
            var points = target.Positions.Value.AsReadOnlySpan();
            var aabb = spriteRenderer.bounds.ToAABB();
            foreach (var i in 0..weights.Length)
            {
                var p = points[i];
                if (aabb.Contains(p))
                {
                    weights[i] *= factor;
                }
            }
        }
    }
}