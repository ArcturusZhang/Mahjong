using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Multi;
using UnityEngine;
using UnityEngine.UI;

namespace Single.UI
{
    public class InTurnPanelManager : MonoBehaviour
    {
        public Button TsumoButton;
        public Button RichiButton;
        public Button KongButton;
        public Button SkipButton;

        public void SetOperations(InTurnOperation[] operations)
        {
            if (operations.All(op => op.Type == InTurnOperationType.Discard)) {
                Debug.Log("There no other operations than discard that can be taken.");
                ClientBehaviour.Instance.OnInTurnSkipButtonClicked();
                return;
            }
            SkipButton.onClick.RemoveAllListeners();
            SkipButton.gameObject.SetActive(true);
            SkipButton.onClick.AddListener(ClientBehaviour.Instance.OnInTurnSkipButtonClicked);
            if (operations.Any(op => op.Type == InTurnOperationType.Tsumo)) {
                TsumoButton.onClick.RemoveAllListeners();
                TsumoButton.gameObject.SetActive(true);
                var tsumoOperation = System.Array.Find(operations, op => op.Type == InTurnOperationType.Tsumo);
                TsumoButton.onClick.AddListener(() => ClientBehaviour.Instance.OnTsumoButtonClicked(tsumoOperation));
            }
            if (operations.Any(op => op.Type == InTurnOperationType.Richi)) {
                RichiButton.onClick.RemoveAllListeners();
                RichiButton.gameObject.SetActive(true);
                var richiOperation = System.Array.Find(operations, op => op.Type == InTurnOperationType.Richi);
                RichiButton.onClick.AddListener(() => ClientBehaviour.Instance.OnRichiButtonClicked(richiOperation, operations));
            }
            if (operations.Any(op => op.Type == InTurnOperationType.Kong)) {
                KongButton.onClick.RemoveAllListeners();
                KongButton.gameObject.SetActive(true);
                var kongOperations = System.Array.FindAll(operations, op => op.Type == InTurnOperationType.Kong);
                KongButton.onClick.AddListener(() => ClientBehaviour.Instance.OnInTurnKongButtonClicked(kongOperations, operations));
            }
            // todo -- bei button
        }

        public void Disable()
        {
            TsumoButton.gameObject.SetActive(false);
            RichiButton.gameObject.SetActive(false);
            KongButton.gameObject.SetActive(false);
            SkipButton.gameObject.SetActive(false);
        }
    }
}
