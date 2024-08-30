using System.Collections.Generic;
using System;

/// <summary>
/// A*에 사용될 우선순위 큐
/// </summary>
/// <typeparam name="TElement"></typeparam>
/// <typeparam name="TPriority"></typeparam>
public class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
{
    private struct Node
    {
        public TElement element;
        public TPriority priority;

        public Node(TElement element, TPriority priority)
        {
            this.element = element;
            this.priority = priority;
        }
    }
    private List<Node> nodes;

    public PriorityQueue()
    {
        nodes = new List<Node>();
    }

    public int Count()
    {
        return nodes.Count;
    }


    public void Enqueue(TElement element, TPriority priority)
    {
        Node newNode = new Node(element, priority);

        nodes.Add(newNode);

        int index = nodes.Count - 1;

        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            Node parentNode = nodes[parentIndex];

            if (newNode.priority.CompareTo(parentNode.priority) < 0)
            {
                nodes[index] = parentNode;
                nodes[parentIndex] = newNode;
                index = parentIndex;
            }
            else
            {
                break;
            }
        }
    }

    public TElement Dequeue()
    {
        Node rootNode = nodes[0];

        Node lastNode = nodes[nodes.Count - 1];
        nodes[0] = lastNode;

        nodes.RemoveAt(nodes.Count - 1);

        int index = 0;
        while (index < nodes.Count)
        {
            int leftIndex = (index * 2) + 1;
            int rightIndex = (index * 2) + 2;

            if (rightIndex < nodes.Count)
            {
                int lessIndex;

                if (nodes[leftIndex].priority.CompareTo(nodes[rightIndex].priority) < 0)
                {
                    lessIndex = leftIndex;
                }
                else
                {
                    lessIndex = rightIndex;
                }
                Node lessNode = nodes[lessIndex];

                if (nodes[index].priority.CompareTo(nodes[lessIndex].priority) > 0)
                {
                    nodes[lessIndex] = lastNode;
                    nodes[index] = lessNode;
                    index = lessIndex;
                }
                else
                {
                    break;
                }
            }
            else if (leftIndex < nodes.Count)
            {
                Node leftNode = nodes[leftIndex];
                if (nodes[index].priority.CompareTo(nodes[leftIndex].priority) > 0)
                {
                    nodes[leftIndex] = lastNode;
                    nodes[index] = leftNode;
                    index = leftIndex;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        return rootNode.element;
    }
}