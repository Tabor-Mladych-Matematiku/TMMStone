using CardGame;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

public class SafeZone : MonoBehaviour
{
    static PolygonCollider2D polygonCollider;
    private void Start()
    {
        polygonCollider = GetComponent<PolygonCollider2D>();
    }
    public static bool InSafeZone
    {
        get
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 pos = polygonCollider.ClosestPoint(mousePos);
            return math.abs(pos.x - mousePos.x)<1&& math.abs(pos.y - mousePos.y) < 1;
        }
    }
}
