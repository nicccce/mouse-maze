using System.Collections.Generic;
using UnityEngine;


// 基于二叉堆实现的优先队列（大根堆）
public class PriorityQueue<T>
{
    private int _size;
    public int Size => _size; // 当前队列元素数量

    private int _capacity;
    public int Capacity => _capacity; // 当前队列容量

    private T[] _elements; // 存储元素的数组

    private readonly IComparer<T> _comparator; // 比较器

    public bool IsEmpty => _size == 0; // 队列是否为空
    private T Top => IsEmpty ? throw new System.InvalidOperationException("队列为空，无法访问堆顶元素") : _elements[0];

    // 初始化优先队列
    public PriorityQueue(IComparer<T> comparator, int capacity = 1)
    {
        _size = 0;
        _capacity = Mathf.Max(1, capacity);
        _elements = new T[_capacity];
        _comparator = comparator;
    }

    // 入队操作，将元素加入优先队列
    public void Enqueue(T element)
    {
        if (_size == _capacity)
        {
            ExpandCapacity(); // 扩容
        }

        _elements[_size] = element;
        HeapInsert(_size);
        _size++;
    }

    // 出队操作，移除并返回堆顶元素
    public T Dequeue()
    {
        if (_size == 0)
        {
            throw new System.InvalidOperationException("队列为空，无法出队");
        }

        T element = _elements[0];
        Swap(0, _size - 1); // 将堆顶元素交换到末尾
        _size--;
        Heapify(0);
        return element;
    }

    // 查看堆顶元素（不移除）
    public T Peek()
    {
        return Top;
    }

    // <summary>
    // 清空优先队列
    // </summary>
    public void Clear()
    {
        _size = 0;
    }

    // 堆插入调整（向上调整），用于维护堆的性质
    private void HeapInsert(int index)
    {
        while (index > 0 && _comparator.Compare(_elements[index], _elements[(index - 1) / 2]) > 0)
        {
            Swap(index, (index - 1) / 2);
            index = (index - 1) / 2;
        }
    }

    // 堆化（向下调整），用于维护堆的性质
    private void Heapify(int index)
    {
        int left = index * 2 + 1;
        while (left < _size)
        {
            int largest = (left + 1 < _size && _comparator.Compare(_elements[left + 1], _elements[left]) > 0) ? left + 1 : left;
            largest = _comparator.Compare(_elements[largest], _elements[index]) > 0 ? largest : index;
            if (largest == index)
            {
                break;
            }
            Swap(index, largest);
            index = largest;
            left = index * 2 + 1;
        }
    }

    // 扩容队列，扩展存储数组
    private void ExpandCapacity()
    {
        _capacity = Mathf.CeilToInt(_capacity * 1.5f);
        T[] newElements = new T[_capacity];
        System.Array.Copy(_elements, newElements, _size);
        _elements = newElements;
    }

    // 交换数组中两个元素的位置
    private void Swap(int i, int j)
    {
        (_elements[i], _elements[j]) = (_elements[j], _elements[i]);
    }
}
