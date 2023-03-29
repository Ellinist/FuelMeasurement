using SharpDX;
using System.Windows.Media.Media3D;

namespace FuelMeasurement.Client.UIModule.Infrastructure
{
    public static class RenderingConstants
    {
        public const string SwitchViewerManipulationModeHeader = "Режим редактирования";
        public const string SwitchViewerViewModeHeader = "Режим просмотра";
        public static Vector3 HitTestDownVector = new(0, -10000, 0);
        public static Vector3 HitTestUpVector = new(0, 10000, 0);

        public static int DefaultCameraAnimationTime = 500;
        public static double DefaultFarPlaneDistance = 100000;
        public static double DefaultNearPlaneDistance = 0.1f;

        public static Point3D ManipulationModeCameraPosition = new(0, 2, 0);
        public static Vector3D ManipulationModeCameraUpDirection = new(-1, 1, 0);
        public static Vector3D ManipulationModeCameraLookDirection = new(0, -1, 0);

        public static Point3D ViewModeCameraPosition = new(0.5, 5, 0);
        public static Vector3D ViewModeCameraUpDirection = new(-1, 0.5, 0);
        public static Vector3D ViewModeCameraLookDirection = new(-1, -2, -0);
    }
}
