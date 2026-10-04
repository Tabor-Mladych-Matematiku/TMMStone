using CardGame;

public class Elda__Zaklínač_rostlin : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        // The played card already occupies its slot while its battlecry resolves.
        for (int i = 0; i < GameManager.maxMinionSlots; i++)
        {
            if (GameManager.Instance.SummonMinion(GetOwner(sender), 51) == null) break;
        }
    }
}
