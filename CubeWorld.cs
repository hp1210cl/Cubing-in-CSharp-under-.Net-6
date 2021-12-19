using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Cubing
{
    internal class CubeWorld
    {
        Cube3D cube3d;

        public CubeWorld(Viewport3D viewport, MainWindow mainWindow)
        {

            WorldViewport = viewport;
            MainWindow = mainWindow;

            CubeModel3DGroup = new Model3DGroup();
            SenseLight = new AmbientLight(Colors.White);
            CubeModel3DGroup.Children.Add(SenseLight);
            WorldModelVisual3D = new ModelVisual3D();
            WorldModelVisual3D.Content = CubeModel3DGroup;
            WorldViewport.Children.Add(WorldModelVisual3D);

            WorldCamera = new PerspectiveCamera();
            // Specify where in the 3D scene the camera is.
            WorldCamera.Position = new Point3D(9, 9, 9);
            // Specify the direction that the camera is pointing.
            WorldCamera.LookDirection = new Point3D(0, 0, 0) - WorldCamera.Position;
            // Define camera's horizontal field of view in degrees.
            WorldCamera.FieldOfView = 60;
            WorldViewport.Camera = WorldCamera;

        }

        public void ClearContent()
        {
            CubeModel3DGroup = new Model3DGroup();
            SenseLight = new AmbientLight(Colors.White);
            CubeModel3DGroup.Children.Add(SenseLight);
            WorldModelVisual3D = new ModelVisual3D();
            WorldModelVisual3D.Content = CubeModel3DGroup;
            WorldViewport.Children.Clear();
            WorldViewport.Children.Add(WorldModelVisual3D);

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public AmbientLight SenseLight { get; set; }
        public Model3DGroup CubeModel3DGroup { get; internal set; }
        public ModelVisual3D WorldModelVisual3D { get; internal set; }
        public PerspectiveCamera WorldCamera { get; }
        public Viewport3D WorldViewport { get; internal set; }
        public MainWindow MainWindow { get; internal set; }
        public Cube3D Cube3D
        {
            get
            {
                return cube3d;
            }
            set
            {
                cube3d = value;
                WorldCamera.Position = new Point3D(2.5 * cube3d.Dimension, 2.5 * cube3d.Dimension, 2.5 * cube3d.Dimension);
            }
        }

        public void ReverseLook()
        {
            if (WorldCamera.Position.X > 0)
            {
                WorldCamera.Position = new Point3D( - 1.5 * Cube3D.Dimension,  - 1.5 * Cube3D.Dimension,  - 1.5 * Cube3D.Dimension);
            }
            else
            {
                WorldCamera.Position = new Point3D(  2.5 * Cube3D.Dimension,   2.5 * Cube3D.Dimension,   2.5 * Cube3D.Dimension);
            }
            // Specify the direction that the camera is pointing.
            WorldCamera.LookDirection = new Point3D(0, 0, 0) - WorldCamera.Position;
        }
    }
}
