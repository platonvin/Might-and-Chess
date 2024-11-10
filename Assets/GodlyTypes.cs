using System;

public struct ivec2 {
    public int x;
    public int y;

    public static ivec2 operator *(ivec2 a, ivec2 b) => new ivec2(a.x * b.x, a.y * b.y);
    public static ivec2 operator +(ivec2 a, ivec2 b) => new ivec2(a.x + b.x, a.y + b.y);
    public static ivec2 operator -(ivec2 a, ivec2 b) => new ivec2(a.x - b.x, a.y - b.y);

    public static ivec2 operator *(ivec2 a, int b) => new ivec2(a.x * b, a.y * b);
    public static ivec2 operator *(int b, ivec2 a) => new ivec2(a.x * b, a.y * b);

    // public static ivec2 clamp(ivec2 value, ivec2 low, ivec2 high) => new ivec2(
    //     Math.Clamp(value.x, low.x, high.x),
    //     Math.Clamp(value.y, low.y, high.y)
    // );

    public ivec2 clamp(ivec2 low, ivec2 high) => new ivec2(
        Math.Clamp(x, low.x, high.x),
        Math.Clamp(y, low.y, high.y)
    );

    public ivec2(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public ivec2 Rotate(Rotation rotation) {
        switch (rotation) {
            case Rotation.NoRotation:
                return this;
            case Rotation.Rotate90:
                return new ivec2(-y, x); // 90-degree clockwise rotation
            case Rotation.Rotate180:
                return new ivec2(-x, -y); // 180-degree rotation
            case Rotation.Rotate270:
                return new ivec2(y, -x); // 270-degree clockwise rotation
            default:
                return this;
        }
    }

    public int dot(ivec2 other) => x * other.x + y * other.y;

    public double len() => Math.Sqrt(x * x + y * y);
    public double length() => len();
}

public struct ivec3 {
    public int x;
    public int y;
    public int z;

    public ivec3(int x, int y, int z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static ivec3 operator *(ivec3 a, ivec3 b) => new ivec3(a.x * b.x, a.y * b.y, a.z * b.z);
    public static ivec3 operator +(ivec3 a, ivec3 b) => new ivec3(a.x + b.x, a.y + b.y, a.z + b.z);
    public static ivec3 operator -(ivec3 a, ivec3 b) => new ivec3(a.x - b.x, a.y - b.y, a.z - b.z);

    public int dot(ivec3 other) => x * other.x + y * other.y + z * other.z;

    public double len() => Math.Sqrt(x * x + y * y + z * z);
    public double length() => len();

    public ivec3 cross(ivec3 other) => new ivec3(
        y * other.z - z * other.y,
        z * other.x - x * other.z,
        x * other.y - y * other.x
    );
}

public struct ivec4 {
    public int x;
    public int y;
    public int z;
    public int w;

    public ivec4(int x, int y, int z, int w) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public static ivec4 operator *(ivec4 a, ivec4 b) => new ivec4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
    public static ivec4 operator +(ivec4 a, ivec4 b) => new ivec4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
    public static ivec4 operator -(ivec4 a, ivec4 b) => new ivec4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);

    public int dot(ivec4 other) => x * other.x + y * other.y + z * other.z + w * other.w;

    public double len() => Math.Sqrt(x * x + y * y + z * z + w * w);
    public double length() => len();
}
