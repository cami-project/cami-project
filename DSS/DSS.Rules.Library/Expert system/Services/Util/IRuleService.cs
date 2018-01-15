using System;
namespace DSS.Rules.Library
{
    public interface IRuleService<T>
    {
        void UpdateFact(T val);
    }
}
