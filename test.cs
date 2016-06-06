using System;
using System.Collections.Generic;


namespace main
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //输入迷宫矩阵行数
                int size = Convert.ToInt32(Console.ReadLine());
                if (size > 10 || size < 1)
                    return;
                //获取输入，构造迷宫矩阵
                List<List<int>> matrix = new List<List<int>>();
                if (!inputMatrix(size, ref matrix))
                    return;

                //检查矩阵首尾是否可达
                if (matrix[0][0] != 0 || matrix[size - 1][size - 1] != 0)
                {
                    Console.WriteLine(0);
                    return;
                }

                //构造图，进行最短路径寻路
                MatrixMap matrixMap = new MatrixMap(size, matrix);
                matrixMap.GenShortestPath(0, size * size - 1);
            }
            catch (Exception)
            {
                return;
            }
        }

        //获取输入，构造迷宫矩阵
        static bool inputMatrix(int size ,ref List<List<int>> matrix)
        {
            for (int i = 0; i < size; i++)
            {
                string input = Console.ReadLine();
                string[] row = input.Trim().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (row.Length != size)
                {
                    return false;
                }

                List<int> line = new List<int>();
                for (int j = 0; j < size; j++)
                {
                    line.Add(Convert.ToInt32(row[j]));
                }
                matrix.Add(line);
            }
            return true;
        }
    }


    // 图中的节点
    public class Node
    {
        public int x, y;
        public int value;

        public int size,no;
        public Node(int inX, int inY, int inValue, int inSize)
        {
            x = inX;
            y = inY;
            value = inValue;
            size = inSize; 
            no = GetNodeNo(inX, inY, size);
        }

        public int GetNodeNo(int inX, int inY, int inSize)
        {
            return inY * inSize + inX; 
        }

        public List<int> SearchNeighbor()
        {
            var neighborNos = new List<int>();
            //4个方向寻找-方向1
            int x2 = x + 1;
            int y2 = y;
            if (x2>=0 && x2<size && y2>=0 && y2 < size)
                neighborNos.Add(GetNodeNo(x2, y2, size));

            //4个方向寻找-方向2
            x2 = x;
            y2 = y+1;
            if (x2 >= 0 && x2 < size && y2 >= 0 && y2 < size)
                neighborNos.Add(GetNodeNo(x2, y2, size));

            //4个方向寻找-方向3
            x2 = x-1;
            y2 = y ;
            if (x2 >= 0 && x2 < size && y2 >= 0 && y2 < size)
                neighborNos.Add(GetNodeNo(x2, y2, size));

            //4个方向寻找-方向4
            x2 = x;
            y2 = y -1;
            if (x2 >= 0 && x2 < size && y2 >= 0 && y2 < size)
                neighborNos.Add(GetNodeNo(x2, y2, size));

            return neighborNos;
        }
    };

    // 图中带邻接表的节点
    public class EdgeNode
    {
        public Node node;
        public List<int> neighborNos = null;

        public EdgeNode(Node inNode, List<Node> nodes)
        {
            node = inNode;
            neighborNos = new List<int>();
            var nos = node.SearchNeighbor();
            foreach (int no in nos)
            {
                if (nodes[no].value == 0)
                    neighborNos.Add(no);
            }
        }

        static public int SearchEdgeNodeNo(List<EdgeNode> edgeNodes, int no)
        {
            int edgeNodeNo = -1;
            foreach (var edgeNode in edgeNodes)
            {
                edgeNodeNo++;
                if (edgeNode.node.no == no)
                    return edgeNodeNo;
            }
            return -1;
        }

    };

    // 图
    public class MatrixMap
    {
        const int MAX_SIZE= 10;             //矩阵最大行列数
        const int MAX_DISTANCE = 1000;      //路径最大距离，设一个不可能出现的较大的数   
        const int NULL_NODE = -1;           //无效的前趋节点
        const int INIT_NODE = -2;           //对起始点设置的前趋节点
        public int size;                    //实际输入的矩阵行列数，此处约定行数=列数
        public List<List<int>> martrix;     //输入的矩阵，值为0代表可以通过，值为1代表不可以通过  
        public List<Node> nodes = null;            //将矩阵里的每一个点记录为一个节点    
        public List<EdgeNode> edgeNodes = null;    //对每一个点记录其可以通过的相邻节点

        int[] distances = new int[MAX_SIZE * MAX_SIZE];    //记录每个节点的路径距离，用于最短寻路
        int[] preNodeNos = new int[MAX_SIZE * MAX_SIZE];   //记录每个节点的前一个节点，用于最短寻路

        public MatrixMap(int inSize, List<List<int>> inMatrix)
        {
            size = inSize;
            martrix = inMatrix;
        }

        public void GenShortestPath(int startNo, int endNo)
        {
            GenNodes();
            GenEdgeTable(startNo); 
            
            if(EdgeNode.SearchEdgeNodeNo(edgeNodes, endNo) <0)
            {
                Console.WriteLine(0);
                return;
            }

            Dijkstra(startNo, endNo);
            PrintShortestPath(startNo, endNo);
        }



        private void GenNodes()
        {
            nodes = new List<Node>();
            int y = -1;
            foreach (var line in martrix)
            {
                y++;
                int x = -1;
                foreach (var value in line)
                {
                    x++;
                    var node = new Node(x, y, value,size);
                    nodes.Add(node);
                }
            }
        }


        // 从起点开始用广度优先遍历方式建立相邻节点表，可覆盖从起点开始所有可达的节点，组成一张连接拓扑图
        private void GenEdgeTable(int startNo)
        {
            edgeNodes = new List<EdgeNode>();
            var totalNos = new List<int>();
            var newNos = new List<int>();
            newNos.Add(startNo);
            while (newNos.Count > 0)
            {
                var newNos2 = new List<int>();
                foreach (var no in newNos)
                {
                    var node = nodes[no];
                    if (node.value == 1) continue;
                    if (totalNos.Contains(node.no)) continue;

                    totalNos.Add(no);
                    var edgeNode = new EdgeNode(node, nodes);
                    edgeNodes.Add(edgeNode);
                    foreach (var newNo in edgeNode.neighborNos)
                    {
                        if (!totalNos.Contains(newNo) && !newNos.Contains(newNo) && !newNos2.Contains(newNo))
                            newNos2.Add(newNo);
                    }
                }
                newNos = newNos2;
            }
        }

        // Dijkstra最短路径算法
        private void Dijkstra(int startNo,int endNo)
        {
            var s = new List<int>();   //已找到最短路径的端点
            //var u = new List<int>();   //未找到最短路径的端点

            for (int i = 0; i < MAX_SIZE * MAX_SIZE; i++)
            {
                distances[i]= MAX_DISTANCE;   // 初始化为最长距离
                preNodeNos[i] = NULL_NODE;           // 初始化为无前趋节点
            }

            //初始化将第一个节点加入s,其余加入待定集合u
            int selectEdgeNodeNo = EdgeNode.SearchEdgeNodeNo(edgeNodes, startNo);
            s.Add(selectEdgeNodeNo);
            distances[selectEdgeNodeNo] = 0;        //修改距离
            preNodeNos[selectEdgeNodeNo] = INIT_NODE;     // 修改前趋节点

            if (edgeNodes.Count <= 1)
            {
                return;
            }

            // 当待定集合U里节点不为空时
            while (s.Count < edgeNodes.Count)
            {
                // 根据选定的新加入节点更新待定集合u中节点的距离和前趋节点
                foreach (var nodeNo in edgeNodes[selectEdgeNodeNo].neighborNos)
                {
                    int edgeNodeNo= EdgeNode.SearchEdgeNodeNo(edgeNodes, nodeNo);
                    if (s.Contains(edgeNodeNo))
                        continue;

                    // 如果经过新加入节点的新路比老路距离短
                    if (distances[edgeNodeNo]> distances[selectEdgeNodeNo] + 1)
                    {
                        distances[edgeNodeNo] = distances[selectEdgeNodeNo] + 1;   //修改距离
                        preNodeNos[edgeNodeNo] = selectEdgeNodeNo;           //修改前趋节点
                    }
                }

                // 选定新的S节点，挑选U里路径最短的
                int min = MAX_DISTANCE;
                int newNo = NULL_NODE;
                for (int i = 0; i < edgeNodes.Count; i++)
                {
                    if (s.Contains(i))
                       continue;
                    if (distances[i] < min)
                    {
                        min = distances[i];
                        newNo = i;
                    }
                }
                selectEdgeNodeNo = newNo;
                s.Add(selectEdgeNodeNo);

                // 如果选定节点是结束点，则提前结束算法
                if (edgeNodes[selectEdgeNodeNo].node.no == endNo)
                    return;
            }       
        }

        private void PrintShortestPath(int startNo, int endNo)
        {
            int endEdgeNodeNo = EdgeNode.SearchEdgeNodeNo(edgeNodes, endNo);

            // 未寻到路
            if (preNodeNos[endEdgeNodeNo] == NULL_NODE)
            {
                Console.WriteLine(0);
                return;
            }

            int distance = distances[endEdgeNodeNo];
            // 逆序寻找前趋节点
            var path = new List<int>();
            while(endEdgeNodeNo != INIT_NODE)
            {
                path.Insert(0, endEdgeNodeNo);
                endEdgeNodeNo = preNodeNos[endEdgeNodeNo];
            }

            // 修改矩阵
            foreach (var j in path)
            {
                int x = edgeNodes[j].node.x;
                int y = edgeNodes[j].node.y;
                martrix[y][x] = 2;
            }

            // 打印
            Console.WriteLine(distance);
            for (int i = 0; i < size; i++)
            {
                Console.WriteLine(string.Join(" ", martrix[i]));
            }
        }

    };


}
