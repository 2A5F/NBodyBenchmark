struct random
{
    uint state;

    static random create_from_index(const uint index);

    static uint wang_hash(uint n);

    uint next_state();

    float next_float();
    float next_float(const float min, const float max);

    float3 next_float3();
    float3 next_float3(const float3 min, const float3 max);
};


static random random::create_from_index(const uint index)
{
    // ReSharper disable once CppLocalVariableMayBeConst
    random random = {random::wang_hash(index) + 62u};
    random.next_state();
    return random;
}

static uint random::wang_hash(uint n)
{
    n = n ^ 61u ^ n >> 16;
    n *= 9u;
    n = n ^ n >> 4;
    n *= 0x27d4eb2du;
    n = n ^ n >> 15;

    return n;
}

uint random::next_state()
{
    const uint t = state;
    state ^= state << 13;
    state ^= state >> 17;
    state ^= state << 5;
    return t;
}

float random::next_float()
{
    return asfloat(0x3f800000 | next_state() >> 9) - 1.0f;
}

float random::next_float(const float min, const float max)
{
    return next_float() * (max - min) + min;
}

float3 random::next_float3()
{
    return asfloat(0x3f800000 | uint3(next_state(), next_state(), next_state()) >> 9) - 1.0f;
}

float3 random::next_float3(const float3 min, const float3 max)
{
    return next_float3() * (max - min) + min;
}
