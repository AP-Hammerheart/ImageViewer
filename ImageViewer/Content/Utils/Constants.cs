﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;

namespace ImageViewer.Content.Utils
{
    internal static class Constants
    {
        private static readonly float scale = 1.0f;

        internal static float DistanceFromUser { get; } = -1.4f;

        internal static float ViewSize { get; } = 0.6f * scale;
        internal static float HalfViewSize { get; } = 0.3f * scale;
        internal static double TileSize { get; } = 0.2 * (double)scale;

        internal static float X00 { get; } = -1.024264f * scale;
        internal static float X01 { get; } = -0.6f * scale;
        internal static float X02 { get; } = -0.2f * scale;
        internal static float X03 { get; } = -0.1f * scale;
        internal static float X04 { get; } = 0.0f * scale;                     
        internal static float X05 { get; } = 0.25f * scale;
        internal static float X06 { get; } = 0.35f * scale;
        internal static float X07 { get; } = 0.45f * scale;
        internal static float X08 { get; } = 0.5f * scale;
        internal static float X09 { get; } = 0.6f * scale;
        internal static float X10 { get; } = 1.024264f * scale;

        internal static float Y0 { get; } = -0.35f * scale;
        internal static float Y1 { get; } = -0.3f * scale;
        internal static float Y2 { get; } = 0.3f * scale;
        internal static float Y3 { get; } = 0.33f * scale;
        internal static float Y4 { get; } = 0.38f * scale;

        internal static float Z0 { get; } = 0.0f * scale;
        internal static float Z1 { get; } = 0.424264f * scale;       
        internal static float Z2 { get; } = 1.324264f * scale;
        internal static float Z3 { get; } = 1.754203f * scale;

        internal static int TileResolution { get; } = 256;

        internal static int TileCountX { get; } = 3;
        internal static int TileCountY { get; } = 3;

        internal static double Diagonal { get; } = Math.Sqrt(2.0) * 1.5 * TileResolution;

        internal static float MX { get; } = 0.8f * scale;
        internal static float MY { get; } = 0.0f * scale;
        internal static float MZ { get; } = -0.5f * scale;


        //Case Selection Constants:
    }
}
