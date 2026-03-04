using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;

namespace Project1.Core.Interactions
{
    class InteractionAssignVisitorRoomLogic : InteractionLogic
    {
        //int RoomID;
        //public InteractionAssignVisitorRoom()
        //{

        //}
        //public InteractionAssignVisitorRoom(int roomID)
        //{
        //    this.RoomID = roomID;
        //}
        //public override void Perform()
        //{
        //    var a = this.Actor;
        //    var t = this.Target;
        //    var roomOwner = t.Object as Actor;
        //    var room = a.Map.Town.RoomManager.GetRoom(this.RoomID);
        //    roomOwner.Possessions.Claim(room);
        //}
        //protected override void AddSaveData(SaveTag tag)
        //{
        //    this.RoomID.Save(tag, "RoomID");
        //}
        //public override void LoadData(SaveTag tag)
        //{
        //    tag.TryGetTagValue<int>("RoomID", ref this.RoomID);
        //}
        //protected override void WriteExtra(IDataWriter w)
        //{
        //    w.Write(this.RoomID);
        //}
        //protected override void ReadExtra(IDataReader r)
        //{
        //    this.RoomID = r.ReadInt32();
        //}
    }
}
