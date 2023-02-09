#define BODY_INIT_PARAM(name) cbuffer name\
{\
    uint count;\
    uint seed;\
    float speed_limit;\
    float space_size;\
    float2 weight_range;\
    float2 velocity_range;\
};

struct Body
{
    float weight;
    float size;
};

struct BodyRender
{
    float3 pos;
    float3 color;
};

struct BodyVelocity
{
    float3 velocity;
};
