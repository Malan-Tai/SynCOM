using UnityEngine;

public struct CoverPlane
{
    public Plane plane;
    public EnumCover cover;

    public bool IntersectsSegment(Vector3 start, Vector3 end)
    {
        return !plane.SameSide(start, end);
    }
}
