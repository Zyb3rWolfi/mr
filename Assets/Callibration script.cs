using UnityEngine;

public class CallbirationScript : MonoBehaviour
{
    [Header("Calibration Key")]
    [Tooltip("Pressing the primary button (X on left controller / A on right controller) will snap the level to you.")]
    [SerializeField] private OVRInput.Button _calibrationButton = OVRInput.Button.One;

    [Header("Tracking Target")]
    [SerializeField] private Transform _centerEyeAnchor; // Drag OVRCameraRig's CenterEyeAnchor here

    private void Update()
    {
        // Check if you press the calibration button on your Quest 3S controller
        if (OVRInput.GetDown(_calibrationButton))
        {
            CalibrateLevelPosition();
        }
    }

    public void CalibrateLevelPosition()
    {
        if (_centerEyeAnchor == null)
        {
            if (Camera.main != null) _centerEyeAnchor = Camera.main.transform;
        }

        if (_centerEyeAnchor != null)
        {
            // Calculate where you are physically standing relative to the glitched origin
            Vector3 playerFloorPos = _centerEyeAnchor.position;
            playerFloorPos.y = 0f; // Keep the level snapped flat to your lab floor

            // Instantly pull the entire level container to your exact physical position!
            transform.position = playerFloorPos;

            // Match the rotation so the rows align with the direction you are facing
            Vector3 forward = _centerEyeAnchor.forward;
            forward.y = 0f; // Prevent tilting
            transform.rotation = Quaternion.LookRotation(forward);

            Debug.Log("[Calibration Success] Virtual store level shifted and aligned to your real-world position!");
        }
    }
}