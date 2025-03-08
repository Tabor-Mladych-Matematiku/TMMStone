using System;

namespace CardGame
{

    public class Effect : TableActor
    {
        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}