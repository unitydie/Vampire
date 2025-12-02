using System;
using TMPro;
using UnityEngine;

public class GameUiController : MonoBehaviour
{
    public static GameUiController Instance { get; private set; }
    [SerializeField] private TextMeshProUGUI _gemText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void GemCounter(int gemsCollected)
    {
        _gemText.text = gemsCollected.ToString();
    }
}
