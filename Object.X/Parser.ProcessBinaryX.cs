using System;

namespace Plugin
{
	internal static partial class Parser {

		/// <summary>Loads binary .X object data and returns a compatible mesh.</summary>
		/// <param name="fileName">The platform-specific absolute file name of the object.</param>
		/// <param name="data">The uncompressed data stream from the .X object file.</param>
		/// <param name="encoding">The fallback encoding.</param>
		/// <param name="floatingPointSize">The floating point size used within the data stream.</param>
		/// <returns>A compatible mesh, or null if the object data could not be processed.</returns>
		private static OpenBveApi.Geometry.GenericObject LoadBinaryX(string fileName, byte[] data, int startingPosition, System.Text.Encoding encoding, int floatingPointSize) {
			/*
			 * parse file
			 */
			Structure structure;
			try {
				bool result;
				using (System.IO.MemoryStream stream = new System.IO.MemoryStream(data)) {
					using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream)) {
						stream.Position = startingPosition;
						BinaryCache cache = new BinaryCache();
						cache.IntegersRemaining = 0;
						cache.FloatsRemaining = 0;
						result = ReadBinaryTemplate(fileName, reader, floatingPointSize, new Template("", new string[] { "[...]" }), false, ref cache, out structure);
						reader.Close();
					}
					stream.Close();
				} if (!result) {
					return null;
				}
			} catch (Exception ex) {
				IO.ReportError(fileName, true, "Unhandled error (" + ex.Message + ") encountered");
				return null;
			}
			
