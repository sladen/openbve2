using System;
using System.IO;

namespace Plugin {
	internal static class WaveParser {
		
		/*
		 * TODO: Add missing XML annotation.
		 * TODO: Replace throwing errors by reporting messages via the host interface.
		 * */
		
		// --- structures and enumerations ---
		
		/// <summary>Represents the endianness of an integer.</summary>
		private enum Endianness {
			/// <summary>Represents little endian byte order, i.e. least-significant byte first.</summary>
			Little = 0,
			/// <summary>Represents big endian byte order, i.e. most-significant byte first.</summary>
			Big = 1
		}
		
		
		// --- format-specific data ---
		
		/// <summary>Represents format-specific data.</summary>
		private abstract class FormatData {
			internal int BlockSize;
		}
		
		/// <summary>Represents PCM-specific data.</summary>
		private class PcmData : FormatData { }
		
		/// <summary>Represents Microsoft-ADPCM-specific data.</summary>
		private class MicrosoftAdPcmData : FormatData {
			// structures
			internal struct ChannelData {
				internal int bPredictor;
				internal short iDelta;
				internal short iSamp1;
				internal short iSamp2;
				internal int iCoef1;
				internal int iCoef2;
			}
			// members
			internal int SamplesPerBlock;
			internal short[][] Coefficients = null;
			// read-only fields
			internal static readonly short[] AdaptionTable = new short[] {
				230, 230, 230, 230, 307, 409, 512, 614,
				768, 614, 512, 409, 307, 230, 230, 230
			};
		}

		
		// --- functions ---
		
