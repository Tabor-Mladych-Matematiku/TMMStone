using CardGame;

public class Robůtek_z_plastové_stavebnice : CardScriptBase
{
    public override bool OnBeforePlayed(CardPlayContext context)
    {
        CardChoiceRequest request = new() { Header = "Jak Robůtka vylepšíš?" };
        request.Options.Add(new(0, "+2 Útoku"));
        request.Options.Add(new(1, "+2 života"));
        return context.TrySelectChoices(request, out _);
    }

    protected override void OnSelfSummoned(object sender, Minion.SummonedEventArgs e)
    {//This happens only if its Played because otherwise there was no choice done. Has to be OnSelfSummoned because the minion is buffing itself 
        if (e.Choices.Count != 1) return;
        Minion self = (Minion)sender;
        if (e.Choices[0] == 0) self.Buff(2, 0);
        else if (e.Choices[0] == 1) self.Buff(0, 2);
    }
}
