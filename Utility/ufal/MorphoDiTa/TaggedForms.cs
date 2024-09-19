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

public class TaggedForms : global::System.IDisposable, global::System.Collections.IEnumerable, global::System.Collections.Generic.IEnumerable<TaggedForm>
 {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal TaggedForms(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(TaggedForms obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~TaggedForms() {
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
          morphodita_csharpPINVOKE.delete_TaggedForms(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public TaggedForms(global::System.Collections.IEnumerable c) : this() {
    if (c == null)
      throw new global::System.ArgumentNullException("c");
    foreach (TaggedForm element in c) {
      this.Add(element);
    }
  }

  public TaggedForms(global::System.Collections.Generic.IEnumerable<TaggedForm> c) : this() {
    if (c == null)
      throw new global::System.ArgumentNullException("c");
    foreach (TaggedForm element in c) {
      this.Add(element);
    }
  }

  public bool IsFixedSize {
    get {
      return false;
    }
  }

  public bool IsReadOnly {
    get {
      return false;
    }
  }

  public TaggedForm this[int index]  {
    get {
      return getitem(index);
    }
    set {
      setitem(index, value);
    }
  }

  public int Capacity {
    get {
      return (int)capacity();
    }
    set {
      if (value < size())
        throw new global::System.ArgumentOutOfRangeException("Capacity");
      reserve((uint)value);
    }
  }

  public int Count {
    get {
      return (int)size();
    }
  }

  public bool IsSynchronized {
    get {
      return false;
    }
  }

  public void CopyTo(TaggedForm[] array)
  {
    CopyTo(0, array, 0, this.Count);
  }

  public void CopyTo(TaggedForm[] array, int arrayIndex)
  {
    CopyTo(0, array, arrayIndex, this.Count);
  }

  public void CopyTo(int index, TaggedForm[] array, int arrayIndex, int count)
  {
    if (array == null)
      throw new global::System.ArgumentNullException("array");
    if (index < 0)
      throw new global::System.ArgumentOutOfRangeException("index", "Value is less than zero");
    if (arrayIndex < 0)
      throw new global::System.ArgumentOutOfRangeException("arrayIndex", "Value is less than zero");
    if (count < 0)
      throw new global::System.ArgumentOutOfRangeException("count", "Value is less than zero");
    if (array.Rank > 1)
      throw new global::System.ArgumentException("Multi dimensional array.", "array");
    if (index+count > this.Count || arrayIndex+count > array.Length)
      throw new global::System.ArgumentException("Number of elements to copy is too large.");
    for (int i=0; i<count; i++)
      array.SetValue(getitemcopy(index+i), arrayIndex+i);
  }

  public TaggedForm[] ToArray() {
    TaggedForm[] array = new TaggedForm[this.Count];
    this.CopyTo(array);
    return array;
  }

  global::System.Collections.Generic.IEnumerator<TaggedForm> global::System.Collections.Generic.IEnumerable<TaggedForm>.GetEnumerator() {
    return new TaggedFormsEnumerator(this);
  }

  global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() {
    return new TaggedFormsEnumerator(this);
  }

  public TaggedFormsEnumerator GetEnumerator() {
    return new TaggedFormsEnumerator(this);
  }

  // Type-safe enumerator
  /// Note that the IEnumerator documentation requires an InvalidOperationException to be thrown
  /// whenever the collection is modified. This has been done for changes in the size of the
  /// collection but not when one of the elements of the collection is modified as it is a bit
  /// tricky to detect unmanaged code that modifies the collection under our feet.
  public sealed class TaggedFormsEnumerator : global::System.Collections.IEnumerator
    , global::System.Collections.Generic.IEnumerator<TaggedForm>
  {
    private TaggedForms collectionRef;
    private int currentIndex;
    private object currentObject;
    private int currentSize;

    public TaggedFormsEnumerator(TaggedForms collection) {
      collectionRef = collection;
      currentIndex = -1;
      currentObject = null;
      currentSize = collectionRef.Count;
    }

    // Type-safe iterator Current
    public TaggedForm Current {
      get {
        if (currentIndex == -1)
          throw new global::System.InvalidOperationException("Enumeration not started.");
        if (currentIndex > currentSize - 1)
          throw new global::System.InvalidOperationException("Enumeration finished.");
        if (currentObject == null)
          throw new global::System.InvalidOperationException("Collection modified.");
        return (TaggedForm)currentObject;
      }
    }

    // Type-unsafe IEnumerator.Current
    object global::System.Collections.IEnumerator.Current {
      get {
        return Current;
      }
    }

    public bool MoveNext() {
      int size = collectionRef.Count;
      bool moveOkay = (currentIndex+1 < size) && (size == currentSize);
      if (moveOkay) {
        currentIndex++;
        currentObject = collectionRef[currentIndex];
      } else {
        currentObject = null;
      }
      return moveOkay;
    }

    public void Reset() {
      currentIndex = -1;
      currentObject = null;
      if (collectionRef.Count != currentSize) {
        throw new global::System.InvalidOperationException("Collection modified.");
      }
    }

    public void Dispose() {
        currentIndex = -1;
        currentObject = null;
    }
  }

  public void Clear() {
    morphodita_csharpPINVOKE.TaggedForms_Clear(swigCPtr);
  }

  public void Add(TaggedForm x) {
    morphodita_csharpPINVOKE.TaggedForms_Add(swigCPtr, TaggedForm.getCPtr(x));
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  private uint size() {
    uint ret = morphodita_csharpPINVOKE.TaggedForms_size(swigCPtr);
    return ret;
  }

  private uint capacity() {
    uint ret = morphodita_csharpPINVOKE.TaggedForms_capacity(swigCPtr);
    return ret;
  }

  private void reserve(uint n) {
    morphodita_csharpPINVOKE.TaggedForms_reserve(swigCPtr, n);
  }

  public TaggedForms() : this(morphodita_csharpPINVOKE.new_TaggedForms__SWIG_0(), true) {
  }

  public TaggedForms(TaggedForms other) : this(morphodita_csharpPINVOKE.new_TaggedForms__SWIG_1(TaggedForms.getCPtr(other)), true) {
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public TaggedForms(int capacity) : this(morphodita_csharpPINVOKE.new_TaggedForms__SWIG_2(capacity), true) {
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  private TaggedForm getitemcopy(int index) {
    TaggedForm ret = new TaggedForm(morphodita_csharpPINVOKE.TaggedForms_getitemcopy(swigCPtr, index), true);
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private TaggedForm getitem(int index) {
    TaggedForm ret = new TaggedForm(morphodita_csharpPINVOKE.TaggedForms_getitem(swigCPtr, index), false);
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private void setitem(int index, TaggedForm val) {
    morphodita_csharpPINVOKE.TaggedForms_setitem(swigCPtr, index, TaggedForm.getCPtr(val));
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public void AddRange(TaggedForms values) {
    morphodita_csharpPINVOKE.TaggedForms_AddRange(swigCPtr, TaggedForms.getCPtr(values));
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public TaggedForms GetRange(int index, int count) {
    global::System.IntPtr cPtr = morphodita_csharpPINVOKE.TaggedForms_GetRange(swigCPtr, index, count);
    TaggedForms ret = (cPtr == global::System.IntPtr.Zero) ? null : new TaggedForms(cPtr, true);
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void Insert(int index, TaggedForm x) {
    morphodita_csharpPINVOKE.TaggedForms_Insert(swigCPtr, index, TaggedForm.getCPtr(x));
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public void InsertRange(int index, TaggedForms values) {
    morphodita_csharpPINVOKE.TaggedForms_InsertRange(swigCPtr, index, TaggedForms.getCPtr(values));
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public void RemoveAt(int index) {
    morphodita_csharpPINVOKE.TaggedForms_RemoveAt(swigCPtr, index);
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public void RemoveRange(int index, int count) {
    morphodita_csharpPINVOKE.TaggedForms_RemoveRange(swigCPtr, index, count);
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public static TaggedForms Repeat(TaggedForm value, int count) {
    global::System.IntPtr cPtr = morphodita_csharpPINVOKE.TaggedForms_Repeat(TaggedForm.getCPtr(value), count);
    TaggedForms ret = (cPtr == global::System.IntPtr.Zero) ? null : new TaggedForms(cPtr, true);
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void Reverse() {
    morphodita_csharpPINVOKE.TaggedForms_Reverse__SWIG_0(swigCPtr);
  }

  public void Reverse(int index, int count) {
    morphodita_csharpPINVOKE.TaggedForms_Reverse__SWIG_1(swigCPtr, index, count);
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public void SetRange(int index, TaggedForms values) {
    morphodita_csharpPINVOKE.TaggedForms_SetRange(swigCPtr, index, TaggedForms.getCPtr(values));
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

}

}
