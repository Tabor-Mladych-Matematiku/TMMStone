using CardGame;

public class Velmi_otravný_robot : CardScriptBase
{
    public override bool Taunt => true;

    protected override void OnSelfSummoned(object sender, Minion.TargetedEventEventArgs e) =>
        ((Minion)sender).Shielded = true;
}
