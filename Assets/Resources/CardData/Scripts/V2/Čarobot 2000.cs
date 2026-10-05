using CardGame;

public class Čarobot_2000 : CardScriptBase
{
    protected override void OnSelfSummoned(object sender, Minion.TargetedEventEventArgs e) =>
        ((Minion)sender).Shielded = true;
}
