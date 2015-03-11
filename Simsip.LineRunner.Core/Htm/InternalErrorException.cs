using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
#region Copyright Gyorgy Fekete
/* 
 * Copyright Alex Szalay, Gyorgy Fekete, Jim Gray 2007
 * The Spherical Geometry and Hierarchical Triangular Mesh libraries and source code are
 * provided as-is with no warranties. The user is hereby granted a non-exclusive license to use,
 * modify and extend these libraries. We ask that the authors (A. Szalay, T. Budavari, G. Fekete
 * at The Johns Hopkins University,  and Jim Gray, Microsoft Research be credited in any publication
 * or derived product that incorporates the whole or any part of our system
 */
#endregion

/*=====================================================================

  File:      InternalErrorException.cs
  Summary:   Htm specific expcetion
  Date:	     August 10, 2005
 
======================================================= */
namespace Spherical.Htm {
    /// <summary>
    /// Exceptions specific to Spherical.HTM
    /// </summary>
    [Serializable]
    public class InternalErrorException : System.Exception {
        /// <summary>
        /// Specifics of message describe the error
        /// </summary>
        /// <param name="message"></param>
        public InternalErrorException(string message)
            : base(message) {
        }
        public InternalErrorException(String msg, Exception e)
            : base(msg, e) {
        }
        public InternalErrorException()
            : base() {
        }
        protected InternalErrorException(SerializationInfo si, StreamingContext sc)
            : base(si, sc) {
        }
    }
}

