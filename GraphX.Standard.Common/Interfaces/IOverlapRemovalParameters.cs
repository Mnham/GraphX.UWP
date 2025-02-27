﻿namespace GraphX.Common.Interfaces
{
    public interface IOverlapRemovalParameters : IAlgorithmParameters
    {
        float VerticalGap { get; set; }
        float HorizontalGap { get; set; }
    }
}