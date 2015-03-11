using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Spherical;
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

  File:      Enums.cs
  Summary:   All enumerated types not in a class
  Date:	     August 10, 2005
 
======================================================= */
/// <summary>
/// All Enumerated types and their text descriptions are here
/// 
/// </summary> 
///
namespace Spherical.Htm {
    /// <summary>
    /// Markup is a classification of a trixel with respect to the way it intersects a 
    /// region. 
    /// </summary>
    public enum Markup {
        /// <summary>trixel is completely inside</summary>
        Inner,
        /// <summary>trixel non-trivially intersects</summary>
        Partial,
        /// <summary>trixel is completely outside</summary>
        Reject,
        /// <summary>trixel's status is not known</summary>
        Undefined,
        /// <summary>
        /// used for requesting trixels that are either Inner or Partial
        /// </summary>
        Outer
    };

    /// <summary>
    /// HaltCodition 
    /// </summary>
    enum HaltCondition {
        Continue,
        Hold,
        Backup
    };
}
