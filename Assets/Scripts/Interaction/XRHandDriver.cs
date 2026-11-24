using UnityEngine;
using UnityEngine.XR;

namespace GoreSandbox
{
    /// <summary>
    /// Drives a hand transform from an XR controller node (position + rotation).
    /// Keeps the hand aligned with the real controller in VR.
    /// Falls back to leaving the transform as-is when no XR device is present.
    /// </summary>
    public class XRHandDriver : MonoBehaviour
    {
        public XRNode node = XRNode.LeftHand;

        void Update()
        {
            var dev = InputDevices.GetDeviceAtXRNode(node);
            if (!dev.isValid) return;

            if (dev.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos))
                transform.localPosition = pos;

            if (dev.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rot))
                transform.localRotation = rot;
        }
    }
}
