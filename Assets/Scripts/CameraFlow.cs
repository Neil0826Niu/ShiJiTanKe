using UnityEngine;

namespace Assets.Scripts
{
    public class CameraFlow : MonoBehaviour
    {

        private float distance = 10F;
        public GameObject targetGameObject;
        private float roll = 30f * Mathf.PI * 2 / 360;

        private float rot = 0;

        private void Update()
        {
            Vector3 targetPos = targetGameObject.transform.position;
            Vector3 cameraPos;
            float d = distance * Mathf.Cos(roll);
            float height = distance * Mathf.Sin(roll);
            cameraPos.x = targetPos.x + d * Mathf.Sin(rot);
            cameraPos.z = targetPos.z + d * Mathf.Cos(rot);
            cameraPos.y = targetPos.y + height;
            Camera.main.transform.position = cameraPos;
            Camera.main.transform.LookAt(targetGameObject.transform);
        }
    }
}
