using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.UI
{
    public sealed class TabNavigationView : MonoBehaviour
    {
        [SerializeField] private Button[] tabButtons;
        [SerializeField] private Image[] tabBackgrounds;
        [SerializeField] private TMP_Text[] tabLabels;
        [SerializeField] private GameObject[] pages;

        public event Action<int> TabChanged;
        public int SelectedIndex { get; private set; } = -1;

        public void Bind(Button[] buttons, Image[] backgrounds, TMP_Text[] labels)
        {
            tabButtons = buttons;
            tabBackgrounds = backgrounds;
            tabLabels = labels;
        }

        private void Awake()
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                int index = i;
                tabButtons[i].onClick.AddListener(() => Select(index));
            }
            Select(0);
        }

        public void SetLabels(params string[] labels)
        {
            for (int i = 0; i < tabLabels.Length && i < labels.Length; i++) tabLabels[i].text = labels[i];
        }

        public void Select(int index)
        {
            if (tabButtons == null || index < 0 || index >= tabButtons.Length || index == SelectedIndex) return;

            SelectedIndex = index;
            for (int i = 0; i < tabBackgrounds.Length; i++)
            {
                tabBackgrounds[i].color = i == index ? UIColors.MilitaryGreen : UIColors.Panel;
            }
            if (pages != null)
            {
                for (int i = 0; i < pages.Length; i++)
                {
                    if (pages[i] != null) pages[i].SetActive(i == index);
                }
            }
            TabChanged?.Invoke(index);
        }
    }
}
