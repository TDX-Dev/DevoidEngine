using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Util
{
    internal class SkylinePacker
    {
        private readonly List<SkylineNode> nodes = [];

        private readonly int width;
        private readonly int height;

        public SkylinePacker(int width, int height)
        {
            this.width = width;
            this.height = height;

            nodes.Add(new SkylineNode
            {
                X = 0,
                Y = 0
            });
        }

        public bool TryPack(
            int width,
            int height,
            out int x,
            out int y)
        {
            x = 0;
            y = 0;

            if (width <= 0 || height <= 0 || width > this.width || height > this.height)
            {
                return false;
            }

            int bestY = int.MaxValue;
            int bestX = -1;
            int bestNodeIndex = -1;

            for (int i = 0; i < nodes.Count; i++)
            {
                int currentX = nodes[i].X;

                if (currentX + width > this.width)
                {
                    break;
                }

                int maxY = 0;
                for (int j = i; j < nodes.Count; j++)
                {
                    if (nodes[j].X >= currentX + width)
                    {
                        break;
                    }

                    if (nodes[j].Y > maxY)
                    {
                        maxY = nodes[j].Y;
                    }
                }

                if (maxY + height <= this.height && maxY < bestY)
                {
                    bestY = maxY;
                    bestX = currentX;
                    bestNodeIndex = i;
                }
            }

            if (bestNodeIndex == -1)
            {
                return false;
            }

            x = bestX;
            y = bestY;

            int newRight = x + width;
            int newY = y + height;

            int yAtRight = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].X <= newRight)
                {
                    yAtRight = nodes[i].Y;
                }
                else
                {
                    break;
                }
            }

            nodes.Insert(bestNodeIndex, new SkylineNode { X = x, Y = newY });

            int removeStart = bestNodeIndex + 1;
            while (removeStart < nodes.Count && nodes[removeStart].X < newRight)
            {
                nodes.RemoveAt(removeStart);
            }

            bool needsRightNode = true;
            if (removeStart < nodes.Count && nodes[removeStart].X == newRight)
            {
                needsRightNode = false; 
            }

            if (needsRightNode && newRight < this.width)
            {
                nodes.Insert(removeStart, new SkylineNode { X = newRight, Y = yAtRight });
            }

            for (int i = 0; i < nodes.Count - 1; i++)
            {
                if (nodes[i].Y == nodes[i + 1].Y)
                {
                    nodes.RemoveAt(i + 1);
                    i--;
                }
            }

            return true;
        }
    }
}
