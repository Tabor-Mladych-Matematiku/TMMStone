using CardGame;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultsLabel : MonoBehaviour
{
    void Start()
    {
        GetComponentInChildren<TextMeshProUGUI>().text = MatchResults.result;
    }
}