		/// <summary>Reads wave data from a RIFF/WAVE/PCM file.</summary>
		/// <param name="fileName">The file name of the RIFF/WAVE/PCM file.</param>
		/// <param name="data"></param>
		/// <returns></returns>
		internal static OpenBveApi.General.Result LoadFromFile(string fileName, out OpenBveApi.Sound.SoundData sound) {
			string fileTitle = Path.GetFileName(fileName);
			byte[] fileBytes = File.ReadAllBytes(fileName);
			using (MemoryStream stream = new MemoryStream(fileBytes)) {
				using (BinaryReader reader = new BinaryReader(stream)) {
					// RIFF/RIFX chunk
					Endianness endianness;
					uint headerCkID = reader.ReadUInt32(); /* Chunk ID is character-based */
					if (headerCkID == 0x46464952) {
						endianness = Endianness.Little;
					} else if (headerCkID == 0x58464952) {
						endianness = Endianness.Big;
					} else {
						throw new InvalidDataException("Invalid chunk ID in " + fileTitle);
					}
					uint headerCkSize = ReadUInt32(reader, endianness);
					uint formType = ReadUInt32(reader, endianness);
					if (formType != 0x45564157) {
						throw new InvalidDataException("Unsupported format in " + fileTitle);
					}
					// data chunks
					OpenBveApi.Sound.SoundFormat format = new OpenBveApi.Sound.SoundFormat();
					FormatData data = null;
					byte[] dataBytes = null;
					while (stream.Position + 8 <= stream.Length) {
						uint ckID = reader.ReadUInt32(); /* Chunk ID is character-based */
						uint ckSize = ReadUInt32(reader, endianness);
						if (ckID == 0x20746d66) {
							// "fmt " chunk
							if (ckSize < 14) {
								throw new InvalidDataException("Unsupported fmt chunk size in " + fileTitle);
							}
							ushort wFormatTag = ReadUInt16(reader, endianness);
							ushort wChannels = ReadUInt16(reader, endianness);
							uint dwSamplesPerSec = ReadUInt32(reader, endianness);
							if (dwSamplesPerSec >= 0x80000000) {
								throw new InvalidDataException("Unsupported dwSamplesPerSec in " + fileTitle);
							}
							uint dwAvgBytesPerSec = ReadUInt32(reader, endianness);
							ushort wBlockAlign = ReadUInt16(reader, endianness);
							if (wFormatTag == 1) {
								// PCM
								if (ckSize < 16) {
									throw new InvalidDataException("Unsupported fmt chunk size in " + fileTitle);
								}
								ushort wBitsPerSample = ReadUInt16(reader, endianness);
								stream.Position += ckSize - 16;
								if (wBitsPerSample < 1) {
									throw new InvalidDataException("Unsupported wBitsPerSample in " + fileTitle);
								}
								if (wBlockAlign != ((wBitsPerSample + 7) / 8) * wChannels) {
									throw new InvalidDataException("Unexpected wBlockAlign in " + fileTitle);
								}
								format.SampleRate = (int)dwSamplesPerSec;
								format.BitsPerSample = (int)wBitsPerSample;
								format.Channels = (int)wChannels;
								PcmData pcmData = new PcmData();
								pcmData.BlockSize = (int)wBlockAlign;
								data = pcmData;
							} else if (wFormatTag == 2) {
								// Microsoft ADPCM
								if (ckSize < 22) {
									throw new InvalidDataException("Unsupported fmt chunk size in " + fileTitle);
								}
								ushort wBitsPerSample = ReadUInt16(reader, endianness);
								if (wBitsPerSample != 4) {
									throw new InvalidDataException("Unsupported wBitsPerSample in " + fileTitle);
								}
								ushort cbSize = ReadUInt16(reader, endianness);
								MicrosoftAdPcmData adpcmData = new MicrosoftAdPcmData();
								adpcmData.SamplesPerBlock = ReadUInt16(reader, endianness);
								if (adpcmData.SamplesPerBlock == 0 | adpcmData.SamplesPerBlock > 2 * ((int)wBlockAlign - 6)) {
									throw new InvalidDataException("Unexpected nSamplesPerBlock in " + fileTitle);
								}
								ushort wNumCoef = ReadUInt16(reader, endianness);
								if (ckSize < 22 + 4 * wNumCoef) {
									throw new InvalidDataException("Unsupported fmt chunk size in " + fileTitle);
								}
								adpcmData.Coefficients = new short[wNumCoef][];
								for (int i = 0; i < wNumCoef; i++) {
									unchecked {
										adpcmData.Coefficients[i] = new short[] {
											(short)ReadUInt16(reader, endianness),
											(short)ReadUInt16(reader, endianness)
										};
									}
								}
								stream.Position += ckSize - (22 + 4 * wNumCoef);
								format.SampleRate = (int)dwSamplesPerSec;
								format.BitsPerSample = 16;
								format.Channels = (int)wChannels;
								adpcmData.BlockSize = wBlockAlign;
								data = adpcmData;
							} else {
								// unsupported format
								throw new InvalidDataException("Unsupported wFormatTag in " + fileTitle);
							}
						} else if (ckID == 0x61746164) {
							// "data" chunk
							if (ckSize >= 0x80000000) {
								throw new InvalidDataException("Unsupported data chunk size in " + fileTitle);
							}
							if (data is PcmData) {
								// PCM
								int bytesPerSample = (format.BitsPerSample + 7) / 8;
								int samples = (int)ckSize / (format.Channels * bytesPerSample);
								int dataSize = samples * format.Channels * bytesPerSample;
								dataBytes = reader.ReadBytes(dataSize);
								stream.Position += ckSize - dataSize;
							} else if (data is MicrosoftAdPcmData) {
								// Microsoft ADPCM
								MicrosoftAdPcmData adpcmData = (MicrosoftAdPcmData)data;
								int blocks = (int)ckSize / adpcmData.BlockSize;
								dataBytes = new byte[2 * blocks * format.Channels * adpcmData.SamplesPerBlock];
								int position = 0;
								for (int i = 0; i < blocks; i++) {
									unchecked {
										MicrosoftAdPcmData.ChannelData[] channelData = new MicrosoftAdPcmData.ChannelData[format.Channels];
										for (int j = 0; j < format.Channels; j++) {
											channelData[j].bPredictor = (int)reader.ReadByte();
											if (channelData[j].bPredictor >= adpcmData.Coefficients.Length) {
												throw new InvalidDataException("Invalid bPredictor in " + fileTitle);
											} else {
												channelData[j].iCoef1 = (int)adpcmData.Coefficients[channelData[j].bPredictor][0];
												channelData[j].iCoef2 = (int)adpcmData.Coefficients[channelData[j].bPredictor][1];
											}
										}
										for (int j = 0; j < format.Channels; j++) {
											channelData[j].iDelta = (short)ReadUInt16(reader, endianness);
										}
										for (int j = 0; j < format.Channels; j++) {
											channelData[j].iSamp1 = (short)ReadUInt16(reader, endianness);
										}
										for (int j = 0; j < format.Channels; j++) {
											channelData[j].iSamp2 = (short)ReadUInt16(reader, endianness);
										}
										for (int j = 0; j < format.Channels; j++) {
											dataBytes[position] = (byte)(ushort)channelData[j].iSamp2;
											dataBytes[position + 1] = (byte)((ushort)channelData[j].iSamp2 >> 8);
											position += 2;
										}
										for (int j = 0; j < format.Channels; j++) {
											dataBytes[position] = (byte)(ushort)channelData[j].iSamp1;
											dataBytes[position + 1] = (byte)((ushort)channelData[j].iSamp1 >> 8);
											position += 2;
										}
										uint nibbleByte = 0;
										bool nibbleFirst = true;
										for (int j = 0; j < adpcmData.SamplesPerBlock - 2; j++) {
											for (int k = 0; k < format.Channels; k++) {
												int lPredSample =
													(int)channelData[k].iSamp1 * channelData[k].iCoef1 +
													(int)channelData[k].iSamp2 * channelData[k].iCoef2 >> 8;
												int iErrorDeltaUnsigned;
												if (nibbleFirst) {
													nibbleByte = (uint)reader.ReadByte();
													iErrorDeltaUnsigned = (int)(nibbleByte >> 4);
													nibbleFirst = false;
												} else {
													iErrorDeltaUnsigned = (int)(nibbleByte & 15);
													nibbleFirst = true;
												}
												int iErrorDeltaSigned =
													iErrorDeltaUnsigned >= 8 ? iErrorDeltaUnsigned - 16 : iErrorDeltaUnsigned;
												int lNewSampInt =
													lPredSample + (int)channelData[k].iDelta * iErrorDeltaSigned;
												short lNewSamp =
													lNewSampInt <= -32768 ? (short)-32768 :
													lNewSampInt >= 32767 ? (short)32767 :
													(short)lNewSampInt;
												channelData[k].iDelta = (short)(
													(int)channelData[k].iDelta *
													(int)MicrosoftAdPcmData.AdaptionTable[iErrorDeltaUnsigned] >> 8
												);
												if (channelData[k].iDelta < 16) {
													channelData[k].iDelta = 16;
												}
												channelData[k].iSamp2 = channelData[k].iSamp1;
												channelData[k].iSamp1 = lNewSamp;
												dataBytes[position] = (byte)(ushort)lNewSamp;
												dataBytes[position + 1] = (byte)((ushort)lNewSamp >> 8);
												position += 2;
											}
										}
									}
									stream.Position += adpcmData.BlockSize - (format.Channels * (adpcmData.SamplesPerBlock - 2) + 1 >> 1) - 7 * format.Channels;
								}
								stream.Position += (int)ckSize - blocks * adpcmData.BlockSize;
							} else {
								// invalid
								throw new InvalidDataException("No fmt chunk before the data chunk in " + fileTitle);
							}
						} else {
							// unsupported chunk
							stream.Position += (long)ckSize;
						}
						// pad byte
						if ((ckSize & 1) == 1) {
							stream.Position++;
						}
					}
					// finalize
					if (dataBytes == null) {
						throw new InvalidDataException("No data chunk before the end of the file in " + fileTitle);
					} else {
						sound = new OpenBveApi.Sound.SoundData(format, dataBytes);
						return OpenBveApi.General.Result.Successful;
					}
				}
			}
		}
		
