using UnityEngine;
using UnityEngine.XR;

namespace GoreSandbox
{
    /// <summary>
    /// Drives the camera transform from the XR headset (CenterEye).
    /// Keeps the camera aligned with the headset in VR.
    /// </summary>
    public class XRHeadDriver : MonoBehaviour
    {
        void Update()
        {
            var dev = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            if (!dev.isValid) return;

            if (dev.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos))
                transform.localPosition = pos;

            if (dev.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rot))
                transform.localRotation = rot;
        }
    }
}