			/*
			 * process structure
			 */
			OpenBveApi.Geometry.GenericObject obj;
			if (!ProcessStructure(fileName, structure, out obj, encoding)) {
				return null;
			}
			return obj;
		}
		
		/*
		 * read binary template
		 */
		/// <summary>Gets structure data for processing and returns true if successful.</summary>
		/// <param name="fileName">The platform-specific absolute file name of the object.</param>
		/// <param name="reader">The binary data stream.</param>
		/// <param name="floatingPointSize">The floating point size used within the data stream.</param>
		/// <param name="template">The template.</param>
		/// <param name="inline">Whether the template is inlined.</param>
		/// <param name="cache">The binary cache.</param>
		/// <param name="structure">Receives a structure for processing.</param>
		/// <returns>True if the operation was a success.</returns>
		private static bool ReadBinaryTemplate(string fileName, System.IO.BinaryReader reader, int floatingPointSize, Template template, bool inline, ref BinaryCache cache, out Structure structure) {
			const short TOKEN_NAME = 0x1;
			const short TOKEN_STRING = 0x2;
			const short TOKEN_INTEGER = 0x3;
			const short TOKEN_INTEGER_LIST = 0x6;
			const short TOKEN_FLOAT_LIST = 0x7;
			const short TOKEN_OBRACE = 0xA;
			const short TOKEN_CBRACE = 0xB;
			const short TOKEN_COMMA = 0x13;
			const short TOKEN_SEMICOLON = 0x14;
			structure = new Structure(template.Name, new object[] { });
			System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
			System.Text.ASCIIEncoding ascii = new System.Text.ASCIIEncoding();
			int m; for (m = 0; m < template.Members.Length; m++) {
				if (template.Members[m] == "[???]") {
					/*
					 * unknown template
					 */
					int Level = 0;
					if (cache.IntegersRemaining != 0) {
						IO.ReportError(fileName, true, "An integer list was not depleted at position 0x" + reader.BaseStream.Position.ToString("X", culture));
					} else if (cache.FloatsRemaining != 0) {
						IO.ReportError(fileName, true, "A float list was not depleted at position 0x" + reader.BaseStream.Position.ToString("X", culture));
					}
					short token = reader.ReadInt16();
					switch (token) {
						case TOKEN_NAME:
							{
								Level++;
								int n = reader.ReadInt32();
								if (n < 1) {
									IO.ReportError(fileName, true, "count is invalid in TOKEN_NAME at position 0x" + reader.BaseStream.Position.ToString("X", culture));
									return false;
								}
								reader.BaseStream.Position += n;
								token = reader.ReadInt16();
								if (token != TOKEN_OBRACE) {
									IO.ReportError(fileName, true, "TOKEN_OBRACE expected at position 0x" + reader.BaseStream.Position.ToString("X", culture));
									return false;
								}
							} break;
						case TOKEN_INTEGER:
							{
								reader.BaseStream.Position += 4;
							} break;
						case TOKEN_INTEGER_LIST:
							{
								int n = reader.ReadInt32();
								if (n < 0) {
									IO.ReportError(fileName, true, "count is invalid in TOKEN_INTEGER_LIST at position 0x" + reader.BaseStream.Position.ToString("X", culture));
									return false;
								}
								reader.BaseStream.Position += 4 * n;
							} break;
						case TOKEN_FLOAT_LIST:
							{
								int n = reader.ReadInt32();
								if (n < 0) {
									IO.ReportError(fileName, true, "count is invalid in TOKEN_FLOAT_LIST at position 0x" + reader.BaseStream.Position.ToString("X", culture));
									return false;
								}
								reader.BaseStream.Position += (floatingPointSize >> 3) * n;
							} break;
						case TOKEN_STRING:
							{
								int n = reader.ReadInt32();
								if (n < 0) {
									IO.ReportError(fileName, true, "count is invalid in TOKEN_STRING at position 0x" + reader.BaseStream.Position.ToString("X", culture));
									return false;
								}
								reader.BaseStream.Position += n;
								token = reader.ReadInt16();
								if (token != TOKEN_COMMA & token != TOKEN_SEMICOLON) {
									IO.ReportError(fileName, true, "TOKEN_COMMA or TOKEN_SEMICOLON expected at position 0x" + reader.BaseStream.Position.ToString("X", culture));
									return false;
								}
							} break;
						case TOKEN_OBRACE:
							IO.ReportError(fileName, true, "Unexpected token TOKEN_OBRACE encountered at position 0x" + reader.BaseStream.Position.ToString("X", culture));
							return false;
						case TOKEN_CBRACE:
							if (Level == 0) return true;
							Level--;
							break;
						default:
							IO.ReportError(fileName, true, "Unknown token encountered at position 0x" + reader.BaseStream.Position.ToString("X", culture));
							return false;
					} m--;
				} else if (template.Members[m] == "[...]") {
					/*
					 * any template
					 */
					if (cache.IntegersRemaining != 0) {
						IO.ReportError(fileName, true, "An integer list was not depleted at position 0x" + reader.BaseStream.Position.ToString("X", culture));
					} else if (cache.FloatsRemaining != 0) {
						IO.ReportError(fileName, true, "A float list was not depleted at position 0x" + reader.BaseStream.Position.ToString("X", culture));
					}
					if (template.Name.Length == 0 && reader.BaseStream.Position == reader.BaseStream.Length) {
						/*
						 * end of file
						 */
						return true;
					}
					short token = reader.ReadInt16();
					switch (token) {
						case TOKEN_NAME:
							int n = reader.ReadInt32();
							if (n < 1) {
								IO.ReportError(fileName, true, "count is invalid in TOKEN_NAME at position 0x" + reader.BaseStream.Position.ToString("X", culture));
								return false;
							}
							string Name = new string(ascii.GetChars(reader.ReadBytes(n)));
							token = reader.ReadInt16();
							if (token != TOKEN_OBRACE) {
								IO.ReportError(fileName, true, "TOKEN_OBRACE expected at position 0x" + reader.BaseStream.Position.ToString("X", culture));
								return false;
							}
							Structure o;
							if (!ReadBinaryTemplate(fileName, reader, floatingPointSize, GetTemplate(Name), false, ref cache, out o)) {
								return false;
							}
							Array.Resize<object>(ref structure.Data, structure.Data.Length + 1);
							structure.Data[structure.Data.Length - 1] = o;
							break;
						case TOKEN_CBRACE:
							if (template.Name.Length == 0) {
								IO.ReportError(fileName, true, "Unexpected TOKEN_CBRACE encountered at position 0x" + reader.BaseStream.Position.ToString("X", culture));
								return false;
							}
							m++;
							break;
						default:
							IO.ReportError(fileName, true, "TOKEN_NAME or TOKEN_CBRACE expected at position 0x" + reader.BaseStream.Position.ToString("X", culture));
							return false;
					} m--;
				} else if (template.Members[m].EndsWith("]", StringComparison.Ordinal)) {
					/*
					 * inlined array expected
					 */
					string r = template.Members[m].Substring(0, template.Members[m].Length - 1);
					int h = r.IndexOf('[');
					if (h >= 0) {
						string z = r.Substring(h + 1, r.Length - h - 1);
						r = r.Substring(0, h);
						if (!int.TryParse(z, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out h)) {
							IO.ReportError(fileName, true, "The internal format description for a template array is invalid in template " + template.Name);
							return false;
						}
						if (h < 0 || h >= structure.Data.Length || !(structure.Data[h] is int)) {
							IO.ReportError(fileName, true, "The internal format description for a template array is invalid in template " + template.Name);
							return false;
						}
						h = (int)structure.Data[h];
					} else {
						IO.ReportError(fileName, true, "The internal format description for a template array is invalid in template " + template.Name);
						return false;
					}
					if (r == "DWORD") {
						/*
						 * dword array
						 */
						int[] o = new int[h];
						for (int i = 0; i < h; i++) {
							if (cache.IntegersRemaining != 0) {
								/*
								 * use cached integer
								 */
								int a = cache.Integers[cache.IntegersRemaining - 1];
								cache.IntegersRemaining--;
								o[i] = a;
							} else if (cache.FloatsRemaining != 0) {
								/*
								 * cannot use cached float
								 */
								IO.ReportError(fileName, true, "A float list was not depleted at position 0x" + reader.BaseStream.Position.ToString("X", culture));
								return false;
							} else {
								while (true) {
									short token = reader.ReadInt16();
									if (token == TOKEN_INTEGER) {
										int a = reader.ReadInt32();
										o[i] = a; break;
									} else if (token == TOKEN_INTEGER_LIST) {
										int n = reader.ReadInt32();
										if (n < 0) {
											IO.ReportError(fileName, true, "count is invalid in TOKEN_INTEGER_LIST at position 0x" + reader.BaseStream.Position.ToString("X", culture));
											return false;
										}
										if (n != 0) {
											cache.Integers = new int[n];
											for (int j = 0; j < n; i++) {
												cache.Integers[n - j - 1] = reader.ReadInt32();
											}
											cache.IntegersRemaining = n - 1;
											int a = cache.Integers[cache.IntegersRemaining];
											o[i] = a;
											break;
										}
									} else {
										IO.ReportError(fileName, true, "TOKEN_INTEGER or TOKEN_INTEGER_LIST expected at position 0x" + reader.BaseStream.Position.ToString("X", culture));
										return false;
									}
								}
							}
						}
						Array.Resize<object>(ref structure.Data, structure.Data.Length + 1);
						structure.Data[structure.Data.Length - 1] = o;
					} else if (r == "float") {
						/*
						 * float array
						 */
						double[] o = new double[h];
						for (int i = 0; i < h; i++) {
							if (cache.IntegersRemaining != 0) {
								/*
								 * cannot use cached integer
								 */
								IO.ReportError(fileName, true, "An integer list was not depleted at position 0x" + reader.BaseStream.Position.ToString("X", culture));
								return false;
							} else if (cache.FloatsRemaining != 0) {
								/*
								 * use cached float
								 */
								double a = cache.Floats[cache.FloatsRemaining - 1];
								cache.FloatsRemaining--;
								o[i] = a;
							} else {
								while (true) {
									short token = reader.ReadInt16();
									if (token == TOKEN_FLOAT_LIST) {
										int n = reader.ReadInt32();
										if (n < 0) {
											IO.ReportError(fileName, true, "count is invalid in TOKEN_FLOAT_LIST at position 0x" + reader.BaseStream.Position.ToString("X", culture));
											return false;
										}
										if (n != 0) {
											cache.Floats = new double[n];
											for (int j = 0; j < n; i++) {
												if (floatingPointSize == 32) {
													cache.Floats[n - j - 1] = (double)reader.ReadSingle();
												} else if (floatingPointSize == 64) {
													cache.Floats[n - j - 1] = reader.ReadDouble();
												}
											}
											cache.FloatsRemaining = n - 1;
											double a = cache.Floats[cache.FloatsRemaining];
											o[i] = a;
											break;
										}
									} else {
										IO.ReportError(fileName, true, "TOKEN_FLOAT_LIST expected at position 0x" + reader.BaseStream.Position.ToString("X", culture));
										return false;
									}
								}
							}
						}
						Array.Resize<object>(ref structure.Data, structure.Data.Length + 1);
						structure.Data[structure.Data.Length - 1] = o;
					} else {
						/*
						 * template array
						 */
						Structure[] o = new Structure[h];
						for (int i = 0; i < h; i++) {
							ReadBinaryTemplate(fileName, reader, floatingPointSize, GetTemplate(r), true, ref cache, out o[i]);
						}
						Array.Resize<object>(ref structure.Data, structure.Data.Length + 1);
						structure.Data[structure.Data.Length - 1] = o;
					}
				} else {
					/*
					 * inlined template or primitive expected
					 */
					switch (template.Members[m]) {
						case "DWORD":
							/*
							 * dword expected
							 */
							if (cache.IntegersRemaining != 0) {
								/*
								 * use cached integer
								 */
								int a = cache.Integers[cache.IntegersRemaining - 1];
								cache.IntegersRemaining--;
								Array.Resize<object>(ref structure.Data, structure.Data.Length + 1);
								structure.Data[structure.Data.Length - 1] = a;
							} else if (cache.FloatsRemaining != 0) {
								/*
								 * cannot use cached float
								 */
								IO.ReportError(fileName, true, "A float list was not depleted at position 0x" + reader.BaseStream.Position.ToString("X", culture));
								return false;
							} else {
								/*
								 * read new data
								 */
								while (true) {
									short token = reader.ReadInt16();
									if (token == TOKEN_INTEGER) {
										int a = reader.ReadInt32();
										Array.Resize<object>(ref structure.Data, structure.Data.Length + 1);
										structure.Data[structure.Data.Length - 1] = a;
										break;
									} else if (token == TOKEN_INTEGER_LIST) {
										int n = reader.ReadInt32();
										if (n < 0) {
											IO.ReportError(fileName, true, "count is invalid in TOKEN_INTEGER_LIST at position 0x" + reader.BaseStream.Position.ToString("X", culture));
											return false;
										}
										if (n != 0) {
											cache.Integers = new int[n];
											for (int i = 0; i < n; i++) {
												cache.Integers[n - i - 1] = reader.ReadInt32();
											}
											cache.IntegersRemaining = n - 1;
											int a = cache.Integers[cache.IntegersRemaining];
											Array.Resize<object>(ref structure.Data, structure.Data.Length + 1);
											structure.Data[structure.Data.Length - 1] = a;
											break;
										}
									} else {
										IO.ReportError(fileName, true, "TOKEN_INTEGER or TOKEN_INTEGER_LIST expected at position 0x" + reader.BaseStream.Position.ToString("X", culture));
										return false;
									}
								}
							} break;
						case "float":
							/*
							 * float expected
							 */
							if (cache.IntegersRemaining != 0) {
								/*
								 * cannot use cached integer
								 */
								IO.ReportError(fileName, true, "An integer list was not depleted at position 0x" + reader.BaseStream.Position.ToString("X", culture));
								return false;
							} else if (cache.FloatsRemaining != 0) {
								/*
								 * use cached float
								 */
								double a = cache.Floats[cache.FloatsRemaining - 1];
								cache.FloatsRemaining--;
								Array.Resize<object>(ref structure.Data, structure.Data.Length + 1);
								structure.Data[structure.Data.Length - 1] = a;
							} else {
								/*
								 * read new data
								 */
								while (true) {
									short token = reader.ReadInt16();
									if (token == TOKEN_FLOAT_LIST) {
										int n = reader.ReadInt32();
										if (n < 0) {
											IO.ReportError(fileName, true, "count is invalid in TOKEN_FLOAT_LIST at position 0x" + reader.BaseStream.Position.ToString("X", culture));
											return false;
										}
										if (n != 0) {
											cache.Floats = new double[n];
											for (int i = 0; i < n; i++) {
												if (floatingPointSize == 32) {
													cache.Floats[n - i - 1] = (double)reader.ReadSingle();
												} else if (floatingPointSize == 64) {
													cache.Floats[n - i - 1] = reader.ReadDouble();
												}
											}
											cache.FloatsRemaining = n - 1;
											double a = cache.Floats[cache.FloatsRemaining];
											Array.Resize<object>(ref structure.Data, structure.Data.Length + 1);
											structure.Data[structure.Data.Length - 1] = a;
											break;
										}
									} else {
										IO.ReportError(fileName, true, "TOKEN_FLOAT_LIST expected at position 0x" + reader.BaseStream.Position.ToString("X", culture));
										return false;
									}
								}
							} break;
						case "string":
							{
								/*
								 * string expected
								 */
								if (cache.IntegersRemaining != 0) {
									IO.ReportError(fileName, true, "An integer list was not depleted at position 0x" + reader.BaseStream.Position.ToString("X", culture));
								} else if (cache.FloatsRemaining != 0) {
									IO.ReportError(fileName, true, "A float list was not depleted at position 0x" + reader.BaseStream.Position.ToString("X", culture));
								}
								short token = reader.ReadInt16();
								if (token == TOKEN_STRING) {
									int n = reader.ReadInt32();
									if (n < 0) {
										IO.ReportError(fileName, true, "count is invalid in TOKEN_STRING at position 0x" + reader.BaseStream.Position.ToString("X", culture));
										return false;
									}
									string s = new string(ascii.GetChars(reader.ReadBytes(n)));
									Array.Resize<object>(ref structure.Data, structure.Data.Length + 1);
									structure.Data[structure.Data.Length - 1] = s;
									token = reader.ReadInt16();
									if (token != TOKEN_SEMICOLON) {
										IO.ReportError(fileName, true, "TOKEN_SEMICOLON expected at position 0x" + reader.BaseStream.Position.ToString("X", culture));
										return false;
									}
								} else {
									IO.ReportError(fileName, true, "TOKEN_STRING expected at position 0x" + reader.BaseStream.Position.ToString("X", culture));
									return false;
								}
							} break;
						default:
							/*
							 * inlined template expected
							 */
							Structure o;
							ReadBinaryTemplate(fileName, reader, floatingPointSize, GetTemplate(template.Members[m]), true, ref cache, out o);
							Array.Resize<object>(ref structure.Data, structure.Data.Length + 1);
							structure.Data[structure.Data.Length - 1] = o;
							break;
					}
				}
			}
			if (inline) {
				return true;
			} else {
				string s = template.Members[template.Members.Length - 1];
				if (s != "[???]" & s != "[...]") {
					int token = reader.ReadInt16();
					if (token != TOKEN_CBRACE) {
						IO.ReportError(fileName, true, "TOKEN_CBRACE expected at position 0x" + reader.BaseStream.Position.ToString("X", culture));
						return false;
					}
				}
				return true;
			}
		}
	}
}