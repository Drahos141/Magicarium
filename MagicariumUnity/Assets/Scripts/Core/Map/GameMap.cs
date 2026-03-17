using System;
using System.Collections.Generic;

namespace Magicarium.Map
{
    /// <summary>
    /// Represents the game map as a 2-D grid of <see cref="MapTile"/> objects.
    /// </summary>
    public class GameMap
    {
        private readonly MapTile[,] _tiles;

        public int Width { get; }
        public int Height { get; }

        public GameMap(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException("width");
            if (height <= 0) throw new ArgumentOutOfRangeException("height");

            Width = width;
            Height = height;
            _tiles = new MapTile[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    _tiles[x, y] = new MapTile(x, y);
        }

        public MapTile GetTile(int x, int y)
        {
            if (x < 0 || x >= Width) throw new ArgumentOutOfRangeException("x");
            if (y < 0 || y >= Height) throw new ArgumentOutOfRangeException("y");
            return _tiles[x, y];
        }

        public bool IsInBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        public IEnumerable<MapTile> GetNeighbours(int x, int y)
        {
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };
            for (int i = 0; i < 4; i++)
            {
                int nx = x + dx[i];
                int ny = y + dy[i];
                if (IsInBounds(nx, ny))
                    yield return _tiles[nx, ny];
            }
        }

        public IEnumerable<MapTile> AllTiles()
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    yield return _tiles[x, y];
        }
    }
}
