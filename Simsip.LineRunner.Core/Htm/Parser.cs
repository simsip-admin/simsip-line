using System;
using System.Collections.Generic;
using Spherical;
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

  File:      Parser.cs 
  Summary:   Implements a parser which converts text to various kinds of regions.
  Date:	     November 27, 2006
 
======================================================= */

namespace Spherical.Shape
{
    [Serializable]
    public class ParserException : System.Exception {
        public Parser.Error errno;
        protected ParserException(SerializationInfo si, StreamingContext sc):
            base(si, sc) {
        }
        public ParserException() : base() {
        }
        public ParserException(String s) : base(s) {
        }
        public ParserException(String s, Exception e)
            : base(s, e) {
        }
        public ParserException(Parser.Error errno)
            : base(Parser.errmsg(errno)) {
            this.errno = errno;
        }
    }
	/// <summary>
	/// With the rules of the <a href="../../HtmPrimer/regiongrammar.html">Shape Grammar</a>,
	/// turn the given specification into a Region object with its only public method.
	/// </summary>
	public class Parser {
		private enum Format {
			J2000,
			Cartesian,
			Latlon,
			Null,
			Unkown
		}
		private enum Geometry {
			Region,
			Convex,
			Halfspace, /* n/a */
			Rect,
			Circle,
			Poly,
			Chull,
			Null
		}
        /// <summary>
        /// Descibes the Spherical Shape Parser error
        /// </summary>
		public enum Error {
            /// <summary>
            /// All is well, no error
            /// </summary>
			Ok,
			/// <summary>
			/// The parsed token was not a number.
			/// </summary>
			errIllegalNumber,
            /// <summary>
            /// Premature end of line was reached
            /// </summary>
			errEol,
            /// <summary>
            /// There were other than 2 points specified for a RECT command.
            /// </summary>
			errNot2forRect,
            /// <summary>
            /// There weren't at least 3 points for specifying a polygon or convex hull.
            /// </summary>
			errNotenough4Poly,
            /// <summary>
            /// The given polygon is either concave, or has self-intersecting edges
            /// </summary>
			errPolyBowtie,
            /// <summary>
            /// The polygon specified contains edges of zero length (eg, from consecutive 
            /// identical points).
            /// </summary>
			errPolyZeroLength,
            /// <summary>
            /// The list of points for convex hull generation span a space larger than a hemisphere.
            /// </summary>
			errChullTooBig,
            /// <summary>
            /// The list of points for convex hull generation are all on the same great circle.
            /// </summary>
            errChullCoplanar,
            /// <summary>
            /// An empty string was given as a specification.
            /// </summary>
			errNullSpecification,
            /// <summary>
            /// Some other error occured. Please call someone if this happens
            /// </summary>
			errUnknown
		}
		private const int INITIAL_CAPACITY = 30;
		private int _current_capacity = INITIAL_CAPACITY;
		private List<double> xs, ys, zs, ras, decs;
		private string _spec;
		private char[] _delims;
		private string[] _tokens;
		private int _ct;
		private Error _error;
		private Format _format;
		private Geometry _geometry; // The kind of object we are building
		private Region _targetregion;		// Everything is a region, it will be built to here....
		private bool beginAccumulate;

		private void init(){
			_delims = " \t\n\r".ToCharArray();
			xs  = new List<double>(INITIAL_CAPACITY); 
			ys = new List<double>(INITIAL_CAPACITY);
			zs = new List<double>(INITIAL_CAPACITY);
			ras = new List<double>(INITIAL_CAPACITY);
			decs = new List<double>(INITIAL_CAPACITY);
			beginAccumulate = false;
		}
        /// <summary>
        /// Turns a Parser.Error argument into a textual description.
        /// </summary>
        /// <param name="errno"></param>
        /// <returns>string containing text of description</returns>
        public static string errmsg(Error  errno) {
            switch (errno) {
                case Error.Ok: return "Ok";
                case Error.errIllegalNumber: return "Illegal number format";
                case Error.errUnknown: return "Unknown error (2). Call George";
                case Error.errEol: return "End of line not expected. Possibly need more arguments";
                case Error.errNot2forRect: return "Not 2 points given (exactly 2 needed) for a rectangle)";
                case Error.errPolyBowtie: return "Polygon has a bowtie or is concave";
                case Error.errPolyZeroLength: return "Polygon had zero length edges";
                case Error.errNotenough4Poly: return "Need 3 or more points for a polygon";
                case Error.errChullTooBig: return "Points for convex hull cover more than half the globe";
                case Error.errChullCoplanar: return "Points for convex are all on the same great circle";
                case Error.errNullSpecification: return "Empty string is an incorrect specification";
            }
            return "Unknown error (3). Call George";
        }
		private Parser() : this(null) {
		}
        /// <summary>
        /// Create an instance of a shape parser with a specification
        /// 
        /// Parsing occurs when parse() is invoked.
        /// </summary>
        /// <param name="in_spec">a valid shape specification</param>
        public Parser(string in_spec) {
            init();
            input = in_spec;
        }

