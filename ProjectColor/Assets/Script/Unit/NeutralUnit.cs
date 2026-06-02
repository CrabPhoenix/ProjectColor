/// <summary>
/// 表示中立阵营单位，声明中立单位的原生阵营特征。
/// </summary>
public class NeutralUnit : Unit
{
    protected override UnitTeam NativeTeam => UnitTeam.Neutral;
}
