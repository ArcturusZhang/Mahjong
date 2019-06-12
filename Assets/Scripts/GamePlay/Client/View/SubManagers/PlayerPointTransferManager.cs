using System.Collections;
using System.Collections.Generic;
using GamePlay.Client.Controller;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Client.View.SubManagers
{
    public class PlayerPointTransferManager : MonoBehaviour
    {
        [SerializeField] private Text PlayerNameText;
        [SerializeField] private NumberPanelController PointController;
        [SerializeField] private NumberChangeController ChangeController;
        [SerializeField] private PlayerPlaceController PlaceController;
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
            int totalTransfer = 0;
            foreach (var transfer in transfers)
            {
                totalTransfer += transfer.Amount;
                SetArrow(transfer);
            }
            StartCoroutine(SetAnimation(point, totalTransfer));
        }

        public void SetPlace(int place)
        {
            PlaceController.SetPlace(place);
        }

        public void ShowPlace()
        {
            PlaceController.Show();
        }

        private void SetArrow(Transfer transfer)
        {
            if (transfer.Type == Type.None || transfer.Amount >= 0) return;
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

        private const float Duration = 1.5f;
        private const int Ticks = 50;

        private IEnumerator SetAnimation(int point, int totalTransfer)
        {
            var waiting = new WaitForSeconds(Duration / Ticks);
            int gap = totalTransfer / Ticks;
            int currentPoint = point - totalTransfer;
            for (int tick = 0; tick <= Ticks; tick++)
            {
                PointController.SetNumber(currentPoint);
                ChangeController.SetNumber(point - currentPoint);
                currentPoint += gap;
                yield return waiting;
            }
            PointController.SetNumber(point);
            ChangeController.Close();
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
