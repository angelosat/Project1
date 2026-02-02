using System.Collections.Generic;

namespace Start_a_Town_
{
    public interface IBlockEntityCompContainer
    {
        ICollection<IBlockEntityComp> Comps { get; }
        T GetComp<T>() where T : class, IBlockEntityComp;
        bool HasComp<T>() where T : class, IBlockEntityComp;
    }
    public interface IEntityCompContainer<U> where U : class, IBlockEntityComp
    {
        //ICollection<U> Comps { get; }
        T GetComp<T>() where T : class, IBlockEntityComp;
        bool HasComp<T>() where T : class, IBlockEntityComp;
    }
}
