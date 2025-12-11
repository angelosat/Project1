using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class BehaviorGrabTool : BehaviorSelector
    {
        public BehaviorGrabTool()
        {
            this.Children = new List<Behavior>(){
                    BehaviorHelper.ToolNotRequired(),
                    new BehaviorCustom()
                    {
                        FailCondition=(a)=>{
                            var tool = a.CurrentTask.GetTarget(TargetIndex.Tool);
                            var equipped = a.GetEquipmentSlot(GearType.Mainhand);
                            var result = tool.Object != null && equipped != tool.Object;
                            return result;
                        },
                        Label = "ToolAlreadyEquipped"
                    },
                    new BehaviorSequence(
                        new BehaviorSelector(
                            new BehaviorItemIsInInventory(TargetIndex.Tool),
                            new BehaviorGetAtNewNew(TargetIndex.Tool)),
                        new BehaviorInteractionNew(TargetIndex.Tool, new InteractionEquip())
                        )
            };
        }
    }
}
