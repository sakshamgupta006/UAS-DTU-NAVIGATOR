﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;
using ProjNet.Converters;

namespace ArdupilotMega.Swarm
{
    /// <summary>
    /// Follow the leader
    /// </summary>
    class Formation: Swarm
    {

        Dictionary<MAVLink, HIL.Vector3> offsets = new Dictionary<MAVLink, HIL.Vector3>();
        
        PointLatLngAlt masterpos = new PointLatLngAlt();

        public void setOffsets(MAVLink mav, float x, float y, float z)
        {
            offsets[mav] = new HIL.Vector3(x,y,z);
            //log.Info(mav.ToString() + " " + offsets[mav].ToString());
        }

        public HIL.Vector3 getOffsets(MAVLink mav)
        {
            if (offsets.ContainsKey(mav))
            {
                return offsets[mav];
            }
            
            if (masterpos.GetDistance(new PointLatLngAlt(mav.MAV.cs.lat, mav.MAV.cs.lng, mav.MAV.cs.alt, "")) > 100)
            {
                return new HIL.Vector3(offsets.Count, 0, 0); 
            }

            return getOffsetFromLeader(mav);//offsets.Count, 0, 0);
        }

        public HIL.Vector3 getOffsetFromLeader(MAVLink mav)
        {
            //convert Wgs84ConversionInfo to utm
            CoordinateTransformationFactory ctfac = new CoordinateTransformationFactory();

            GeographicCoordinateSystem wgs84 = GeographicCoordinateSystem.WGS84;

            int utmzone = (int)((masterpos.Lng - -183.0) / 6.0);

            IProjectedCoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(utmzone, masterpos.Lat < 0 ? false : true);

            ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(wgs84, utm);

            double[] masterpll = { masterpos.Lng, masterpos.Lat };

            // get leader utm coords
            double[] masterutm = trans.MathTransform.Transform(masterpll);

            double[] mavpll = { mav.MAV.cs.lng, mav.MAV.cs.lat };

            //getLeader follower utm coords
            double[] mavutm = trans.MathTransform.Transform(mavpll);

            return new HIL.Vector3(masterutm[1] - mavutm[1], masterutm[0] - mavutm[0],0);
        }

        public override void Update()
        {
            if (MainV2.comPort.MAV.cs.lat == 0 || MainV2.comPort.MAV.cs.lng == 0)
                return;

            if (Leader == null)
                Leader = MainV2.comPort;

            masterpos = new PointLatLngAlt(Leader.MAV.cs.lat, Leader.MAV.cs.lng, Leader.MAV.cs.alt, "");
        }

        public override void SendCommand()
        {
            if (masterpos.Lat == 0 || masterpos.Lng == 0)
                return;

            Console.WriteLine(DateTime.Now);
            Console.WriteLine("Leader {0} {1} {2}",masterpos.Lat,masterpos.Lng,masterpos.Alt);

            int a = 0;
            foreach (var port in MainV2.Comports)
            {
                if (port == Leader)
                    continue;

                PointLatLngAlt target = new PointLatLngAlt(masterpos);

                try
                {
                    //convert Wgs84ConversionInfo to utm
                    CoordinateTransformationFactory ctfac = new CoordinateTransformationFactory();

                    GeographicCoordinateSystem wgs84 = GeographicCoordinateSystem.WGS84;

                    int utmzone = (int)((masterpos.Lng - -183.0) / 6.0);

                    IProjectedCoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(utmzone, masterpos.Lat < 0 ? false : true);

                    ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(wgs84, utm);

                    double[] pll1 = { target.Lng, target.Lat };

                    double[] p1 = trans.MathTransform.Transform(pll1);

                    // add offsets to utm
                    p1[0] += ((HIL.Vector3)offsets[port]).x;
                    p1[1] += ((HIL.Vector3)offsets[port]).y;

                    // convert back to wgs84
                    IMathTransform inversedTransform = trans.MathTransform.Inverse();
                    double[] point = inversedTransform.Transform(p1);

                    target.Lat = point[1];
                    target.Lng = point[0];
                    target.Alt += ((HIL.Vector3)offsets[port]).z;

                    port.setGuidedModeWP(new Locationwp() { alt = (float)target.Alt, lat = (float)target.Lat, lng = (float)target.Lng, id = (byte)MAVLink.MAV_CMD.WAYPOINT });

                    Console.WriteLine("{0} {1} {2} {3}", port.ToString(), target.Lat, target.Lng, target.Alt);

                }
                catch (Exception ex) { Console.WriteLine("Failed to send command " + port.ToString() + "\n" + ex.ToString()); }

                a++;
            }


        }


    }
}