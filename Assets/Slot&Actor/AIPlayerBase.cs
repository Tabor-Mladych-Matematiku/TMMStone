using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayerBase : MonoBehaviour
{
    public virtual void OnTurnStart()
    {
        Debug.Log("AIPlayerBase.OnTurnStart");
    }

    void Start()
    {
        
    }
}
