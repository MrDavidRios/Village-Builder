using UnityEngine;

// Replacement for Vector2Int, which was causing slowdowns in big loops due to x,y accessor overhead
// A struct is a collection of variables and functions, used to simplify code.
public struct Coord
{

    //x and y values of the coordinate.
    public readonly int x;
    public readonly int y;

    //Initializes the coordinate values of the coordinate when they are set.
    public Coord(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    //Returns the squared distance between two coordinate values (squared)
    public static float SqrDistance(Coord a, Coord b)
    {
        return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
    }

    //Returns the distance between two coordinate values (direct)
    public static float Distance(Coord a, Coord b)
    {
        return (float)System.Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
    }

    //Checks if two coordinate values are neighbors
    public static bool AreNeighbours(Coord a, Coord b)
    {
        return System.Math.Abs(a.x - b.x) <= 1 && System.Math.Abs(a.y - b.y) <= 1;
    }

    //The invalid coordinate value
    public static Coord invalid
    {
        get
        {
            return new Coord(-1, -1);
        }
    }

    //The 'up' coordinate direction
    public static Coord up
    {
        get
        {
            return new Coord(0, 1);
        }
    }

    //The 'down' coordinate direction
    public static Coord down
    {
        get
        {
            return new Coord(0, -1);
        }
    }

    //The 'left' coordinate direction
    public static Coord left
    {
        get
        {
            return new Coord(-1, 0);
        }
    }

    //The 'right' coordinate direction
    public static Coord right
    {
        get
        {
            return new Coord(1, 0);
        }
    }

    //Different operators, making sure that Coords can be added, subtracted, and checked against each other
    public static Coord operator +(Coord a, Coord b)
    {
        return new Coord(a.x + b.x, a.y + b.y);
    }

    public static Coord operator -(Coord a, Coord b)
    {
        return new Coord(a.x - b.x, a.y - b.y);
    }

    public static bool operator ==(Coord a, Coord b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Coord a, Coord b)
    {
        return a.x != b.x || a.y != b.y;
    }

    public static implicit operator Vector2(Coord v)
    {
        return new Vector2(v.x, v.y);
    }

    public static implicit operator Vector3(Coord v)
    {
        return new Vector3(v.x, 0, v.y);
    }

    public override bool Equals(object other)
    {
        return (Coord)other == this;
    }

    public override int GetHashCode()
    {
        return 0;
    }
}