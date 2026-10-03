using System;
using UnityEngine;
using UnityEngine.UI;
namespace CardGame
{
    public interface IHandRevealProvider
    {
        bool RevealsHands { get; }
    }

    public class Field : TableActor
    {

    }
}
