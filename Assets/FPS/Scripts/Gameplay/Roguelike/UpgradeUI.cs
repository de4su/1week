using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace Unity.FPS.Roguelike
{
    public class UpgradeUI : MonoBehaviour
    {
        public GameObject UIContainer;
        public GameObject UpgradeCardPrefab;
        public Transform CardsParent;

        public void ShowOptions(List<UpgradeData> upgrades)
        {
            UIContainer.SetActive(true);
            foreach (Transform child in CardsParent)
            {
                Destroy(child.gameObject);
            }

            foreach (var upgrade in upgrades)
            {
                GameObject card = Instantiate(UpgradeCardPrefab, CardsParent);
                // Setup card UI (assuming it has a script or we find components)
                card.GetComponentInChildren<TextMeshProUGUI>().text = upgrade.Title + "\n" + upgrade.Description;
                Button btn = card.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => {
                    Debug.Log("[Roguelike] Button clicked for: " + upgrade.Title);
                    UpgradeManager.Instance.SelectUpgrade(upgrade);
                    UIContainer.SetActive(false);
                });
}
}
    }
}
