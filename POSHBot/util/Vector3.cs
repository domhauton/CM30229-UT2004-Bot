﻿using System;

namespace Posh_sharp.POSHBot.util
{
    public class Vector3
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        public enum Orientation {XY,XZ,YZ};

        /// <summary>
        /// takes a string of the form 'x,y,z' and converts it to a tuple (x,y,z)
        /// </summary>
        /// <param name="location">a string of three floats separated by ','</param>
        /// <returns>returns a tuple (x,y,z)</returns>
        public static Vector3 ConvertToVector3(string location)
        {
            string[] locList = location.Split(',');
            if (locList.Length != 3)
                return Vector3.NullVector();

            return new Vector3(float.Parse(locList[0]), float.Parse(locList[1]), float.Parse(locList[2]));
        }

        public static Vector3 NullVector()
        {
            return new Vector3(0,0,0);
        }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float DistanceFrom(Vector3 origin)
        {
            if (origin == null)
                return (float)Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));
            else
                return (float)Math.Sqrt(Math.Pow(X-origin.X, 2) + Math.Pow(Y-origin.Y, 2) + Math.Pow(Z-origin.Z, 2));
        }

        public float Distance2DFrom(Vector3 vector,Orientation orientation)
        {
            float result = float.MaxValue;

            switch (orientation)
            {
                case Orientation.XZ:
                    result = (float)Math.Sqrt(Math.Pow(X - vector.X, 2) + Math.Pow(Z - vector.Z, 2));
                    break;
                case Orientation.YZ:
                    result = (float)Math.Sqrt(Math.Pow(Y - vector.Y, 2) + Math.Pow(Z - vector.Z, 2));
                    break;
                default: //XY
                    result = (float)Math.Sqrt(Math.Pow(X - vector.X, 2) + Math.Pow(Y - vector.Y, 2));
                    break;
            }

            return result;
        }

        public Vector3 Add(Vector3 vector)
        {
            return new Vector3(X+vector.X,Y+vector.Y,Z+vector.Z);
        }

        public Vector3 Subtract(Vector3 vector)
        {
            return new Vector3(X - vector.X, Y - vector.Y, Z - vector.Z);
        }

        public Vector3 Mult(int multiplier)
        {
            return new Vector3(X*multiplier, Y*multiplier, Z*multiplier);
        }

        public float Norm()
        {
            return DistanceFrom(null);
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", X, Y, Z);
        }
    }

}
