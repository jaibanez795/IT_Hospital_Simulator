using UnityEngine;

public class WorldBillboardLabel : MonoBehaviour
{
    void LateUpdate()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(transform.position - camera.transform.position);
    }
}
