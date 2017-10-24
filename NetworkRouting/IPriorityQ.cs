using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkRouting
{
    public interface IPriorityQ
    {
        void Makequeue(double[] dist);
        void Insert(int v);
        int Deletemin();
        void OnKeyDecreased(int v); //Decreasekey method in book
        bool NotEmpty();
    }

    public class ArrayQ : IPriorityQ
    {
        private List<int> q = new List<int>();
        private double[] dist;

        public ArrayQ() {}

        public void Makequeue(double[] dist)
        {
            this.dist = dist;
            for(int i = 0; i < dist.Length; i++)
            {
                Insert(i);
            }
        }

        public void Insert(int v)
        {
            q.Add(v);
        }

        public int Deletemin()
        {
            int minDistIndex = q[0];
            for(int i = 1; i < q.Count; i++)
            {
                minDistIndex = (dist[q[i]] < dist[minDistIndex]) ? q[i] : minDistIndex;
            }

            q.Remove(minDistIndex);
            return minDistIndex;
        }

        public void OnKeyDecreased(int v) { return; }

        public bool NotEmpty()
        {
            return q.Count > 0;
        }
    }
}
