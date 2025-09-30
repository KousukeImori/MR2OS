using UnityEngine;

namespace raspberly.ovr
{
    public class FollowHUD : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float followMoveSpeed = 0.1f;
        [SerializeField] private bool isImmediateMove;

        [Header("並進のロック")]
        [SerializeField] private bool isLockX;
        [SerializeField] private bool isLockY;
        [SerializeField] private bool isLockZ;

        [Header("回転のロック")]
        [SerializeField] private bool isLockRotX;
        [SerializeField] private bool isLockRotY;
        [SerializeField] private bool isLockRotZ;

        private void Start()
        {
            if (!target) target = Camera.main.transform;
        }

        private void LateUpdate()
        {
            // --- 位置 ---
            Vector3 targetPos = target.position;
            Vector3 newPos;

            if (isImmediateMove)
            {
                newPos = targetPos;
            }
            else
            {
                newPos = Vector3.Lerp(transform.position, targetPos, followMoveSpeed);
            }

            if (isLockX) newPos.x = transform.position.x;
            if (isLockY) newPos.y = transform.position.y;
            if (isLockZ) newPos.z = transform.position.z;

            transform.position = newPos;

            // --- 回転 ---
            Vector3 targetEuler = target.rotation.eulerAngles;
            Vector3 currentEuler = transform.rotation.eulerAngles;

            if (isLockRotX) targetEuler.x = currentEuler.x;
            if (isLockRotY) targetEuler.y = currentEuler.y;
            if (isLockRotZ) targetEuler.z = currentEuler.z;

            transform.rotation = Quaternion.Euler(targetEuler);
        }

        // 強制同期
        public void ImmediateSync(Transform targetTransform)
        {
            transform.position = targetTransform.position;
            transform.rotation = targetTransform.rotation;
        }
    }
}
