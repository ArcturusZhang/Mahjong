using System.Linq;
using GamePlay.Client.Controller;
using GamePlay.Server.Model;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Client.View
{
    public class OutTurnPanelManager : MonoBehaviour
    {
        public Button RongButton;
        public Button ChowButton;
        public Button PongButton;
        public Button KongButton;
        public Button SkipButton;
        public Button BackButton;

        public void SetOperations(OutTurnOperation[] operations)
        {
            // only valid operation is skip: this should not happen
            if (operations.All(op => op.Type == OutTurnOperationType.Skip))
            {
                Debug.LogError("This logic should not be reached.");
                return;
            }
            SkipButton.onClick.RemoveAllListeners();
            SkipButton.gameObject.SetActive(true);
            var skipOperation = System.Array.Find(operations, op => op.Type == OutTurnOperationType.Skip);
            SkipButton.onClick.AddListener(() => ClientBehaviour.Instance.OnOutTurnButtonClicked(skipOperation));
            BackButton.onClick.RemoveAllListeners();
            BackButton.gameObject.SetActive(false);
            if (operations.Any(op => op.Type == OutTurnOperationType.Rong))
            {
                RongButton.onClick.RemoveAllListeners();
                RongButton.gameObject.SetActive(true);
                var rongOperation = System.Array.Find(operations, op => op.Type == OutTurnOperationType.Rong);
                RongButton.onClick.AddListener(() => ClientBehaviour.Instance.OnOutTurnButtonClicked(rongOperation));
            }
            if (operations.Any(op => op.Type == OutTurnOperationType.Kong))
            {
                KongButton.onClick.RemoveAllListeners();
                KongButton.gameObject.SetActive(true);
                var kongOperation = System.Array.Find(operations, op => op.Type == OutTurnOperationType.Kong);
                KongButton.onClick.AddListener(() => ClientBehaviour.Instance.OnOutTurnButtonClicked(kongOperation));
            }
            if (operations.Any(op => op.Type == OutTurnOperationType.Chow))
            {
                ChowButton.onClick.RemoveAllListeners();
                ChowButton.gameObject.SetActive(true);
                var chowOptions = System.Array.FindAll(operations, op => op.Type == OutTurnOperationType.Chow);
                ChowButton.onClick.AddListener(() =>
                {
                    ClientBehaviour.Instance.OnChowButtonClicked(chowOptions, operations);
                    Close();
                    BackButton.onClick.AddListener(() => ClientBehaviour.Instance.OnOutTurnBackButtonClicked(operations));
                });
            }
            if (operations.Any(op => op.Type == OutTurnOperationType.Pong))
            {
                PongButton.onClick.RemoveAllListeners();
                PongButton.gameObject.SetActive(true);
                var pongOptions = System.Array.FindAll(operations, op => op.Type == OutTurnOperationType.Pong);
                PongButton.onClick.AddListener(() =>
                {
                    ClientBehaviour.Instance.OnPongButtonClicked(pongOptions, operations);
                    Close();
                    BackButton.onClick.AddListener(() => ClientBehaviour.Instance.OnOutTurnBackButtonClicked(operations));
                });
            }
        }

        public void ShowBackButton()
        {
            BackButton.gameObject.SetActive(true);
        }

        public void Close()
        {
            RongButton.gameObject.SetActive(false);
            ChowButton.gameObject.SetActive(false);
            PongButton.gameObject.SetActive(false);
            KongButton.gameObject.SetActive(false);
            SkipButton.gameObject.SetActive(false);
            BackButton.gameObject.SetActive(false);
        }
    }
}
