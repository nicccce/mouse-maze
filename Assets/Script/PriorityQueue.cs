using System.Collections.Generic;
using UnityEngine;


// ���ڶ����ʵ�ֵ����ȶ��У�����ѣ�
public class PriorityQueue<T>
{
    private int _size;
    public int Size => _size; // ��ǰ����Ԫ������

    private int _capacity;
    public int Capacity => _capacity; // ��ǰ��������

    private T[] _elements; // �洢Ԫ�ص�����

    private readonly IComparer<T> _comparator; // �Ƚ���

    public bool IsEmpty => _size == 0; // �����Ƿ�Ϊ��
    private T Top => IsEmpty ? throw new System.InvalidOperationException("����Ϊ�գ��޷����ʶѶ�Ԫ��") : _elements[0];

    // ��ʼ�����ȶ���
    public PriorityQueue(IComparer<T> comparator, int capacity = 1)
    {
        _size = 0;
        _capacity = Mathf.Max(1, capacity);
        _elements = new T[_capacity];
        _comparator = comparator;
    }

    // ��Ӳ�������Ԫ�ؼ������ȶ���
    public void Enqueue(T element)
    {
        if (_size == _capacity)
        {
            ExpandCapacity(); // ����
        }

        _elements[_size] = element;
        HeapInsert(_size);
        _size++;
    }

    // ���Ӳ������Ƴ������ضѶ�Ԫ��
    public T Dequeue()
    {
        if (_size == 0)
        {
            throw new System.InvalidOperationException("����Ϊ�գ��޷�����");
        }

        T element = _elements[0];
        Swap(0, _size - 1); // ���Ѷ�Ԫ�ؽ�����ĩβ
        _size--;
        Heapify(0);
        return element;
    }

    // �鿴�Ѷ�Ԫ�أ����Ƴ���
    public T Peek()
    {
        return Top;
    }

    // <summary>
    // ������ȶ���
    // </summary>
    public void Clear()
    {
        _size = 0;
    }

    // �Ѳ�����������ϵ�����������ά���ѵ�����
    private void HeapInsert(int index)
    {
        while (index > 0 && _comparator.Compare(_elements[index], _elements[(index - 1) / 2]) > 0)
        {
            Swap(index, (index - 1) / 2);
            index = (index - 1) / 2;
        }
    }

    // �ѻ������µ�����������ά���ѵ�����
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

    // ���ݶ��У���չ�洢����
    private void ExpandCapacity()
    {
        _capacity = Mathf.CeilToInt(_capacity * 1.5f);
        T[] newElements = new T[_capacity];
        System.Array.Copy(_elements, newElements, _size);
        _elements = newElements;
    }

    // ��������������Ԫ�ص�λ��
    private void Swap(int i, int j)
    {
        (_elements[i], _elements[j]) = (_elements[j], _elements[i]);
    }
}
