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

public class TaggedLemma : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal TaggedLemma(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(TaggedLemma obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~TaggedLemma() {
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
          morphodita_csharpPINVOKE.delete_TaggedLemma(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public string lemma {
    set {
      morphodita_csharpPINVOKE.TaggedLemma_lemma_set(swigCPtr, value);
      if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
    } 
    get {
      string ret = morphodita_csharpPINVOKE.TaggedLemma_lemma_get(swigCPtr);
      if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
      return ret;
    } 
  }

  public string tag {
    set {
      morphodita_csharpPINVOKE.TaggedLemma_tag_set(swigCPtr, value);
      if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
    } 
    get {
      string ret = morphodita_csharpPINVOKE.TaggedLemma_tag_get(swigCPtr);
      if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
      return ret;
    } 
  }

  public TaggedLemma() : this(morphodita_csharpPINVOKE.new_TaggedLemma(), true) {
  }

}

}
