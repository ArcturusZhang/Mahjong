using System.Linq;
using GamePlay.Client.Controller;
using GamePlay.Server.Model;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Client.View
{
    public class InTurnPanelManager : MonoBehaviour
    {
        public Button TsumoButton;
        public Button RichiButton;
        public Button DrawButton;
        public Button KongButton;
        public Button BeiButton;
        public Button SkipButton;
        public Button BackButton;

        public void SetOperations(InTurnOperation[] operations)
        {
            if (operations.All(op => op.Type == InTurnOperationType.Discard))
            {
                Debug.Log("There no other operations than discard that can be taken.");
                ClientBehaviour.Instance.OnInTurnSkipButtonClicked();
                return;
            }
            SkipButton.onClick.RemoveAllListeners();
            SkipButton.gameObject.SetActive(true);
            SkipButton.onClick.AddListener(ClientBehaviour.Instance.OnInTurnSkipButtonClicked);
            BackButton.onClick.RemoveAllListeners();
            BackButton.gameObject.SetActive(false);
            if (operations.Any(op => op.Type == InTurnOperationType.Tsumo))
            {
                TsumoButton.onClick.RemoveAllListeners();
                TsumoButton.gameObject.SetActive(true);
                var tsumoOperation = System.Array.Find(operations, op => op.Type == InTurnOperationType.Tsumo);
                TsumoButton.onClick.AddListener(() => ClientBehaviour.Instance.OnTsumoButtonClicked(tsumoOperation));
            }
            if (operations.Any(op => op.Type == InTurnOperationType.Richi))
            {
                RichiButton.onClick.RemoveAllListeners();
                RichiButton.gameObject.SetActive(true);
                var richiOperation = System.Array.Find(operations, op => op.Type == InTurnOperationType.Richi);
                RichiButton.onClick.AddListener(() =>
                {
                    ClientBehaviour.Instance.OnRichiButtonClicked(richiOperation);
                    Close();
                    BackButton.gameObject.SetActive(true);
                    BackButton.onClick.AddListener(() => ClientBehaviour.Instance.OnInTurnBackButtonClicked(operations));
                });
            }
            if (operations.Any(op => op.Type == InTurnOperationType.Kong))
            {
                KongButton.onClick.RemoveAllListeners();
                KongButton.gameObject.SetActive(true);
                var kongOperations = System.Array.FindAll(operations, op => op.Type == InTurnOperationType.Kong);
                KongButton.onClick.AddListener(() =>
                {
                    ClientBehaviour.Instance.OnInTurnKongButtonClicked(kongOperations);
                    Close();
                    BackButton.onClick.AddListener(() => ClientBehaviour.Instance.OnInTurnBackButtonClicked(operations));
                });
            }
            if (operations.Any(op => op.Type == InTurnOperationType.RoundDraw))
            {
                DrawButton.onClick.RemoveAllListeners();
                DrawButton.gameObject.SetActive(true);
                var drawOperation = System.Array.Find(operations, op => op.Type == InTurnOperationType.RoundDraw);
                DrawButton.onClick.AddListener(() => ClientBehaviour.Instance.OnInTurnButtonClicked(drawOperation));
            }
            if (operations.Any(op => op.Type == InTurnOperationType.Bei))
            {
                BeiButton.onClick.RemoveAllListeners();
                BeiButton.gameObject.SetActive(true);
                var beiOperation = System.Array.Find(operations, op => op.Type == InTurnOperationType.Bei);
                BeiButton.onClick.AddListener(() => ClientBehaviour.Instance.OnInTurnButtonClicked(beiOperation));
            }
        }

        public void ShowBackButton()
        {
            BackButton.gameObject.SetActive(true);
        }

        public void Close()
        {
            TsumoButton.gameObject.SetActive(false);
            RichiButton.gameObject.SetActive(false);
            DrawButton.gameObject.SetActive(false);
            KongButton.gameObject.SetActive(false);
            BeiButton.gameObject.SetActive(false);
            SkipButton.gameObject.SetActive(false);
            BackButton.gameObject.SetActive(false);
        }
    }
}
