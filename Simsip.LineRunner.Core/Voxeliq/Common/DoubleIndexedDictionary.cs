/*
 * Voxeliq Engine, Copyright (C) 2011 - 2013 Int6 Studios - All Rights Reserved. - http://www.int6.org - https://github.com/raistlinthewiz/voxeliq
 *
 * This file is part of Voxeliq Engine project. This program is free software; you can redistribute it and/or modify 
 * it under the terms of the Microsoft Public License (Ms-PL).
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
#if WINDOWS_PHONE
using Simsip.LineRunner.Concurrent;
#else
using System.Collections.Concurrent;
#endif

namespace Engine.Common
{
    /// <summary>
    /// A dictionary objects that accepts dual keys for indexing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DoubleIndexedDictionary<T> : ConcurrentDictionary<long, T>
    // public class DoubleIndexedDictionary<T> : Dictionary<long, T>
    {
        private const long RowSize = Int32.MaxValue;

        public T this[int x, int z]
        {
            get
            {
                T @out = default(T);
                TryGetValue(x + (z*RowSize), out @out);
                return @out;
            }
            set { this[x + (z*RowSize)] = value; }
        }

        public bool ContainsKey(int x, int z)
        {
            return ContainsKey(x + (z*RowSize));
        }

        // public bool Remove(int x, int z)
        public T Remove(int x, int z)
        {
            // TODO: Had to modify this as TryRemove was not available
            T removed;
            TryRemove(x + (z*RowSize), out removed);
            return removed;
            /*
            bool removed = false;
            try
            {
                // TryRemove()
                removed = Remove(x + (z * RowSize));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in DoubleIndexedDictionary.Remove: " + ex);
            }

            return removed;
            */
        }
    }
}