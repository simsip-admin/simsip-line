using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
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

  File:      HtmState.cs 
  Summary:   Implements a singlton object for holding global state.
  Date:	     August 16, 2005

======================================================= */

namespace Spherical.Htm
{
	/// <summary>
	/// This class maintains writeable global variables for otherwise static method
	/// This is a sigleton object
	/// </summary>
	/// Things have been flagged MAKESTATE in other source files that are affeced
	
    public struct HtmFace {
        public Int64 hid;
        public char[] name;
        public int vi0, vi1, vi2;
        public HtmFace(Int64 hid, char n0, char n1, int i0, int i1, int i2) {
            this.hid = hid;
            this.name = new char[2];
            this.name[0] = n0;
            this.name[1] = n1;
            this.vi0 = i0;
            this.vi1 = i1;
            this.vi2 = i2;
        }

    }
	public sealed class HtmState
	{
		
		// //////////////////////////////////////////////
#if DEBUG
		private const string _versionstring = "Spherical.HTM 3.1.2 (Debug - Build 3-29-2007)";
#else
		private const string _versionstring = "Spherical.HTM 3.1.2 (Release - Build 3-29-2007)";
#endif
        private int _hdelta      =  2;


		private int _deltalevel = 0;	// used in interactive experimentation, developer
		private Int64 _uniqueID = 0; // each halfspace may have a unique number
		private int _times = 0;

		private int _minlevel;
		private int _maxlevel;
		/// <summary>
		/// In radians
		/// </summary>
		private double[] _featuresizes = null;
		static readonly HtmState instance = new HtmState();
		/// FOR MAKING STATIC HTMLOOKUP
		public  int[] Xfaces;
        public HtmFace[] faces;
		internal  Cartesian[] originalPoints;
		HtmState()
		{
            faces = new HtmFace[8];
            faces[0] = new HtmFace(8, 'S', '0', 1, 5, 2);
            faces[1] = new HtmFace(9, 'S', '1', 2,5,3);
            faces[2] = new HtmFace(10,'S', '2',3,5,4);
            faces[3] = new HtmFace(11, 'S', '3', 4,5,1);
            faces[4] = new HtmFace(12, 'N', '0', 1,0,4);
            faces[5] = new HtmFace(13, 'N', '1', 4,0,3);
            faces[6] = new HtmFace(14, 'N', '2', 3,0,2);
            faces[7] = new HtmFace(15, 'N', '3', 2,0,1);

            originalPoints = new Cartesian[6];
            originalPoints[0] = new Cartesian( 0.0,  0.0,  1.0, false);
            originalPoints[1] = new Cartesian( 1.0,  0.0,  0.0, false);
            originalPoints[2] = new Cartesian( 0.0,  1.0,  0.0, false);
            originalPoints[3] = new Cartesian(-1.0,  0.0,  0.0, false);
            originalPoints[4] = new Cartesian( 0.0, -1.0,  0.0, false);
            originalPoints[5] = new Cartesian( 0.0,  0.0, -1.0, false);

			
			_featuresizes = new double[] {				
							  1.5707963267949,        //0
							  0.785398163397448,      //1
							  0.392699081698724,      //2
							  0.196349540849362,
							  0.098174770424681,
							  0.0490873852123405,     //5
							  0.0245436926061703,
							  0.0122718463030851,
							  0.00613592315154256,
							  0.00306796157577128,
							  0.00153398078788564,   //10
							  0.000766990393942821,
							  0.00038349519697141,
							  0.000191747598485705,
							  9.58737992428526E-05,
							  4.79368996214263E-05,  //15
							  2.39684498107131E-05,
							  1.19842249053566E-05,
							  5.99211245267829E-06,
							  2.99605622633914E-06,
							  1.49802811316957E-06,  //20
							  7.49014056584786E-07,  //21
							  3.74507028292393E-07,  //22
							  0.0,
							 -1.0 // to stop search beyond 0
						  };

		}
        public Cartesian Originalpoint(int i) {
            return originalPoints[i];
        }
        public Int64 Face(int i, out int ix, out int iy, out int iz) {
            ix = this.faces[i].vi0;
            iy = this.faces[i].vi1;
            iz = this.faces[i].vi2;
            return this.faces[i].hid;
        }
		static HtmState()
		{
		}
		public static HtmState Instance
		{
			get
			{
				return instance;
			}
		}
		public int times
		{
			get
			{
				return _times;
			}
		}
		public int minlevel {
			get { return _minlevel; }
			set { _minlevel = value; }
		}
		public int maxlevel {
			get { return _maxlevel; }
			set { _maxlevel = value; }
		}
		public int hdelta {
			get {
				return _hdelta;
			}
			set {
				_hdelta = value;
				throw new InternalErrorException("Someone is changing helta");
			}
		}
		public int deltalevel {
			get {
				return _deltalevel;
			}
			set {
				_deltalevel = value;
			}
		}
		public long newID
		{
			get
			{
				_times++;
				_uniqueID++;
				return _uniqueID;
			}
		}
		public string getVersion()
		{
			return _versionstring;
		}
		public int getLevel(double radius)
		{
			int i = 0;
			while (this._featuresizes[i] > radius)
			{
				i++; // there is a -1 at the end, will force a break
			}
			return i;
		}
	}
}
