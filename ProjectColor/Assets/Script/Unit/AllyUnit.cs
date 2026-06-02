/// <summary>
/// 表示友方阵营单位，声明友方单位的原生阵营特征。
/// </summary>
public class AllyUnit : Unit
{
    protected override UnitTeam NativeTeam => UnitTeam.Ally;
}
