namespace libmtrack.Audio;

public static class MathHelper
{
    public static short Lerp(short a, short b, float m) => (short) (m * (b - a) + a);

    public static float Clamp(float value, float min, float max) => value <= min ? min : value >= max ? max : value;
}