/// <summary>
/// 表示玩家阵营单位，声明玩家可控制的单位特征。
/// </summary>
public class PlayerUnit : Unit
{
    public override UnitTeam Team => UnitTeam.Player;
    public override bool CanPlayerControl => true;
    public override bool UsesRandomAI => false;
}
