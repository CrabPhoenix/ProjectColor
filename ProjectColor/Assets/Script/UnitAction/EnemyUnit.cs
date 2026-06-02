/// <summary>
/// 表示敌方阵营单位，声明敌方单位的阵营特征。
/// </summary>
public class EnemyUnit : Unit
{
    public override UnitTeam Team => UnitTeam.Enemy;
}
