using UnityEngine;

public class ConsistentShelfScaler : MonoBehaviour
{
    void Update()
    {
        // When you press 'R' in the editor, rotate this specific shelf by 90 degrees
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.Rotate(0, 90, 0, Space.Self);
            Debug.Log($"[{gameObject.name}] Manually rotated 90 degrees.");
        }
    }
    
}