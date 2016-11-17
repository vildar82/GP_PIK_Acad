namespace PIK_GP_Acad.Insolation.Services
{
    public interface ICalcValues
    {
        double Fi { get; }
        double SunCalcAngleEnd { get; }
        double SunCalcAngleEndOnPlane { get; }
        double SunCalcAngleStart { get; }
        double SunCalcAngleStartOnPlane { get; }

        double AngleSun (double angleOnPlane);
        double AngleSunOnPlane (double angleSun);
        double AngleSunOnPlane (double yShadow, double xShadow);
        double GetInsAngleFromAcad (double acadAngle);
        double GetXRay (double cSunPlane, double angleSun);
        double YShadowLineByHeight (double height, out double cShadowPlane);
    }
}