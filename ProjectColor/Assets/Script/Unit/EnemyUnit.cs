/// <summary>
/// 表示敌方阵营单位，声明敌方单位的原生阵营特征。
/// </summary>
public class EnemyUnit : Unit
{
    protected override UnitTeam NativeTeam => UnitTeam.Enemy;
}
