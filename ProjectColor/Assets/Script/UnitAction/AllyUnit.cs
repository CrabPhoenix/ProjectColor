/// <summary>
/// 表示友方阵营单位，声明友方单位的阵营特征。
/// </summary>
public class AllyUnit : Unit
{
    public override UnitTeam Team => UnitTeam.Ally;
}
