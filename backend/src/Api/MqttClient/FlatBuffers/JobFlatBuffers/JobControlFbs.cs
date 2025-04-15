// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace JobFlatBuffers
{

using global::System;
using global::System.Collections.Generic;
using global::Google.FlatBuffers;

public struct JobControlFbs : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_25_2_10(); }
  public static JobControlFbs GetRootAsJobControlFbs(ByteBuffer _bb) { return GetRootAsJobControlFbs(_bb, new JobControlFbs()); }
  public static JobControlFbs GetRootAsJobControlFbs(ByteBuffer _bb, JobControlFbs obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public JobControlFbs __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public string JobId { get { int o = __p.__offset(4); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetJobIdBytes() { return __p.__vector_as_span<byte>(4, 1); }
#else
  public ArraySegment<byte>? GetJobIdBytes() { return __p.__vector_as_arraysegment(4); }
#endif
  public byte[] GetJobIdArray() { return __p.__vector_as_array<byte>(4); }
  public JobFlatBuffers.JobControlEnumFbs Control { get { int o = __p.__offset(6); return o != 0 ? (JobFlatBuffers.JobControlEnumFbs)__p.bb.GetSbyte(o + __p.bb_pos) : JobFlatBuffers.JobControlEnumFbs.JOB_PAUSE; } }

  public static Offset<JobFlatBuffers.JobControlFbs> CreateJobControlFbs(FlatBufferBuilder builder,
      StringOffset job_idOffset = default(StringOffset),
      JobFlatBuffers.JobControlEnumFbs control = JobFlatBuffers.JobControlEnumFbs.JOB_PAUSE) {
    builder.StartTable(2);
    JobControlFbs.AddJobId(builder, job_idOffset);
    JobControlFbs.AddControl(builder, control);
    return JobControlFbs.EndJobControlFbs(builder);
  }

  public static void StartJobControlFbs(FlatBufferBuilder builder) { builder.StartTable(2); }
  public static void AddJobId(FlatBufferBuilder builder, StringOffset jobIdOffset) { builder.AddOffset(0, jobIdOffset.Value, 0); }
  public static void AddControl(FlatBufferBuilder builder, JobFlatBuffers.JobControlEnumFbs control) { builder.AddSbyte(1, (sbyte)control, 0); }
  public static Offset<JobFlatBuffers.JobControlFbs> EndJobControlFbs(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<JobFlatBuffers.JobControlFbs>(o);
  }
  public JobControlFbsT UnPack() {
    var _o = new JobControlFbsT();
    this.UnPackTo(_o);
    return _o;
  }
  public void UnPackTo(JobControlFbsT _o) {
    _o.JobId = this.JobId;
    _o.Control = this.Control;
  }
  public static Offset<JobFlatBuffers.JobControlFbs> Pack(FlatBufferBuilder builder, JobControlFbsT _o) {
    if (_o == null) return default(Offset<JobFlatBuffers.JobControlFbs>);
    var _job_id = _o.JobId == null ? default(StringOffset) : builder.CreateString(_o.JobId);
    return CreateJobControlFbs(
      builder,
      _job_id,
      _o.Control);
  }
}

public class JobControlFbsT
{
  public string JobId { get; set; }
  public JobFlatBuffers.JobControlEnumFbs Control { get; set; }

  public JobControlFbsT() {
    this.JobId = null;
    this.Control = JobFlatBuffers.JobControlEnumFbs.JOB_PAUSE;
  }
}


static public class JobControlFbsVerify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyString(tablePos, 4 /*JobId*/, false)
      && verifier.VerifyField(tablePos, 6 /*Control*/, 1 /*JobFlatBuffers.JobControlEnumFbs*/, 1, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}

}