		private Region region {
			get {
				return this._targetregion;
			}
			set {
				this._targetregion = value;
			}
		}
		/* *******************process tokens******************************/
		private void advance(){
			_ct++;
		}
		/// <summary>
		/// True, if there are any tokens left to parse. ismore() skips over white spaces
		/// </summary>
		private  bool ismore {
			get {
				if (_tokens == null)
					return false;
				if (_tokens[0] == null)
					return false;

				while (_ct < _tokens.Length && _tokens[_ct].Length == 0) {
					_ct++;
				}
				if (_ct < _tokens.Length && beginAccumulate) {
					try {
						double.Parse(_tokens[_ct]);
					} catch {
						return false;
					}
				}
				return _ct < _tokens.Length;
			}
		}
		private int getint(int skip){
			int res;
			if (!ismore){
				_error = Error.errEol;
				throw new ParserException(Error.errEol);
			}
			res = int.Parse(_tokens[_ct].Substring(skip));
			advance();
			return res;
		}
		private int getint() {
			int res;
			if (!ismore){
				_error = Error.errEol;
                throw new ParserException(Error.errEol);
			}
			res = int.Parse(_tokens[_ct]);
			advance();
			return res;
		}
		private double getdouble() {
			double res;
			if (!ismore){
				_error = Error.errEol;
                throw new ParserException(Error.errEol);
			}
			res = double.Parse(_tokens[_ct]);
			advance();
			return res;
		}
        private void getNormalVector(out double x, out double y, out double z) {
            x = getdouble();
            y = getdouble();
            z = getdouble();
            double norm = x * x + y * y + z * z;
            norm = Math.Sqrt(norm);
            x /= norm;
            y /= norm;
            z /= norm;
            return;
        }
        /// <summary>
        /// Is the current token same as pattern?
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="casesensitive"></param>
        /// <returns></returns>
		private bool match_current(string pattern, bool casesensitive){
			if (!ismore){
				return false;
			}
			if (casesensitive){
				return (_tokens[_ct].Equals(pattern));
			} else {
				return (_tokens[_ct].ToLower().Equals(pattern));
			}
		}
        /// <summary>
        /// Does the current token have the given prefix?
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
		private bool isprefix(string prefix){
			if (!ismore)
				return false;
			return _tokens[_ct].Substring(0, prefix.Length).Equals(prefix);
		}
        /// <summary>
        /// 
        /// </summary>
		private string input {
			get {
				return _spec;
			}
			set {
				_error = Error.Ok;
				_format = Format.Unkown;
				_geometry = Geometry.Null;
				_ct = 0;
				_spec = value;
                if (_targetregion != null) {
                    _targetregion.Clear();
                }
				if (_spec == null){
					_tokens = null;
				} else if (_spec.Length > 0){
					_tokens = _spec.Split(_delims);

				} else {
					_tokens = null;
				}		
			}
		}
		private void rewind(){
			_ct = 0;
		}
        //private  Parser(char[] in_spec) {
        //    //
        //    // Break spec into words (tokens)
        //    // First word must be recognized as one of the keys
        //    // Case insensitive
        //    init();
        //    input = new string(in_spec);
        //}
		private Format peekFormat() {

            // Peek into the string, and return the type of geometric object
			// the parser will build.

			if (_tokens == null)
				return Format.Null;
			if (match_current("cartesian", false)) {
				return Format.Cartesian;
			} else if (match_current("j2000", false)) {
				return Format.J2000;
			} else if (match_current("latlon", false)) {
				return Format.Latlon;
			} else {
				return Format.Null;
			}
			// return Format.Null;
		}
		private Geometry peekGeometry() {
			// Peek into the string, and return the type of geometric object
			// the parser will build.
			if (_tokens == null)
				return Geometry.Null;
			if (match_current("convex", false))
				return Geometry.Convex;
			if (match_current("region", false))
				return Geometry.Region;
			if (match_current("halfspace", false))
				return Geometry.Halfspace;
			if (match_current("poly", false))
				return Geometry.Poly;
			if (match_current("chull", false))
				return Geometry.Chull;
			if (match_current("rect", false))
				return Geometry.Rect;
			if (match_current("circle", false))
				return Geometry.Circle;
			return Geometry.Null;
		}
		private Error parse() {
			// smarter parser, allows mixed geometries and object
			// Skip the first 'REGION', else assume that
			bool multiple = false; // parse a region, use multiple, else just one object

			if (!ismore) {
				return Error.errNullSpecification;
			}
			_geometry = peekGeometry();
			if (_geometry == Geometry.Region) {
				advance();
				multiple = true;
			}
			while (ismore) {			// We loop on each CONVEX, whatever it may be...
				_geometry = peekGeometry();
                this.crearPoints();
				switch (_geometry) {
					case Geometry.Circle:
						if (parse_circle() != Error.Ok) {
							return _error; // Adds circle convex to _targetreion
						}
						break;
					case Geometry.Convex:
						if (parse_convex() != Error.Ok) {
							return _error;
						}
						break;
					case Geometry.Rect:
						if (parse_rectangle() != Error.Ok) {
							return _error;
						}
						break;
					case Geometry.Poly:
						if (parse_polygon() != Error.Ok) {
							return _error;
						}
						break;
					case Geometry.Chull:
						if (parse_chull() != Error.Ok) {
							return _error;
						}
						break;
					case Geometry.Null:
						_error = Error.errUnknown;
						return _error;
						// break;
				}
				if (!multiple)
					break;
			}
			return _error;
		}
        private void crearPoints()
        {
            xs.Clear();
            ys.Clear();
            zs.Clear();
            ras.Clear();
            decs.Clear();
        }
		private void increaseCapacity() {
			this._current_capacity += INITIAL_CAPACITY;
			xs.Capacity += INITIAL_CAPACITY;
			ys.Capacity += INITIAL_CAPACITY;
			zs.Capacity += INITIAL_CAPACITY;
			ras.Capacity += INITIAL_CAPACITY;
			decs.Capacity += INITIAL_CAPACITY;
		}

