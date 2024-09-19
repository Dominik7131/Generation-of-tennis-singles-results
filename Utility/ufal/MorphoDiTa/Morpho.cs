//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------

namespace Ufal.MorphoDiTa {

public class Morpho : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal Morpho(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(Morpho obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~Morpho() {
    Dispose(false);
  }

  public void Dispose() {
    Dispose(true);
    global::System.GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          morphodita_csharpPINVOKE.delete_Morpho(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public static Morpho load(string fname) {
    global::System.IntPtr cPtr = morphodita_csharpPINVOKE.Morpho_load(fname);
    Morpho ret = (cPtr == global::System.IntPtr.Zero) ? null : new Morpho(cPtr, true);
    return ret;
  }

  public virtual int analyze(string form, int guesser, TaggedLemmas lemmas) {
    int ret = morphodita_csharpPINVOKE.Morpho_analyze(swigCPtr, form, guesser, TaggedLemmas.getCPtr(lemmas));
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public virtual int generate(string lemma, string tag_wildcard, int guesser, TaggedLemmasForms forms) {
    int ret = morphodita_csharpPINVOKE.Morpho_generate(swigCPtr, lemma, tag_wildcard, guesser, TaggedLemmasForms.getCPtr(forms));
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public string rawLemma(string lemma) {
    string ret = morphodita_csharpPINVOKE.Morpho_rawLemma(swigCPtr, lemma);
    return ret;
  }

  public string lemmaId(string lemma) {
    string ret = morphodita_csharpPINVOKE.Morpho_lemmaId(swigCPtr, lemma);
    return ret;
  }

  public string rawForm(string form) {
    string ret = morphodita_csharpPINVOKE.Morpho_rawForm(swigCPtr, form);
    return ret;
  }

  public virtual Tokenizer newTokenizer() {
    global::System.IntPtr cPtr = morphodita_csharpPINVOKE.Morpho_newTokenizer(swigCPtr);
    Tokenizer ret = (cPtr == global::System.IntPtr.Zero) ? null : new Tokenizer(cPtr, true);
    return ret;
  }

  public virtual Derivator getDerivator() {
    global::System.IntPtr cPtr = morphodita_csharpPINVOKE.Morpho_getDerivator(swigCPtr);
    Derivator ret = (cPtr == global::System.IntPtr.Zero) ? null : new Derivator(cPtr, false);
    return ret;
  }

  public static readonly int NO_GUESSER = morphodita_csharpPINVOKE.Morpho_NO_GUESSER_get();
  public static readonly int GUESSER = morphodita_csharpPINVOKE.Morpho_GUESSER_get();
  public static readonly int GUESSER_UNSPECIFIED = morphodita_csharpPINVOKE.Morpho_GUESSER_UNSPECIFIED_get();

}

}
