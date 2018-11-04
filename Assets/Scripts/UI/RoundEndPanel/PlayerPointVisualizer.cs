using System;
using Single;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.RoundEndPanel
{
    // todo -- this needs rework
    public class PlayerPointVisualizer : MonoBehaviour
    {
        public Image Minus;
        public Image[] Points;

        public void SetPoint(int point)
        {
            Minus.gameObject.SetActive(point < 0);
//            Points.SetNumber(Math.Abs(point), ResourceManager.Instance.BaseNumber);
        }
    }
}