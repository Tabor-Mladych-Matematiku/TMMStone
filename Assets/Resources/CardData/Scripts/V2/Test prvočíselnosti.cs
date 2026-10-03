using System.Linq;
using CardGame;

public class Test_prvočíselnosti : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        foreach (Minion minion in GameManager.Instance.AllMinions.Where(m => IsPrime(m.Health)).ToArray())
            minion.Death();
    }

    static bool IsPrime(int value)
    {
        if (value < 2) return false;
        for (int divisor = 2; divisor * divisor <= value; divisor++)
            if (value % divisor == 0) return false;
        return true;
    }
}
