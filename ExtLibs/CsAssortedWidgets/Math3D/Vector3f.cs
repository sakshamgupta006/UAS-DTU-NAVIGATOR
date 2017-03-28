﻿
#region GPL License
/*
Copyright (c) 2010 Miguel Angel Guirado López

This file is part of CsAssortedWidgets.

    Trixion3D is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    CsAssortedWidgets is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with CsAssortedWidgets.  If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

namespace AssortedWidgets.Math3D
{
    public class Vector3f
    {
        public float[] values = new float[3];

        #region Constructores

        public Vector3f()
        {
        }
        public Vector3f(float x, float y, float z)
        {
            values[0] = x;
            values[1] = y;
            values[2] = z;
        }
        #endregion Constructores

        #region Propiedades

        public float X
        {
            get { return values[0]; }
            set { values[0] = value; }
        }

        public float Y
        {
            get { return values[1]; }
            set { values[1] = value; }
        }

        public float Z
        {
            get { return values[2]; }
            set { values[2] = value; }
        }
        #endregion Propiedades
    }
}
