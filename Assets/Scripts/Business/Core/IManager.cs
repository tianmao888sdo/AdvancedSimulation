using System.Collections.Generic;
public interface IManager
{
    void ParseConfig<T>(List<string[]> lines);

    T GetConfigByID<T>(int id) where T : AbsConfig;
}