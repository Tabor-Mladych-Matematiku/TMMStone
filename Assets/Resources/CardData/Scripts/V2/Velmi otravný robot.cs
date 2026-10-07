using CardGame;

public class Velmi_otravný_robot : CardScriptBase
{
    public override bool Taunt => true;

    protected override void OnSelfSummoned(object sender, Minion.SummonedEventArgs e) =>
        ((Minion)sender).Shielded = true;
}
