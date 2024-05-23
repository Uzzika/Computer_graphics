#version 430

uniform vec3 uCamera;  // Объявляем uniform переменную для позиции камеры
uniform float aspect;  // Объявляем uniform переменную для соотношения сторон

out vec4 fragColor;

/*** DATA STRUCTURES ***/
struct SCamera
{
    vec3 Position;
    vec3 View;
    vec3 Up;
    vec3 Side;
    vec2 Scale;
};

struct SRay
{
    vec3 Origin;
    vec3 Direction;
};

SCamera initializeDefaultCamera()
{
    SCamera camera;
    camera.Position = vec3(0.0, 0.0, -8.0);
    camera.View = vec3(0.0, 0.0, 1.0);
    camera.Up = vec3(0.0, 1.0, 0.0);
    camera.Side = vec3(1.0, 0.0, 0.0);
    camera.Scale = vec2(1.0);
    return camera;
}

SRay GenerateRay(SCamera uCamera)
{
    vec2 coords = (gl_FragCoord.xy / vec2(800, 600)) * 2.0 - 1.0; // Нормализация координат
    coords.x *= aspect;
    vec3 direction = uCamera.View + uCamera.Side * coords.x + uCamera.Up * coords.y;
    return SRay(uCamera.Position, normalize(direction));
}

void main()
{
    SCamera camera = initializeDefaultCamera();
    SRay ray = GenerateRay(camera);
    fragColor = vec4(abs(ray.Direction.xy), 0.0, 1.0);
}
