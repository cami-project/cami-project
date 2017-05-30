using System;
namespace DSS.RMQ
{
    public interface IWriteToBroker<T>
    {
        void Write(string msg);

    }
}
