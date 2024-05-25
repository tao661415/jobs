using System;
using System.Collections.Generic;

public static class ListExt
{
    /// <summary>
    /// 无GC版本的AddRange，将一个列表中的元素添加到另一个列表中
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="list">目标列表</param>
    /// <param name="collection">要添加的元素列表</param>
    public static void AddRangeNonAlloc<T>(this IList<T> list, IList<T> collection)
    {
        if (collection == null)
            return;
        
        for (int i = 0; i < collection.Count; i++)
        {
            list.Add(collection[i]);
        }
    }

    /// <summary>
    /// 有序插入，将一个元素插入到已经排序好的列表中
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="list">已排序列表</param>
    /// <param name="element">要插入的元素</param>
    /// <returns>插入的位置索引</returns>
    public static int OrderedInsert<T>(this IList<T> list, T element) where T : IComparable<T>
    {
        if (list.Count == 0)
        {
            list.Add(element);
            return 0;
        }

        int start = 0;
        int end = list.Count - 1;
        int index = list.Count;
        while (start < end)
        {
            index = (start + end) / 2;
            int curr = list[index].CompareTo(element);
            int next = list[index + 1].CompareTo(element);
            if (curr > 0)
            {
                end = index - 1;
                continue;
            }
            if (next <= 0)
            {
                start = index + 1;
                continue;
            }

            // 找到位置了
            list.Insert(index + 1, element);
            return index + 1;
        }

        if (start == end)
        {
            index = list[start].CompareTo(element) <= 0 ? start + 1 : start;
        }

        list.Insert(index, element);
        return index;
    }

    /// <summary>
    /// 从列表中移除并返回第一个元素，模拟队列的行为
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="list">列表</param>
    /// <returns>被移除的第一个元素</returns>
    public static T Dequeue<T>(this IList<T> list)
    {
        T element = list[0];
        list.RemoveAt(0);
        return element;
    }
}