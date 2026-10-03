using CardGame;

public class Implementace_v_Pythonu : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e) =>
        GameManager.Instance.SummonMinion(GetOwner(sender), 314);
}
