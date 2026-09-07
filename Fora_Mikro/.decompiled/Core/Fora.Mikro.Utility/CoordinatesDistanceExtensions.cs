using System;

namespace Fora.Mikro.Utility;

public static class CoordinatesDistanceExtensions
{
	public static double DistanceTo(this Coordinates baseCoordinates, Coordinates targetCoordinates)
	{
		return baseCoordinates.DistanceTo(targetCoordinates, UnitOfLength.Kilometers);
	}

	public static double DistanceTo(this Coordinates baseCoordinates, Coordinates targetCoordinates, UnitOfLength unitOfLength)
	{
		double num = Math.PI * baseCoordinates.Latitude / 180.0;
		double num2 = Math.PI * targetCoordinates.Latitude / 180.0;
		double num3 = baseCoordinates.Longitude - targetCoordinates.Longitude;
		double d = Math.PI * num3 / 180.0;
		double d2 = Math.Sin(num) * Math.Sin(num2) + Math.Cos(num) * Math.Cos(num2) * Math.Cos(d);
		d2 = Math.Acos(d2);
		d2 = d2 * 180.0 / Math.PI;
		d2 = d2 * 60.0 * 1.1515;
		return unitOfLength.ConvertFromMiles(d2);
	}
}
