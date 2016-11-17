using System.Collections.Generic;

public class CollectionUtil
{
    /// <summary>
    /// 交换引用
    /// </summary>
    public static void SwapDestroyObject<T>(List<T> list, int pos, int tail)
    {
        if (pos == tail)
            return;

        T t_loader = list[pos];
        list[pos] = list[tail];
        list[tail] = t_loader;
    }
}
