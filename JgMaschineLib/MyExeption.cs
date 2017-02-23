using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace JgMaschineLib
{
  [Serializable()]
  public class MyException : Exception
  {
    public MyException()
       : base()
    { }

    public MyException(string Fehlertext)
       : base(Fehlertext)
    { }
    public MyException(string FehlerText, Exception InnerException) :
       base(FehlerText, InnerException)
    { }
  }
}
