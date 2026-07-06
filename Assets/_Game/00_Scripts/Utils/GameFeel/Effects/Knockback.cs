using UnityEngine;

namespace Slafurry.Utils.GameFeel
{
    /// <summary>
    /// Pushes the entity away on hit. Works with either Rigidbody or
    /// Rigidbody2D - whichever is found on this GameObject.
    /// Since PlayEffect() takes no parameters, call SetDirection() first
    /// (e.g. from the code that detected the hit) before PlayEffect().
    /// If no direction was set, falls back to pushing back along -transform.forward.
    /// </summary>
    public class Knockback : MonoBehaviour, IGameFeelEffect
    {
        [SerializeField] private float force = 5f;
        [SerializeField] private ForceMode forceMode = ForceMode.Impulse;

        private Rigidbody _rb3D;
        private Rigidbody2D _rb2D;
        private Vector3 _pendingDirection;
        private bool _hasPendingDirection;

        void Awake()
        {
            _rb3D = GetComponent<Rigidbody>();
            _rb2D = GetComponent<Rigidbody2D>();

            if (_rb3D == null && _rb2D == null)
                Debug.LogWarning($"[Knockback] No Rigidbody or Rigidbody2D found on {name}.", this);
        }

        /// <summary>Call this before PlayEffect() to control the push direction (e.g. away from the attacker).</summary>
        public void SetDirection(Vector3 direction)
        {
            _pendingDirection = direction;
            _hasPendingDirection = true;
        }

        public void PlayEffect()
        {
            Vector3 direction = _hasPendingDirection && _pendingDirection.sqrMagnitude > 0.001f
                ? _pendingDirection.normalized
                : -transform.forward;

            _hasPendingDirection = false;

            if (_rb3D != null)
            {
                _rb3D.AddForce(direction * force, forceMode);
            }
            else if (_rb2D != null)
            {
                Vector2 dir2D = new Vector2(direction.x, direction.y);
                ForceMode2D mode2D = forceMode == ForceMode.Impulse ? ForceMode2D.Impulse : ForceMode2D.Force;
                _rb2D.AddForce(dir2D * force, mode2D);
            }
        }

        public void StopEffect()
        {
            if (_rb3D != null) _rb3D.velocity = Vector3.zero;
            if (_rb2D != null) _rb2D.velocity = Vector2.zero;
        }
    }
}