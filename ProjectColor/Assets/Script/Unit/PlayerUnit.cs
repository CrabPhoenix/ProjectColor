/// <summary>
/// 表示玩家阵营单位，声明玩家单位的原生阵营特征。
/// </summary>
public class PlayerUnit : Unit
{
    protected override UnitTeam NativeTeam => UnitTeam.Player;
    public override bool CanPlayerControl => true;
    public override bool UsesRandomAI => false;
}
