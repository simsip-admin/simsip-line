using System;

namespace Simsip.LineRunner.Actions
{
	public interface ICopyable
	{
		Object Copy(ICopyable zone);
	}
}

