using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Multi;
using UnityEngine;
using UnityEngine.UI;

namespace Single.UI
{
    public class OutTurnPanelManager : MonoBehaviour
    {
        public Button RongButton;
        public Button ChowButton;
        public Button PongButton;
        public Button KongButton;
        public Button SkipButton;
        // private OutTurnOperation[] operations = null;

        public void SetOperations(OutTurnOperation[] operations)
        {
            // this.operations = operations;
            // only valid operation is skip: this should not happen
            if (operations.All(op => op.Type == OutTurnOperationType.Skip))
            {
                Debug.LogError("This logic should not be reached.");
                ClientBehaviour.Instance.OnSkipButtonClicked();
                return;
            }
            SkipButton.gameObject.SetActive(true);
            if (operations.Any(op => op.Type == OutTurnOperationType.Rong))
            {
                RongButton.onClick.RemoveAllListeners();
                RongButton.gameObject.SetActive(true);
                var rongOperation = System.Array.Find(operations, op => op.Type == OutTurnOperationType.Rong);
                RongButton.onClick.AddListener(() => ClientBehaviour.Instance.OnRongButtonClicked(rongOperation));
            }
            if (operations.Any(op => op.Type == OutTurnOperationType.Kong))
            {
                KongButton.onClick.RemoveAllListeners();
                KongButton.gameObject.SetActive(true);
                var kongOperation = System.Array.Find(operations, op => op.Type == OutTurnOperationType.Kong);
                KongButton.onClick.AddListener(() => ClientBehaviour.Instance.OnKongButtonClicked(kongOperation));
            }
            if (operations.Any(op => op.Type == OutTurnOperationType.Chow))
            {
                ChowButton.onClick.RemoveAllListeners();
                ChowButton.gameObject.SetActive(true);
                var chowOptions = System.Array.FindAll(operations, op => op.Type == OutTurnOperationType.Chow);
                ChowButton.onClick.AddListener(() => ClientBehaviour.Instance.OnChowButtonClicked(chowOptions, operations));
            }
            if (operations.Any(op => op.Type == OutTurnOperationType.Pong))
            {
                PongButton.onClick.RemoveAllListeners();
                PongButton.gameObject.SetActive(true);
                var pongOptions = System.Array.FindAll(operations, op => op.Type == OutTurnOperationType.Pong);
                PongButton.onClick.AddListener(() => ClientBehaviour.Instance.OnPongButtonClicked(pongOptions, operations));
            }
        }

        public void Disable()
        {
            RongButton.gameObject.SetActive(false);
            ChowButton.gameObject.SetActive(false);
            PongButton.gameObject.SetActive(false);
            KongButton.gameObject.SetActive(false);
            SkipButton.gameObject.SetActive(false);
        }
    }
}
