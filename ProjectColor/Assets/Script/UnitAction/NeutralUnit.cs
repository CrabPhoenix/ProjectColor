/// <summary>
/// 表示中立阵营单位，声明中立单位的阵营特征。
/// </summary>
public class NeutralUnit : Unit
{
    public override UnitTeam Team => UnitTeam.Neutral;
}
