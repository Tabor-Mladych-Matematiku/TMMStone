using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI loadingtext;
    public string Text { get => loadingtext.text; set=> loadingtext.text=value; }
    public static LoadingScreen Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
}
