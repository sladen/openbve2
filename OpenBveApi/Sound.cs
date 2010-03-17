using System;

namespace OpenBveApi {
	/// <summary>Provides structures and functions for sound management.</summary>
	public static class Sound {
		
		// sound format
		/// <summary>Represents a sound format.</summary>
		public struct SoundFormat {
			// members
			/// <summary>The number of samples per second.</summary>
			public int SampleRate;
			/// <summary>The number of bits per sample. Allowed values are 8 or 16.</summary>
			public int BitsPerSample;
			/// <summary>The number of channels.</summary>
			public int Channels;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="sampleRate">The number of samples per second.</param>
			/// <param name="bitsPerSample">The number of bits per sample. Allowed values are 8 or 16.</param>
			/// <param name="channels">The number of channels.</param>
			public SoundFormat(int sampleRate, int bitsPerSample, int channels) {
				this.SampleRate = sampleRate;
				this.BitsPerSample = bitsPerSample;
				this.Channels = channels;
			}
		}
		
		// sound data
		/// <summary>Represents sound raw data.</summary>
		public class SoundData {
			// members
			/// <summary>The sound format of the raw data.</summary>
			public SoundFormat Format;
			/// <summary>The byte raw data. With 8 bits per sample, values are unsigned from 0 to 255. With 16 bits per sample, values are signed from -32768 to 32767 in little endian byte order. Channels are interleaved in that one sample from each channel is given sequentially before continuing with the next sample.</summary>
			public byte[] Bytes;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="format">The sound format of the raw data.</param>
			/// <param name="bytes">The byte raw data. With 8 bits per sample, values are unsigned from 0 to 255. With 16 bits per sample, values are signed from -32768 to 32767 in little endian byte order. Channels are interleaved in that one sample from each channel is given sequentially before continuing with the next sample.</param>
			public SoundData(SoundFormat format, byte[] bytes) {
				this.Format = format;
				this.Bytes = bytes;
			}
		}
		
		// sound buffer handle
		/// <summary>Represents a handle to a sound buffer as obtained from the host application.</summary>
		public abstract class SoundBufferHandle { }

		// sound source handle
		/// <summary>Represents a handle to a sound source as obtained from the host application.</summary>
		public abstract class SoundSourceHandle { }
		
	}
}