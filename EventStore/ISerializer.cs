using System;
using System.Collections.Generic;
using System.Text;

namespace EventStore
{
    public interface ISerializer
    {
        string Serialize<T>(T objectToSerialize);
        T Deserialize<T>(string serializedObject);
    }
}
