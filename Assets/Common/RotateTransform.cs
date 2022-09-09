using UnityEngine;

namespace andywiecko.PBD2D.Examples
{
    public class RotateTransform : MonoBehaviour
    {
        [SerializeField]
        private Transform target;

        [SerializeField, Range(-90, 90)]
        private float speed = 10f;

        private void Update()
        {
            if (target != null)
            {
                target.Rotate(new(0, 0, 1), speed);
            }
        }
    }
}