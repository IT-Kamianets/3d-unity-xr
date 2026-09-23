using TMPro;
using UnityEngine;

namespace ITKamianets.Engine.XR
{
    /// <summary>World-space status text. Generic display primitive -- has no idea what it's reporting on.</summary>
    public class StatusHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        public void SetStatus(string message)
        {
            if (text != null)
            {
                text.text = message;
            }
        }
    }
}
