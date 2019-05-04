using System.Collections;
using System.Collections.Generic;
using Single.UI.Controller;
using UnityEngine;
using UnityEngine.UI;

namespace Single.UI.SubManagers
{
    public class PlayerPointTransferManager : MonoBehaviour
    {
        [SerializeField] private Text PlayerNameText;
        [SerializeField] private NumberPanelController PointController;
        // todo -- add a panel to show point change
        [SerializeField] private Image LeftArrow;
        [SerializeField] private Image StraightArrow;
        [SerializeField] private Image RightArrow;

        [HideInInspector] public string PlayerName;

        private void Update()
        {
            PlayerNameText.text = PlayerName;
        }

        public void SetTransfers(int point, IList<Transfer> transfers)
        {
            Debug.Log($"{name} is setting point to {point}, transfers are {string.Join(";", transfers)}");
            gameObject.SetActive(true);
            int total = 0;
            foreach (var transfer in transfers)
            {
                total += transfer.Amount;
                if (transfer.Type == Type.None || transfer.Amount >= 0) continue;
                switch (transfer.Type)
                {
                    case Type.Left:
                        LeftArrow.gameObject.SetActive(true);
                        break;
                    case Type.Straight:
                        StraightArrow.gameObject.SetActive(true);
                        break;
                    case Type.Right:
                        RightArrow.gameObject.SetActive(true);
                        break;
                }
            }
            PointController.SetNumber(point + total);
        }

        private void OnDisable()
        {
            LeftArrow.gameObject.SetActive(false);
            StraightArrow.gameObject.SetActive(false);
            RightArrow.gameObject.SetActive(false);
        }

        public enum Type
        {
            Left, Straight, Right, None
        }

        public struct Transfer
        {
            public Type Type;
            public int Amount;

            public override string ToString()
            {
                return $"Type: {Type}, Amount: {Amount}";
            }
        }
    }
}
