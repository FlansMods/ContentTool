using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



public class ExportLogger : IVerificationLogger
{
	public string OpName;
    public List<Verification> Verifications;

	public ExportLogger(string opName)
	{
		OpName = opName;
		Verifications = new List<Verification>();
	}

	public string GetOpName() { return OpName; }
	public List<Verification> GetVerifications() { return Verifications; }
	public void Success(string msg) { Verifications.Add(Verification.Success(msg)); }
	public void Neutral(string msg) { Verifications.Add(Verification.Neutral(msg)); }
	public void Failure(string msg) { Verifications.Add(Verification.Failure(msg)); }
	public void Exception(Exception e) { Verifications.Add(Verification.Exception(e)); }
	public void Exception(Exception e, string msg) { Verifications.Add(Verification.Exception(e, msg)); }

	public void Dispose()
	{
		string UnityRoot = new FileInfo("Assets").FullName;
		string ExportRoot = ContentManager.inst.ExportRoot;

		if(ContentManager.inst.SaveExportLogs)
		{
			if(!Directory.Exists(ContentManager.EXPORT_LOG_ROOT))
			{
				Directory.CreateDirectory(ContentManager.EXPORT_LOG_ROOT);
			}
			string path = $"{ContentManager.EXPORT_LOG_ROOT}/{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}-{OpName}.log";
			using (StringWriter writer = new StringWriter())
			{
				foreach(Verification verification in Verifications)
				{
					writer.WriteLine(
						verification.ToString()
						// Scrub file paths so these can be passed around
							.Replace(UnityRoot, "%UNITY%")
							.Replace(ExportRoot, "%EXPORT%"));
				}

				File.WriteAllText(path, writer.ToString());
			}
		}
	}
}
