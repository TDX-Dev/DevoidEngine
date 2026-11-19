namespace DevoidEngine.Util
{
    internal class SkylinePacker
    {
        private readonly List<SkylineNode> _nodes = [];

        private readonly int _width;
        private readonly int _height;

        public SkylinePacker(int width, int height)
        {
            this._width = width;
            this._height = height;

            _nodes.Add(new SkylineNode
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

            if (width <= 0 || height <= 0 || width > this._width || height > this._height)
            {
                return false;
            }

            int bestY = int.MaxValue;
            int bestX = -1;
            int bestNodeIndex = -1;

            for (int i = 0; i < _nodes.Count; i++)
            {
                int currentX = _nodes[i].X;

                if (currentX + width > this._width)
                {
                    break;
                }

                int maxY = 0;
                for (int j = i; j < _nodes.Count; j++)
                {
                    if (_nodes[j].X >= currentX + width)
                    {
                        break;
                    }

                    if (_nodes[j].Y > maxY)
                    {
                        maxY = _nodes[j].Y;
                    }
                }

                if (maxY + height <= this._height && maxY < bestY)
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
            for (int i = 0; i < _nodes.Count; i++)
            {
                if (_nodes[i].X <= newRight)
                {
                    yAtRight = _nodes[i].Y;
                }
                else
                {
                    break;
                }
            }

            _nodes.Insert(bestNodeIndex, new SkylineNode { X = x, Y = newY });

            int removeStart = bestNodeIndex + 1;
            while (removeStart < _nodes.Count && _nodes[removeStart].X < newRight)
            {
                _nodes.RemoveAt(removeStart);
            }

            bool needsRightNode = true;
            if (removeStart < _nodes.Count && _nodes[removeStart].X == newRight)
            {
                needsRightNode = false;
            }

            if (needsRightNode && newRight < this._width)
            {
                _nodes.Insert(removeStart, new SkylineNode { X = newRight, Y = yAtRight });
            }

            for (int i = 0; i < _nodes.Count - 1; i++)
            {
                if (_nodes[i].Y == _nodes[i + 1].Y)
                {
                    _nodes.RemoveAt(i + 1);
                    i--;
                }
            }

            return true;
        }
    }
}
