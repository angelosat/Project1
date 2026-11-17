using System.Linq;

namespace Project1.Core.Systems.ItemRoles;


//internal record struct ItemPrefEval(EntityRefId Item, (ItemRoleKey Key, int Score)[] Scores)
//{
//    public readonly int MaxScore => this.Scores.Length > 0 ? this.Scores.Max(r => r.Score) : 0;
//    public readonly int SumScore => this.Scores.Length > 0 ? this.Scores.Sum(r => r.Score) : 0;
//}

public record struct ItemEvaluation(EntityRefId Item, (ItemRoleDef Role, int Score)[] Roles)
{
    public readonly int MaxScore => Roles.Length > 0 ? Roles.Max(r => r.Score) : 0;
    public readonly int SumScore => Roles.Length > 0 ? Roles.Sum(r => r.Score) : 0;
}
