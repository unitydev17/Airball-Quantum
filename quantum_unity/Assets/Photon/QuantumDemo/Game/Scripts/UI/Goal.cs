using TMPro;
using UnityEngine;

namespace Photon.QuantumDemo.Game.Scripts.UI
{
    public class Goal : MonoBehaviour
    {
        [SerializeField] private TMP_Text _score1;
        [SerializeField] private TMP_Text _score2;

        public void SetScore(int score1, int score2)
        {
            _score1.text = score1.ToString();
            _score2.text = score2.ToString();
        }
    }
}