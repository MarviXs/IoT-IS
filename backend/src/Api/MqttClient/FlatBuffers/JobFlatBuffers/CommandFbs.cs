// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace JobFlatBuffers
{

using global::System;
using global::System.Collections.Generic;
using global::Google.FlatBuffers;

public struct CommandFbs : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_25_2_10(); }
  public static CommandFbs GetRootAsCommandFbs(ByteBuffer _bb) { return GetRootAsCommandFbs(_bb, new CommandFbs()); }
  public static CommandFbs GetRootAsCommandFbs(ByteBuffer _bb, CommandFbs obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public CommandFbs __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public string Name { get { int o = __p.__offset(4); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetNameBytes() { return __p.__vector_as_span<byte>(4, 1); }
#else
  public ArraySegment<byte>? GetNameBytes() { return __p.__vector_as_arraysegment(4); }
#endif
  public byte[] GetNameArray() { return __p.__vector_as_array<byte>(4); }
  public double Params(int j) { int o = __p.__offset(6); return o != 0 ? __p.bb.GetDouble(__p.__vector(o) + j * 8) : (double)0; }
  public int ParamsLength { get { int o = __p.__offset(6); return o != 0 ? __p.__vector_len(o) : 0; } }
#if ENABLE_SPAN_T
  public Span<double> GetParamsBytes() { return __p.__vector_as_span<double>(6, 8); }
#else
  public ArraySegment<byte>? GetParamsBytes() { return __p.__vector_as_arraysegment(6); }
#endif
  public double[] GetParamsArray() { return __p.__vector_as_array<double>(6); }

  public static Offset<JobFlatBuffers.CommandFbs> CreateCommandFbs(FlatBufferBuilder builder,
      StringOffset nameOffset = default(StringOffset),
      VectorOffset @paramsOffset = default(VectorOffset)) {
    builder.StartTable(2);
    CommandFbs.AddParams(builder, @paramsOffset);
    CommandFbs.AddName(builder, nameOffset);
    return CommandFbs.EndCommandFbs(builder);
  }

  public static void StartCommandFbs(FlatBufferBuilder builder) { builder.StartTable(2); }
  public static void AddName(FlatBufferBuilder builder, StringOffset nameOffset) { builder.AddOffset(0, nameOffset.Value, 0); }
  public static void AddParams(FlatBufferBuilder builder, VectorOffset paramsOffset) { builder.AddOffset(1, paramsOffset.Value, 0); }
  public static VectorOffset CreateParamsVector(FlatBufferBuilder builder, double[] data) { builder.StartVector(8, data.Length, 8); for (int i = data.Length - 1; i >= 0; i--) builder.AddDouble(data[i]); return builder.EndVector(); }
  public static VectorOffset CreateParamsVectorBlock(FlatBufferBuilder builder, double[] data) { builder.StartVector(8, data.Length, 8); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateParamsVectorBlock(FlatBufferBuilder builder, ArraySegment<double> data) { builder.StartVector(8, data.Count, 8); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateParamsVectorBlock(FlatBufferBuilder builder, IntPtr dataPtr, int sizeInBytes) { builder.StartVector(1, sizeInBytes, 1); builder.Add<double>(dataPtr, sizeInBytes); return builder.EndVector(); }
  public static void StartParamsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(8, numElems, 8); }
  public static Offset<JobFlatBuffers.CommandFbs> EndCommandFbs(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<JobFlatBuffers.CommandFbs>(o);
  }
  public CommandFbsT UnPack() {
    var _o = new CommandFbsT();
    this.UnPackTo(_o);
    return _o;
  }
  public void UnPackTo(CommandFbsT _o) {
    _o.Name = this.Name;
    _o.Params = new List<double>();
    for (var _j = 0; _j < this.ParamsLength; ++_j) {_o.Params.Add(this.Params(_j));}
  }
  public static Offset<JobFlatBuffers.CommandFbs> Pack(FlatBufferBuilder builder, CommandFbsT _o) {
    if (_o == null) return default(Offset<JobFlatBuffers.CommandFbs>);
    var _name = _o.Name == null ? default(StringOffset) : builder.CreateString(_o.Name);
    var _params = default(VectorOffset);
    if (_o.Params != null) {
      var __params = _o.Params.ToArray();
      _params = CreateParamsVector(builder, __params);
    }
    return CreateCommandFbs(
      builder,
      _name,
      _params);
  }
}

public class CommandFbsT
{
  public string Name { get; set; }
  public List<double> Params { get; set; }

  public CommandFbsT() {
    this.Name = null;
    this.Params = null;
  }
}


static public class CommandFbsVerify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyString(tablePos, 4 /*Name*/, false)
      && verifier.VerifyVectorOfData(tablePos, 6 /*Params*/, 8 /*double*/, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}

}
