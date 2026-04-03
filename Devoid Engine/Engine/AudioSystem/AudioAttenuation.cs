namespace DevoidEngine.Engine.AudioSystem
{
    public enum AudioAttenuation
    {
        // No attenuation
        NoAttenuation = 0,
        // Inverse distance attenuation model
        InverseDistance = 1,
        // Linear distance attenuation model
        LinearDistance = 2,
        // Exponential distance attenuation model
        ExponentialDistance = 3
    };
}