		/************************************* ***************** *************************/
		/************************************* SPECIALTY PARSERS *************************/
		/************************************* ***************** *************************/
		#region parse_RECTANGLE
		/* ******************************************** RECT ***/
		private Error parse_rectangle() {

			Region region = _targetregion;
			int npoints = 0;
			double ra, dec, x, y, z;
            
			if (region == null)
				return Error.Ok;
			advance(); // Skip over 'RECT'
			_format = peekFormat();
			if (ismore && _format != Format.Null) {
				advance();
			}
			while (ismore) {
				switch (_format) {
					case Format.Null:
					case Format.Cartesian:
						try {
                            //x = this.getdouble();
                            //y = this.getdouble();
                            //z = this.getdouble();
                            getNormalVector(out x, out y, out z);
						} catch {
							if (_error != Error.errEol)
								_error = Error.errIllegalNumber;
							return _error;
						}
						xs.Add(x);
						ys.Add(y);
						zs.Add(z);
						Cartesian.Xyz2Radec(x, y, z, out ra, out dec);
						ras.Add(ra);
						decs.Add(dec);
						npoints++;
						break;
					case Format.J2000:
					case Format.Latlon:
						try {
							ra = this.getdouble();
							dec = this.getdouble();
						} catch {
							if (_error != Error.errEol)
								_error = Error.errIllegalNumber;
							return _error;
						}
						ras.Add(ra);
						decs.Add(dec);
						npoints++;
						break;
					default:
						break;
				}
				if (npoints >= 2) {
					break;
				}
			}
			if (npoints != 2) {
				_error = Error.errNot2forRect;
				return _error;
			}
            //
            // Rectangle's static methods will create a new convex
            // 
            //
			// WAS:rect = new Rectangle(region);
            Convex con;
			if (_format == Format.Latlon) {
				con = Rectangle.Make(decs[0], ras[0], decs[1], ras[1]);
			} else {
				con = Rectangle.Make(ras[0], decs[0], ras[1], decs[1]);
			}
            _targetregion.Add(con);
			return Error.Ok;
		}
		/************************** END RECT *******************************/
		#endregion
		#region parse_POLYGON
		/* ******************************************** POLY ***/
		private Error parse_polygon() {

			int npoints = 0;
			double ra, dec, x, y, z;

			if (_targetregion == null)
				return Error.Ok;

			advance(); // Skip over "poly"
			_format = peekFormat();
			if (ismore && _format != Format.Null) {
				advance();
			}
			while (ismore) {
				if (npoints >= this._current_capacity) {
					increaseCapacity();
				}
				switch (_format) {
					case Format.Null:
					case Format.Cartesian:
						try {
                            getNormalVector(out x, out y, out z);
                        } catch {
							if (_error != Error.errEol)
								_error = Error.errIllegalNumber;
							return _error;
						}
						xs.Add(x);
						ys.Add(y);
						zs.Add(z);
						npoints++;
						beginAccumulate = true;
						break;
					case Format.J2000:
					case Format.Latlon:
						try {
							ra = this.getdouble();
							dec = this.getdouble();
						} catch {
							if (_error != Error.errEol)
								_error = Error.errIllegalNumber;
							return _error;
						}
						if (_format == Format.Latlon) {
							Cartesian.Radec2Xyz(dec, ra, out x, out y, out z);
						} else {
							Cartesian.Radec2Xyz(ra, dec, out x, out y, out z);
						}
						xs.Add(x);
						ys.Add(y);
						zs.Add(z);
						npoints++;
						beginAccumulate = true;
						break;
					default:
						break;
				}
			}
			if (npoints < 3) {
				_error = Error.errNotenough4Poly;
				return _error;
			}
			// poly = new Polygon(region);
			// WARNING!!! more than one kinf of error. The other one is for
			// degenerate edges, two points are too close... or you can eliminate them!
            Polygon.Error err;
            Convex con;
            con = Polygon.Make(xs, ys, zs, npoints, out err);
            if (con != null) {
                _targetregion.Add(con);
            }
			// = poly.make(xs, ys, zs, npoints);
			if (err == Polygon.Error.errBowtieOrConcave) {
				_error = Error.errPolyBowtie;
				return _error;
			} else if (err == Polygon.Error.errZeroLength){
				_error = Error.errPolyZeroLength;
				return _error;
			}
			beginAccumulate = false;
			return Error.Ok;
		}
		/************************** END POLYGON *******************************/
		#endregion
		#region parse_CHULL
		/* ******************************************** CHULL ***/

        
		private Error parse_chull() {
			Region region = _targetregion;
			int npoints = 0;
			double ra, dec, x, y, z;

            if (region == null) {
                _error = Error.Ok;
                return _error;
            }
			advance(); // Skip over 'CHULL'
			_format = peekFormat();
			if (ismore && _format != Format.Null) {
				advance();
			}
			while (ismore) {


				switch (_format) {
					case Format.Null:
					case Format.Cartesian:
						try {
                            getNormalVector(out x, out y, out z);
                            //x = this.getdouble();
                            //y = this.getdouble();
                            //z = this.getdouble();
						} catch {
							if (_error != Error.errEol)
								_error = Error.errIllegalNumber;
							return _error;
						}
						xs.Add(x);
						ys.Add(y);
						zs.Add(z);
						npoints++;
						break;
					case Format.J2000:
					case Format.Latlon:
						try {
							ra = this.getdouble();
							dec = this.getdouble();
						} catch {
							if (_error != Error.errEol)
								_error = Error.errIllegalNumber;
							return _error;
						}
						if (_format == Format.Latlon) {
							Cartesian.Radec2Xyz(dec, ra, out x, out y, out z);
						} else {
							Cartesian.Radec2Xyz(ra, dec, out x, out y, out z);
						}
						xs.Add(x);
						ys.Add(y);
						zs.Add(z);
						npoints++;
						break;
					default:
						break;
				}
			}
			if (npoints < 3) {
				_error = Error.errNotenough4Poly;
				return _error;
			}
            Chull.Cherror err;
            Convex con;
            con = Chull.Make(xs, ys, zs, out err);
            if (con != null){
               _targetregion.Add(con);
            }

            if (err == Chull.Cherror.BiggerThanHemisphere) {
                _error = Error.errChullTooBig;
                return _error;
            } else if (err == Chull.Cherror.Coplanar) {
                _error = Error.errChullCoplanar;
                return _error;
            }
            
            /* */
            //hull = new Chull(region);
            //if (hull.addCloud(xs, ys, zs) == false) {
            //    _error = Error.errChullTooBig;
            //    return _error;
            //}
			return Error.Ok;
		}
		/************************** END CHULL *******************************/
		#endregion
		#region parse_circle
		/* ******************************************** CIRCLE ***/
		private Error parse_circle() {
	
			Region region = _targetregion;
			// Circle circle = null;
            Convex con;
			double ra, dec, x, y, z, rad;

			if (region == null)
				return Error.Ok;
			advance(); // Skip over 'CIRCLE'
			_format = peekFormat();
			if (ismore && _format != Format.Null) {
				advance();
			}
			if (ismore) {
				switch (_format){
					case Format.Null:
					case Format.Cartesian:
						try {
                            //x = this.getdouble();
                            //y = this.getdouble();
                            //z = this.getdouble();
                            getNormalVector(out x, out y, out z);
							rad = this.getdouble();
						} catch {
							if (_error != Error.errEol)
								_error = Error.errIllegalNumber;
							return _error;
						}
                        con = Circle.Make(x, y, z, rad);
                        _targetregion.Add(con);
						break;
					case Format.J2000:
					case Format.Latlon:
						try {
							ra = this.getdouble();
							dec = this.getdouble();
							rad = this.getdouble();
						} catch {
							if (_error != Error.errEol)
								_error = Error.errIllegalNumber;
							return _error;
						}
						if (_format == Format.J2000) {
                            con = Circle.Make(ra, dec, rad);
						} else {
                            con = Circle.Make(dec, ra, rad);
        
                         }
                        _targetregion.Add(con);
						break;
					default:
						break;
				}
			}
			return Error.Ok;
		}
		/************************** END CIRCLE *******************************/
		#endregion
		#region parse_convex
		// ////////////////////////////// parse CONVEX
		private Error parse_convex() {

			Region region = _targetregion;
			Convex convex = null;
			Geometry nextitem;
			double ra, dec, x, y, z, D;

            if (region == null) {
                _error = Error.Ok;
                return _error;
            }
			advance(); // Skip over 'CONVEX'
			_format = peekFormat();
			if (ismore && _format != Format.Null) {
				advance();
			}
			convex = new Convex();
			region.Add(convex);
			while (ismore) {
				nextitem = peekGeometry();
				if (nextitem != Geometry.Null)
					break;
				switch (_format) {
					case Format.Null:
					case Format.Cartesian:
						try {
                            //x = this.getdouble();
                            //y = this.getdouble();
                            //z = this.getdouble();
                            getNormalVector(out x, out y, out z);
							D = this.getdouble();
						} catch {
							if (_error != Error.errEol)
								_error = Error.errIllegalNumber;
							return _error;
						}
						convex.Add(new Halfspace(new Cartesian (x, y, z, false), D));
						break;
					case Format.J2000:
					case Format.Latlon:
						try {
							ra = this.getdouble();
							dec = this.getdouble();
							D = this.getdouble();
						} catch {
							if (_error != Error.errEol)
								_error = Error.errIllegalNumber;
							return _error;
						}
						if (_format == Format.J2000) {
							Cartesian.Radec2Xyz(ra, dec, out x, out y, out z);
						} else {
							Cartesian.Radec2Xyz(dec, ra, out x, out y, out z);
						}
						convex.Add(new Halfspace(new Cartesian (x, y, z, false), D));
						break;
					default:
						break;
				}
			}
			return Error.Ok;
		}
		#endregion
        /// <summary>
        /// Extracts all the points from a convex hull specification
        /// </summary>
        /// <param name="textSpec"></param>
        /// <returns>a linear array of x, y, z values</returns>
        public static List<double> extract(String textSpec) {
            Region reg = new Region();
			Parser par = new Parser();
            Parser.Error err;
			par.input = textSpec;
			par.region = reg; // tell parser where to put the finished product
            err = par.parse();

			if (err != Error.Ok) {
				throw new ParserException(err);
			}
            List<double> result= null;
            if (par._geometry == Parser.Geometry.Chull){
                result = new List<double>(par.xs.Count);
                for (int i = 0; i < par.xs.Count; i++) {
                    result.Add(par.xs[i]);
                    result.Add(par.ys[i]);
                    result.Add(par.zs[i]);
                }
            }
            return result;
		}
            
        
		// <><>
        /// <summary>
        /// Compile a text description of a region into a new Region object.
        /// 
        /// The Shape Grammar is given in these
        /// <a href="../../HtmPrimer/regiongrammar.html">specification.</a>
        /// If there is an error in the specification, compile will
        /// throw a ParserException with an appropriate text message, and and internal
        /// Parser.Error variable.
        /// </summary>
        /// <param name="textSpec"></param>
        /// <returns></returns>
		public static Region compile(String textSpec) {
			Region reg = new Region();
			Parser par = new Parser();
            Parser.Error err;
			par.input = textSpec;
			par.region = reg; // tell parser where to put the finished product
            err = par.parse();

			if (err != Error.Ok) {
				throw new ParserException(err);
			}
			// reg.SimpleSimplify();
			return reg;
		}
        // after you make an instance with par = new Parser(spec);
        // par.region = null;
        // par.parse... if region is null, then no Region is generated.
        // you can then extract the X Y X list

	}
}
