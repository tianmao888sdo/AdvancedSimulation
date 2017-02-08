using System;
using System.Collections.Generic;

/// <summary>
/// 队列管理器，执行按队列处理的任何任务
/// 待修改
/// </summary>
public class StackManager
{
    public delegate void Delegate(params string[] args);

    public struct Task
    {
        public Delegate func;
        public object[] args;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="func">要执行的方法</param>
        /// <param name="args">执行的方法参数列表</param>
        public Task(Delegate func,params object[] args)
        {
            this.func = func;
            this.args = args;
          
        }
    }

    private Dictionary<string,List<Task>> m_stacks = new Dictionary<string, List<Task>>();

    /// <summary>
    /// 添加任务
    /// </summary>
    /// <param name="name">任务所属名</param>
    /// <param name="task"></param>
    public void Add(string name,Task task)
    {
        List<Task> t_stack = null;

        if(!m_stacks.TryGetValue(name,out t_stack))
        {
            t_stack = new List<Task>();
            m_stacks.Add(name, t_stack);
        }

        t_stack.Add(task);
    }

    /// <summary>
    /// 删除任务
    /// </summary>
    /// <param name="name">任务所属名</param>
    /// <param name="task"></param>
    public void Remove(string name, Task task)
    {
        List<Task> t_stack = null;

        if (m_stacks.TryGetValue(name, out t_stack))
        {
            t_stack.Remove(task);

            if (t_stack.Count == 0)
                m_stacks.Remove(name);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Clear()
    {
        foreach(var item in m_stacks)
        {
            item.Value.Clear();
        }

        m_stacks.Clear();
    }

    /// <summary>
    /// 执行一次处理
    /// </summary>
    public void ExecuteNext()
    {
        string[] keys = new string[m_stacks.Count];
        m_stacks.Keys.CopyTo(keys, 0);//效率为每100个数据拷贝时间为0.4355毫秒，可以接受

        for (int i = 0; i < keys.Length; i++)
        {
            if (m_stacks[keys[i]].Count == 0)
            {
                m_stacks.Remove(keys[i]);
                continue;
            }

            Task t = m_stacks[keys[i]][0];
            t.func.DynamicInvoke(t.args);
            m_stacks[keys[i]].RemoveAt(0);
        }
	}
}
