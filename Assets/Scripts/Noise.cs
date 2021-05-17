public class Noise
{

  public static uint Get1dNoise(int position, uint seed)
  {
    const uint BIT_NOISE1 = 0xB5297A4D;
    const uint BIT_NOISE2 = 0x68E31DA4;
    const uint BIT_NOISE3 = 0x1B56C4E9;

    uint mangled = (uint)position;
    mangled *= BIT_NOISE1;
    mangled += seed;
    mangled ^= (mangled >> 8);
    mangled += BIT_NOISE2;
    mangled ^= (mangled << 8);
    mangled *= BIT_NOISE3;
    mangled ^= (mangled >> 8);
    return mangled;
  }

  public static uint Get2dNoise(int posX, int posY, uint seed)
  {
    const int PRIME_NUMBER = 198491317;
    return Get1dNoise(posX + (PRIME_NUMBER * posY), seed);
  }

  public static uint Get3dNoise(int posX, int posY, int posZ, uint seed)
  {
    const int PRIME1 = 198491317;
    const int PRIME2 = 6542989;
    return Get1dNoise(posX + (PRIME1 * posY) + (PRIME2 * posZ), seed);
  }
}