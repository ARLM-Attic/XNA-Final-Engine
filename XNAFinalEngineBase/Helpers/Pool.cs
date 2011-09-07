
#region License
/*
Copyright (c) 2008-2011, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
                         Departamento de Ciencias e Ingeniería de la Computación - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

•	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

•	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

•	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

-----------------------------------------------------------------------------------------------------------------------------------------------
Author: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
#endregion

namespace XNAFinalEngine.Helpers
{
    /// <summary>
    /// A pool of elements T.
    /// </summary>
    /// <remarks>
    /// This is a custom structure that pursues two specific objectives.
    /// The first and more obvious is to reduce garbage collector frequency.
    /// If almost all reference type fields are allocated beforehand then few news are need it and so the garbage collector will remain idle during execution.
    /// The second objective is to guarantied memory locality so that the access time could be improved because there will be less cache misses.
    /// Plenty of information is read every frame and in chunks (tanks to the component model) so this feature could be critical.
    /// This structure also offers the possibility of modified directly the data stored (to avoid replication) and add and remove elements in O(1).
    /// Another feature added was the possibility of increment the size of the pool. This operation has O(n) so avoid using it.
    /// 
    /// Important: The .NET runtime environment manages where to allocate new reference fields and we can do much about it. 
    /// However, if we create everything at the beginning when little or no fragmentation exists then it could be possible that the reference data will be together. 
    /// Of course, this could be change or could be not true even at the beginning.
    /// 
    /// Optimization: some int values could be transformed to short.
    /// </remarks>
    /// <typeparam name="T">Valid for value or reference type</typeparam>
    public class Pool<T> where T : new()
    {

        #region Accesor

        public class Accessor
        {
            /// <summary>
            /// Indicate the index of the element in the pool.
            /// </summary>
            /// <remarks>
            /// This index could and probably changes its value during runtime, but the system will automatically update this field to the new value.
            /// Use it to access the pool array.
            /// </remarks>
            public int Index { get; internal set; }

            internal Accessor() { }
        } // Accessor

        #endregion

        #region Variables

        /// <summary>
        /// The list of accessors or key to access the pool elements
        /// </summary>
        private Accessor[] accessors;

        /// <summary>
        /// The elements.
        /// </summary>
        /// <remarks>
        /// You can access directly using the accessor's index field.
        /// </remarks>
        public T[] elements;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of active elements in the pool.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets or sets the total number of elements the internal data structure can hold without resizing.
        /// </summary>
        public int Capacity
        {
            get { return elements.Length; }
            set 
            { 
                if (value < Count)
                    throw new ArgumentOutOfRangeException("value", value, "Pool: new size has to be bigger than active elements.");
                ResizePool(value);
            }
        } // Capacity

        #endregion

        #region Constructor

        /// <summary>
        /// A pool of elements T.
        /// </summary>
        /// <param name="capacity">The total number of elements the internal data structure can hold without resizing.</param>
        public Pool(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException("capacity", capacity, "Pool: Argument capacity must be greater than zero.");
            elements = new T[capacity];
            for (int i = 0; i < capacity; i++)
            {
                // If T is a reference type then the explicit creation is need it.
                elements[i] = new T();
            }
            // They are created using another for sentence because we want memory locality.
            accessors = new Accessor[capacity];
            for (int i = 0; i < capacity; i++)
            {
                accessors[i] = new Accessor { Index = i };
            }
        } // Pool

        #endregion

        #region Fetch

        /// <summary>
        /// Marks an element for using it and return its corresponded accessor.
        /// </summary>
        /// <returns>The key to access the element. The element is not returned because was stored using a value type.</returns>
        public Accessor Fetch()
        {
            if (Count >= Capacity)
            {
                ResizePool(Capacity + 25);
            }
            Count++;
            return accessors[Count - 1];
        } // Fetch

        #endregion

        #region Resize Pool

        /// <summary>
        /// Resize the pool.
        /// </summary>
        private void ResizePool(int newCapacity)
        {
            int oldCapacity = Capacity;
            T[] newElements = new T[newCapacity];
            Accessor[] newAccessors = new Accessor[newCapacity];
            // If T is a reference type then the explicit creation is need it.
            for (int i = 0; i < newCapacity; i++)
            {
                if (i < oldCapacity)
                {
                    newElements[i] = elements[i];
                    newAccessors[i] = accessors[i];
                }
                else
                {
                    newElements[i] = new T();
                    newAccessors[i] = new Accessor { Index = i };
                }
            }
            elements = newElements;
            accessors = newAccessors;
        } // ResizePool

        #endregion

        #region Release

        /// <summary>
        /// Set the pool element to available.
        /// </summary>
        /// <param name="accessor">Its accessor</param>
        public void Release(Accessor accessor)
        {
            if (accessor == null)
                throw new ArgumentNullException("accessor", "Pool: Accessor value cannot be null");
            // To accomplish our second objective (memory locality) the last available element will be moved to the place where the released element resided.
            // First swap elements values.
            T accesorPoolElement = elements[accessor.Index]; // If T is a type by reference we can lost its value
            elements[accessor.Index] = elements[Count - 1];
            elements[Count - 1] = accesorPoolElement;
            // The indices have the wrong value. The indices have the wrong value. The last has to index its new place and vice versa.
            int accesorOldIndex = accessor.Index;
            accessor.Index = Count - 1;
            accessors[Count - 1].Index = accesorOldIndex;
            // Also the accessor array has to be sorted. If not the fetch method will give a used accessor element.
            Accessor lastActiveAccessor = accessors[Count - 1]; // Accessor is a reference type.
            accessors[Count - 1] = accessor;
            accessors[accesorOldIndex] = lastActiveAccessor;

            Count--;
        } // Release

        #endregion

    } // Pool
} // XNAFinalEngine.Helpers