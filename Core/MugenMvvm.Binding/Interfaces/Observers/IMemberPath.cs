﻿namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IMemberPath
    {
        string Path { get; }

        string[] Members { get; }//todo review

        bool IsSingle { get; }
    }
}