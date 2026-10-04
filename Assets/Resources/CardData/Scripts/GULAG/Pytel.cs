using CardGame;

public class Pytel : CardScriptBase
{
    private int captiveCardID = -1;

    public void SetCaptive(int cardID) => captiveCardID = cardID;

    protected override void OnDeath(object sender, System.EventArgs e)
    {
        if (captiveCardID < 0) return;
        int releasedCardID = captiveCardID;
        captiveCardID = -1;//To ensure that resummoning and bouncing and such dont cause something to live inside
        GameManager.Instance.SummonMinion(GetOwner(sender).Other(), releasedCardID);
    }
}
