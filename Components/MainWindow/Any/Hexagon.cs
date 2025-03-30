using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Launcher.Components.MainWindow.Any
{
    public class Hexagon : Shape
    {
        protected override Geometry DefiningGeometry
        {
            get
            {
                double width = Width;
                double height = Height;
                double radius = Math.Min(width, height) / 2;
                Point center = new Point(width / 2, height / 2);
                var geometry = new StreamGeometry();

                using (var ctx = geometry.Open())
                {
                    for (int i = 0; i < 6; i++)
                    {
                        double angleDeg = 60 * i - 30;
                        double angleRad = Math.PI / 180 * angleDeg;
                        double x = center.X + radius * Math.Cos(angleRad);
                        double y = center.Y + radius * Math.Sin(angleRad);
                        if (i == 0) ctx.BeginFigure(new Point(x, y), true, true);
                        else ctx.LineTo(new Point(x, y), true, false);
                    }
                }
                geometry.Freeze();
                return geometry;
            }
        }
    }

}
