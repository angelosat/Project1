using Start_a_Town_;
using Start_a_Town_.Framework.AI.NodeTypes;
using System.Collections.Generic;

namespace Project1.Core.Towns
{
    class TaskBehaviorTavernWorkerTakeOrder : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var task = this.Plan;
            var customerIndex = TargetIndex.A;
            var customer = task.GetTarget(customerIndex);
            var shop = actor.Workplace as Tavern;
            var customerProps = shop.GetCustomer(customer) as CustomerTavern;

            // TODO wait until customer is seated
            yield return new BehaviorWait(() => customerProps.IsSeated);
            yield return BehaviorHelper.MoveTo(customerIndex);
            yield return new BehaviorCustom() { InitAction = () => customerProps.OrderTakenBy = actor };
        }
    }
}
