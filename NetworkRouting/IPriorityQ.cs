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
        private List<int> q;
        private double[] dist;
        private List<int> QindexOf;

        //O(|V|log|V|) - Does insert |V| times
        public void Makequeue(double[] dist)
        {
            this.dist = dist;
            QindexOf = new List<int>(dist.Length);
            q = new List<int>(dist.Length);
            q.Add(-1); //Have the first item in the array be garbage so that it's 1-indexed
            for (int v = 0; v < dist.Length; v++)
            {
                Insert(v);
            }
        }

        //O(log|V|) from BubbleUp. Other ops are O(1)
        public void Insert(int v)
        {
            QindexOf.Add(q.Count);
            q.Add(v);
            BubbleUp(v);
        }

        //O(log|V|) - I use a look up array for the Q-index, so that's O(1). 
        //There can be at most log|V| swaps (height of tree), and each swap does only O(1) ops.
        private void BubbleUp(int v)
        {
            int Qi = QindexOf[v]; //O(1)
            int parentQi = Qi / 2;

            while (Qi != 1 && dist[q[parentQi]] > dist[v]) //while not at root and while parent's key is greater than inserted node's key
            {
                q[Qi] = q[parentQi]; //put parent in child's place
                QindexOf[q[Qi]] = Qi; //update QindexOf parent to be child's old index
                Qi = parentQi; //increment current to parent's position
                parentQi = Qi / 2; //find parent of current's new position
            }
            q[Qi] = v; //put v in it's appropriate position
            QindexOf[v] = Qi; //update QindexOf the bubbledUp node
        }

        //O(log|V|) - O(1) ops except for siftdown function. 
        //Siftdown has same complexity as bubbleUp - O(log|V|) - for same reasons
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

                //sift the root down:
                int childQi = SmallestChildQi(currQi);
                while (childQi != 0 && dist[q[childQi]] < dist[lastV]) //while current has children and 
                                                                       //the distance of the smallest child < the distance of the previously last node
                {
                    q[currQi] = q[childQi]; //put smallest child at current position
                    QindexOf[q[currQi]] = currQi; //update QindexOf child to be parent's old index
                    currQi = childQi; //set current = smallest child
                    childQi = SmallestChildQi(currQi); //get new smallest child
                }

                q[currQi] = lastV; //put lastV in it's appropriate position
                QindexOf[lastV] = currQi;
            }
            //********** end siftdown ************//
            QindexOf[v] = -1;
            return v;
        }
        
        //O(1) - all O(1) lookups or arithmetic done one time.
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
                return (dist[q[c1]] < dist[q[c2]]) ? c1 : c2; //smallest child
        }

        //O(log|V|) - just calls BubbleUp
        public void OnKeyDecreased(int v)
        {
            BubbleUp(v);
        }

        //O(1)
        public bool NotEmpty()
        {
            return q.Count > 1; //q[0] is garbage, so q empty when 1 element in it
        }
    }




    public class ArrayQ : IPriorityQ
    {
        private List<int> q;
        private double[] dist;

        public ArrayQ() {}

        //O(|V|) - call insert |V| times
        public void Makequeue(double[] dist)
        {
            q = new List<int>(dist.Length);
            this.dist = dist;
            for (int v = 0; v < dist.Length; v++)
            {
                Insert(v);
            }
        }

        //O(1) - Don't need to ever resize my q array, so constant time insertions
        public void Insert(int v)
        {
            q.Add(v);
        }

        //O(|V|) - iterate over |V| and do simple O(1) operation
        public int Deletemin()
        {
            int minDistIndex = q[0];
            for(int i = 1; i < q.Count; i++) //O(|V|)
            {
                minDistIndex = (dist[q[i]] < dist[minDistIndex]) ? q[i] : minDistIndex;
            }

            q.Remove(minDistIndex); //O(|V|)
            return minDistIndex;
        }

        //O(1)
        public void OnKeyDecreased(int v) { return; }

        //O(1)
        public bool NotEmpty()
        {
            return q.Count > 0;
        }
    }
}
