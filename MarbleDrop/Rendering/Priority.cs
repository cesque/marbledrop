using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarbleDrop.Rendering
{
    public enum Priority
    {
        Default = 0,

        Puzzle = 100,

        SparkWire = 150,
        SparkWireCorners = 160,
        Spark = 170,

        Wire = 200,
        WireCorners = 210,

        Component = 300,

        Marble = 400,
    }
}