		/// <summary>Reads a System.UInt32 from a binary reader with the specified endianness.</summary>
		/// <param name="reader">The binary reader.</param>
		/// <param name="endianness">The endianness.</param>
		/// <returns>The System.UInt32 read from the reader.</returns>
		/// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
		/// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
		/// <exception cref="System.IO.EndOfStreamException">The end of the stream is reached.</exception>
		private static uint ReadUInt32(BinaryReader reader, Endianness endianness) {
			uint value = reader.ReadUInt32();
			if (endianness == Endianness.Big) {
				unchecked {
					return (value << 24) | (value & ((uint)0xFF00 << 8)) | ((value & (uint)0xFF0000) >> 8) | (value >> 24);
				}
			} else {
				return value;
			}
		}
		
		/// <summary>Reads a System.UInt16 from a binary reader with the specified endianness.</summary>
		/// <param name="reader">The binary reader.</param>
		/// <param name="endianness">The endianness.</param>
		/// <returns>The System.UInt16 read from the reader.</returns>
		/// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
		/// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
		/// <exception cref="System.IO.EndOfStreamException">The end of the stream is reached.</exception>
		private static ushort ReadUInt16(BinaryReader reader, Endianness endianness) {
			ushort value = reader.ReadUInt16();
			if (endianness == Endianness.Big) {
				unchecked {
					return (ushort)(((uint)value << 8) | ((uint)value >> 8));
				}
			} else {
				return value;
			}
		}
		
	}
}