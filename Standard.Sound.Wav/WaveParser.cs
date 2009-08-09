using System;

namespace Plugin {
	internal static class WaveParser {
		
		// load from file
		internal static OpenBveApi.General.Result LoadFromFile(string FileName, out OpenBveApi.Sound.SoundData Data) {
			string fileTitle = System.IO.Path.GetFileName(FileName);
			using (System.IO.FileStream stream = new System.IO.FileStream(FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read)) {
				using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream)) {
					// chunk (RIFF)
					uint chunkID = reader.ReadUInt32();
					if (chunkID != 0x46464952) {
						throw new System.IO.InvalidDataException("Invalid chunk ID in " + fileTitle);
					}
					uint chunkSize = reader.ReadUInt32();
					uint riffFormat = reader.ReadUInt32();
					if (riffFormat != 0x45564157) {
						throw new System.IO.InvalidDataException("Unsupported format in " + fileTitle);
					}
					// sub chunks
					OpenBveApi.Sound.SoundFormat format = new OpenBveApi.Sound.SoundFormat();
					byte[] bytes = null;
					while (stream.Position < stream.Length) {
						uint subChunkID = reader.ReadUInt32();
						uint subChunkSize = reader.ReadUInt32();
						if (subChunkID == 0x20746d66) {
							// "fmt " chunk
							if (subChunkSize != 16 & subChunkSize < 18) {
								throw new System.IO.InvalidDataException("Unsupported fmt chunk size in " + fileTitle);
							}
							ushort audioFormat = reader.ReadUInt16();
							if (audioFormat != 1) {
								throw new System.IO.InvalidDataException("Unsupported audioFormat in " + fileTitle);
							}
							ushort numChannels = reader.ReadUInt16();
							uint sampleRate = reader.ReadUInt32();
							uint byteRate = reader.ReadUInt32();
							ushort blockAlign = reader.ReadUInt16();
							ushort bitsPerSample = reader.ReadUInt16();
							if (sampleRate >= 2147483648) {
								throw new System.IO.InvalidDataException("Unsupported sampleRate in " + fileTitle);
							}
							if (bitsPerSample != 8 & bitsPerSample != 16) {
								throw new System.IO.InvalidDataException("Unsupported bitsPerSample in " + fileTitle);
							}
							if (blockAlign != numChannels * bitsPerSample / 8) {
								throw new System.IO.InvalidDataException("Unsupported blockAligm in " + fileTitle);
							}
							if (byteRate != sampleRate * (uint)numChannels * (uint)bitsPerSample / 8) {
								throw new System.IO.InvalidDataException("Unsupported byteRate in " + fileTitle);
							}
							if (subChunkSize >= 18) {
								uint extraParamSize = reader.ReadUInt16();
								if (extraParamSize != subChunkSize - 18) {
									throw new System.IO.InvalidDataException("Invalid extraParamSize in " + fileTitle);
								}
								byte[] extraParams = reader.ReadBytes((int)extraParamSize);
							}
							format.SampleRate = (int)sampleRate;
							format.BitsPerSample = bitsPerSample;
							format.Channels = numChannels;
						} else if (subChunkID == 0x61746164) {
							// "data" chunk
							if (format.SampleRate == 0 | format.BitsPerSample == 0 | format.Channels == 0) {
								throw new System.IO.InvalidDataException("No fmt chunk before data chunk in " + fileTitle);
							}
							if (subChunkSize >= 0x80000000) {
								throw new System.IO.InvalidDataException("Unsupported data chunk size in " + fileTitle);
							}
							uint numSamples = 8 * subChunkSize / ((uint)format.Channels * (uint)format.BitsPerSample);
							bytes = reader.ReadBytes((int)subChunkSize);
							if ((subChunkSize & 1) == 1) {
								stream.Position++;
							}
						} else {
							// unsupported chunk
							stream.Position += (long)subChunkSize;
						}
					}
					// finalize
					if (bytes == null) {
						throw new System.IO.InvalidDataException("No data chunk before the end of the file in " + fileTitle);
					}
					Data = new OpenBveApi.Sound.SoundData(format, bytes);
					return OpenBveApi.General.Result.Successful;
				}
			}
		}
		
	}
}