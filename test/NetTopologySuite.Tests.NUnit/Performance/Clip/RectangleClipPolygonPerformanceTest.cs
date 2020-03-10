﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NetTopologySuite.Clip;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Tests.NUnit.TestData;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Clip
{
    public class RectangleClipPolygonPerformanceTest
    {

        static GeometryFactory factory = new GeometryFactory();

        public static List<Geometry> readWKTFile(Stream fileStream)
        {
            var fileRdr = new WKTFileReader(fileStream, new WKTReader());
            return (List<Geometry>) fileRdr.Read();
        }

        [Test, Category("Stress")]
        public void Test()
        {
            Geometry data = LoadData();

            Console.WriteLine($"Dataset: # geometries = {data.NumGeometries}   # pts = {data.NumPoints}");

            var sw = new Stopwatch();
            sw.Start();
            RunClip(data);
            sw.Stop();

            Console.WriteLine("Time: {0}ms", sw.ElapsedMilliseconds);
        }

        private GeometryCollection LoadData()
        {
            List<Geometry> data = null;
            try
            {
                data = readWKTFile(EmbeddedResourceManager.GetResourceStream("NetTopologySuite.Tests.NUnit.TestData.world.wkt"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            return factory.CreateGeometryCollection(data.ToArray());
        }

        private void RunClip(Geometry data)
        {
            var dataEnv = data.EnvelopeInternal;

            for (int x = -180; x < 180; x += 10)
            {
                for (int y = -90; y < 90; y += 10)
                {
                    var env = new Envelope(x, x + 10, y, y + 10);
                    var rect = factory.ToGeometry(env);
                    RunClip(rect, data);
                }
            }
        }

        private void RunClip(Geometry rect, Geometry data)
        {
            for (int i = 0; i < data.NumGeometries; i++)
            {
                var geom = data.GetGeometryN(i);
                Clip(rect, geom);
                //RectangleIntersection(rect, geom);
            }
        }

        private Geometry Clip(Geometry rect, Geometry geom)
        {
            var clipper = new RectangleClipPolygon(rect);
            return clipper.Clip(geom);
        }

        private Geometry RectangleIntersection(Geometry rect, Geometry geom)
        {
            var env = rect.EnvelopeInternal;
            //Geometry result;
            if (env.Contains(geom.EnvelopeInternal))
            {
                return geom.Copy();
            }

            if (!env.Intersects(geom.EnvelopeInternal))
                return null;
            if (rect.Intersects(geom))
                return rect.Intersection(geom);
            return null;
        }

        private Envelope Envelope(List<Geometry> world)
        {
            var env = new Envelope();
            foreach (var geom in world)
            {
                env.ExpandToInclude(geom.EnvelopeInternal);
            }

            return env;
        }
    }
}
