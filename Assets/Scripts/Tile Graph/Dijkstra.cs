using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Tile_Graph
{
    class Dijkstra
    {
        
        //Dictionary<Node, Dictionary<Node, float>> TileGenerator.Graph.Connections = new Dictionary<char, Dictionary<char, int>>();

        

        /*public void add_vertex(char name, Dictionary<char, int> edges)
        {
            //TileGenerator.Graph.Connections[name] = edges;
        }

        public List<char> shortest_path(Node start, Node finish)
        {
            var previous = new Dictionary<char, char>();
            var distances = new Dictionary<char, int>();
            

            List<Node> path = null; //Switch to connections??

            //
            /*foreach (Connection connection in TileGenerator.Graph.Connections)
            {
                if (connection.No == start)
                {
                    distances[vertex.Key] = 0;
                }
                else
                {
                    distances[vertex.Key] = int.MaxValue;
                }

                TileGenerator.Graph.TileGenerator.Graph.Nodes.Add(vertex.Key);
            }

            while (TileGenerator.Graph.Nodes.Count != 0)
            {
                TileGenerator.Graph.Nodes.Sort((x, y) => distances[x] - distances[y]);

                var smallest = TileGenerator.Graph.Nodes[0];
                TileGenerator.Graph.Nodes.Remove(smallest);

                if (smallest == finish)
                {
                    path = new List<char>();
                    while (previous.ContainsKey(smallest))
                    {
                        path.Add(smallest);
                        smallest = previous[smallest];
                    }

                    break;
                }

                if (distances[smallest] == int.MaxValue)
                {
                    break;
                }

                foreach (var neighbor in TileGenerator.Graph.Connections[smallest])
                {
                    var alt = distances[smallest] + neighbor.Value;
                    if (alt < distances[neighbor.Key])
                    {
                        distances[neighbor.Key] = alt;
                        previous[neighbor.Key] = smallest;
                    }
                }
            }

            return path;
        }
        

        class MainClass
        {
            public static void Main(string[] args)
            {
                Graph g = new Graph();
                g.add_vertex('A', new Dictionary<char, int>() { { 'B', 7 }, { 'C', 8 } });
                g.add_vertex('B', new Dictionary<char, int>() { { 'A', 7 }, { 'F', 2 } });
                g.add_vertex('C', new Dictionary<char, int>() { { 'A', 8 }, { 'F', 6 }, { 'G', 4 } });
                g.add_vertex('D', new Dictionary<char, int>() { { 'F', 8 } });
                g.add_vertex('E', new Dictionary<char, int>() { { 'H', 1 } });
                g.add_vertex('F', new Dictionary<char, int>() { { 'B', 2 }, { 'C', 6 }, { 'D', 8 }, { 'G', 9 }, { 'H', 3 } });
                g.add_vertex('G', new Dictionary<char, int>() { { 'C', 4 }, { 'F', 9 } });
                g.add_vertex('H', new Dictionary<char, int>() { { 'E', 1 }, { 'F', 3 } });

                g.shortest_path('A', 'H').ForEach(x => Console.WriteLine(x));
            }
        }*/
    }
}
