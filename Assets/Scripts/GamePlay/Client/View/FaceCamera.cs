using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Client.View
{
    public class FaceCamera : MonoBehaviour
    {
        public Camera cameraToFace;
        private void Update()
        {
            if (cameraToFace == null) cameraToFace = Camera.main;
            if (cameraToFace == null)
            {
                Debug.LogError("cameraToFace is null");
                return;
            }
            transform.LookAt(cameraToFace.transform);
        }
    }
}
