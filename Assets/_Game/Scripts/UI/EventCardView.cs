using System;
using Deadzone.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.UI
{
    public sealed class EventCardView : MonoBehaviour
    {
        public enum Status { Upcoming, Active, Completed, RewardAvailable, Expired }

        [SerializeField] private Image banner;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text timeText;

        public void Bind(Image bannerImage, TMP_Text title, TMP_Text status, TMP_Text time)
        {
            banner = bannerImage;
            titleText = title;
            statusText = status;
            timeText = time;
        }

        public void Set(string title, Status status, TimeSpan remaining)
        {
            titleText.text = title;
            statusText.text = Loc.T("event.status." + status.ToString().ToLowerInvariant());
            statusText.color = ColorOf(status);
            banner.color = UIColors.WithAlpha(ColorOf(status), 0.45f);
            timeText.text = status == Status.Active || status == Status.Upcoming ? NumberFormatter.Duration(remaining) : string.Empty;
        }

        private static Color ColorOf(Status status)
        {
            switch (status)
            {
                case Status.Active: return UIColors.Success;
                case Status.Upcoming: return UIColors.Info;
                case Status.RewardAvailable: return UIColors.Gold;
                case Status.Expired: return UIColors.Danger;
                default: return UIColors.Silver;
            }
        }
    }
}
