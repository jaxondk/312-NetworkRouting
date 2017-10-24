using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkRouting
{
    public interface IPriorityQ
    {
        IPriorityQ Makequeue(double[] dist);
        void Insert(int v);
        int Deletemin();
        void OnKeyDecreased(int v); //Decreasekey method in book
        bool NotEmpty();
    }

    public class ArrayQ : IPriorityQ
    {
        List<int> q = new List<int>();

        public ArrayQ() {}

        public IPriorityQ Makequeue(double[] dist)
        {
            ArrayQ newQ = new ArrayQ();
            for(int i = 0; i < dist.Length; i++)
            {
                Insert(i);
            }
            return newQ;
        }

        public void Insert(int v)
        {
            q.Add(v);
        }

        public int Deletemin()
        {
            int min = q[0];
            for(int i = 1; i < q.Count; i++)
            {
                min = (q[i] < min) ? i : min;
            }

            q.RemoveAt(min);
            return min;
        }

        public void OnKeyDecreased(int v) { return; }

        public bool NotEmpty()
        {
            return q.Count > 0;
        }
    }
}
