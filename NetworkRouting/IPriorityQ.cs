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

    public class HeapQ : IPriorityQ
    {
        private List<int> q = new List<int>();
        private double[] dist;

        public void Makequeue(double[] dist) //O(|V|log|V|)
        {
            q.Add(-1); //Have the first item in the array be garbage so that it's 1-indexed
            this.dist = dist;
            for (int v = 0; v < dist.Length; v++)
            {
                Insert(v);
            }
        }

        //This is the bubbleUp function. O(log|V|)
        public void Insert(int v)
        {
            int Qi = q.Count; //q.Count will be the q-index of the inserted node
            q.Add(v);
            BubbleUp(v, Qi);
        }

        private void BubbleUp(int v, int Qi)
        {
            int parentQi = Qi / 2;

            while (Qi != 1 && dist[q[parentQi]] > dist[v]) //while not at root and while parent's key is greater than inserted node's key
            {
                q[Qi] = q[parentQi]; //put parent in child's place
                Qi = parentQi; //increment current to parent's position
                parentQi = Qi / 2; //find parent of current's new position
            }
            q[Qi] = v; //put v in it's appropriate position
        }

        public int Deletemin()
        {
            int v = q[1]; //remember, q is 1-indexed

            //******** siftdown function **********//
            int lastV = q.Last();
            q.Remove(lastV); //trim last leaf
            if (NotEmpty())
            {
                q[1] = lastV; //put last at root
                int currQi = 1;
                //sift the root down
                int childQi = SmallestChildQi(currQi);
                while (childQi != 0 && dist[q[childQi]] < dist[lastV]) //while current has children and the distance of the smallest child < the distance of the previously last node
                {
                    q[currQi] = q[childQi]; //put smallest child at current position
                    currQi = childQi; //set current = smallest child
                    childQi = SmallestChildQi(currQi); //get new smallest child
                }

                q[currQi] = lastV; //put lastV in it's appropriate position
            }
            //********** end siftdown ************//

            return v;
        }

        private int SmallestChildQi(int parent)
        {
            //Left child = parent index * 2 
            //Right child = left child + 1
            int c1 = 2 * parent;
            int c2 = c1 + 1;
            if (c1 >= q.Count)
                return 0; //no children
            else if (c2 >= q.Count)
                return c1; //only child
            else
                return (dist[q[c1]] < dist[q[c2]]) ? c1 : c2;
        }

        public void OnKeyDecreased(int v)
        {
            int i = q.IndexOf(v);
            BubbleUp(v, i);
        }

        public bool NotEmpty()
        {
            return q.Count > 1; //q[0] is garbage, so q empty when 1 element in it
        }
    }

    public class ArrayQ : IPriorityQ
    {
        private List<int> q = new List<int>();
        private double[] dist;

        public ArrayQ() {}

        public void Makequeue(double[] dist)
        {
            this.dist = dist;
            for (int v = 0; v < dist.Length; v++)
            {
                Insert(v);
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
