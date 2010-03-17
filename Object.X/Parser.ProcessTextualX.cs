using System;

namespace Plugin
{
	internal static partial class Parser {
		
		/// <summary>Loads textual .X object data and returns a compatible mesh.</summary>
		/// <param name="fileName">The platform-specific absolute file name of the object.</param>
		/// <param name="text">The text from the .X object file.</param>
		/// <param name="encoding">The fallback encoding.</param>
		/// <returns>A compatible mesh, or null if the object data could not be processed.</returns>
		private static OpenBveApi.Geometry.GenericObject LoadTextualX(string fileName, string text, System.Text.Encoding encoding) {
			/*
			 * load
			 */
			string[] lines = text.Replace("\u000D\u000A", "\u2028").Split(new char[] { '\u000A', '\u000C', '\u000D', '\u0085', '\u2028', '\u2029' }, StringSplitOptions.None);
			/*
			 * strip away comments
			 */
			bool quote = false;
			for (int i = 0; i < lines.Length; i++) {
				for (int j = 0; j < lines[i].Length; j++) {
					if (lines[i][j] == '"') quote = !quote;
					if (!quote) {
						if (lines[i][j] == '#' || j < lines[i].Length - 1 && lines[i].Substring(j, 2) == "//") {
							lines[i] = lines[i].Substring(0, j);
							break;
						}
					}
				}
			}
			/*
			 * strip away header
			 */
			if (lines.Length == 0 || lines[0].Length < 16) {
				IO.ReportError(fileName, 1, "The textual X object file is invalid");
				return null;
			}
			lines[0] = lines[0].Substring(16);
			/*
			 * join lines
			 */
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			for (int i = 0; i < lines.Length; i++) {
				builder.Append(lines[i]);
			}
			string content = builder.ToString();
			/*
			 * parse file
			 */
			int position = 0;
			Structure structure;
			if (!ReadTextualTemplate(fileName, content, ref position, new Template("", new string[] { "[...]" }), false, out structure)) {
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
		 * read textual template
		 */
		/// <summary>Gets structure data for processing and returns true if successful.</summary>
		/// <param name="fileName">The platform-specific absolute file name of the object.</param>
		/// <param name="content">The content of the template.</param>
		/// <param name="position">The character position to start at.</param>
		/// <param name="template">The template.</param>
		/// <param name="inline">Whether the template is inlined.</param>
		/// <param name="structure">Receives a structure for processing.</param>
		/// <returns>True if the operation was a success.</returns>
		private static bool ReadTextualTemplate(string fileName, string content, ref int position, Template template, bool inline, out Structure structure) {
			structure = new Structure(template.Name, new object[] { });
			int i = position; bool q = false;
			int m; for (m = 0; m < template.Members.Length; m++) {
				if (position >= content.Length) break;
				if (template.Members[m] == "[???]") {
					/*
					 * unknown data accepted
					 */
					while (position < content.Length) {
						if (q) {
							if (content[position] == '"') q = false;
						} else {
							if (content[position] == '"') {
								q = true;
							} else if (content[position] == ',' | content[position] == ';') {
								i = position + 1;
							} else if (content[position] == '{') {
								string s = content.Substring(i, position - i).Trim();
								Structure o;
								position++;
								if (!ReadTextualTemplate(fileName, content, ref position, GetTemplate(s), false, out o)) {
									return false;
								} position--;
								i = position + 1;
							} else if (content[position] == '}') {
								position++;
								return true;
							}
						} position++;
					} m--;
				} else if (template.Members[m] == "[...]") {
					/*
					 * any template accepted
					 */
					while (position < content.Length) {
						if (q) {
							if (content[position] == '"') q = false;
						} else {
							if (content[position] == '"') {
								q = true;
							} else if (content[position] == '{') {
								string s = content.Substring(i, position - i).Trim();
								Structure o;
								position++;
								if (!ReadTextualTemplate(fileName, content, ref position, GetTemplate(s), false, out o)) {
									return false;
								} position--;
								Array.Resize<object>(ref structure.Data, structure.Data.Length + 1);
								structure.Data[structure.Data.Length - 1] = o;
								i = position + 1;
							} else if (content[position] == '}') {
								if (inline) {
									IO.ReportError(fileName, false, "Unexpected closing brace encountered in inlined template " + template.Name);
									return false;
								} else {
									position++;
									return true;
								}
							} else if (content[position] == ',') {
								IO.ReportError(fileName, false, "Unexpected comma encountered in template " + template.Name);
								return false;
							} else if (content[position] == ';') {
								if (inline) {
									position++;
									return true;
								} else {
									IO.ReportError(fileName, false, "Unexpected semicolon encountered in template " + template.Name);
									return false;
								}
							}
						} position++;
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
							IO.ReportError(fileName, false, "The internal format description for a template array is invalid in template " + template.Name);
							return false;
						}
						if (h < 0 || h >= structure.Data.Length || !(structure.Data[h] is int)) {
							IO.ReportError(fileName, false, "The internal format description for a template array is invalid in template " + template.Name);
							return false;
						}
						h = (int)structure.Data[h];
					} else {
						IO.ReportError(fileName, false, "The internal format description for a template array is invalid in template " + template.Name);
						return false;
					}
					if (r == "DWORD") {
						/*
						 * dword array
						 */
						int[] o = new int[h];
						if (h == 0) {
							/*
							 * empty array
							 */
							while (position < content.Length) {
								if (content[position] == ';') {
									position++;
									break;
								} else if (!char.IsWhiteSpace(content, position)) {
									IO.ReportError(fileName, false, "Invalid character encountered while processing an array in template " + template.Name);
									return false;
								} else {
									position++;
								}
							}
						} else {
							/*
							 * non-empty array
							 */
							for (int k = 0; k < h; k++) {
								while (position < content.Length) {
									if (content[position] == '{' | content[position] == '}' | content[position] == '"') {
										IO.ReportError(fileName, false, "Invalid character encountered while processing a DWORD array in template " + template.Name);
										return false;
									} else if (content[position] == ',') {
										if (k == h - 1) {
											IO.ReportError(fileName, false, "Invalid character encountered while processing a DWORD array in template " + template.Name);
											return false;
										}
										break;
									} else if (content[position] == ';') {
										if (k != h - 1) {
											IO.ReportError(fileName, false, "Invalid character encountered while processing a DWORD array in template " + template.Name);
											return false;
										}
										break;
									} position++;
								} if (position == content.Length) {
									IO.ReportError(fileName, false, "DWORD array was not terminated at the end of the file in template " + template.Name);
									return false;
								}
								string s = content.Substring(i, position - i);
								position++;
								i = position;
								if (!int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out o[k])) {
									IO.ReportError(fileName, false, "DWORD could not be parsed in array in template " + template.Name);
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
						if (h == 0) {
							/*
							 * empty array
							 */
							while (position < content.Length) {
								if (content[position] == ';') {
									position++;
									break;
								} else if (!char.IsWhiteSpace(content, position)) {
									IO.ReportError(fileName, false, "Invalid character encountered while processing an array in template " + template.Name);
									return false;
								} else {
									position++;
								}
							}
						} else {
							/*
							 * non-empty array
							 */
							for (int k = 0; k < h; k++) {
								while (position < content.Length) {
									if (content[position] == '{' | content[position] == '}' | content[position] == '"') {
										IO.ReportError(fileName, false, "Invalid character encountered while processing a float array in template " + template.Name);
										return false;
									} else if (content[position] == ',') {
										if (k == h - 1) {
											IO.ReportError(fileName, false, "Invalid character encountered while processing a float array in template " + template.Name);
											return false;
										}
										break;
									} else if (content[position] == ';') {
										if (k != h - 1) {
											IO.ReportError(fileName, false, "Invalid character encountered while processing a float array in template " + template.Name);
											return false;
										}
										break;
									} position++;
								} if (position == content.Length) {
									IO.ReportError(fileName, false, "float array was not terminated at the end of the file in template " + template.Name);
									return false;
								}
								string s = content.Substring(i, position - i);
								position++;
								i = position;
								if (!double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out o[k])) {
									IO.ReportError(fileName, false, "float could not be parsed in array in template " + template.Name);
								}
							}
						}
						Array.Resize<object>(ref structure.Data, structure.Data.Length + 1);
						structure.Data[structure.Data.Length - 1] = o;
					} else {
						/*
						 * non-primitive array
						 */
						Template t = GetTemplate(r);
						Structure[] o = new Structure[h];
						if (h == 0) {
							/*
							 * empty array
							 */
							while (position < content.Length) {
								if (content[position] == ';') {
									position++;
									break;
								} else if (!char.IsWhiteSpace(content, position)) {
									IO.ReportError(fileName, false, "Invalid character encountered while processing an array in template " + template.Name);
									return false;
								} else {
									position++;
								}
							}
						} else {
							int k;
							for (k = 0; k < h; k++) {
								if (!ReadTextualTemplate(fileName, content, ref position, t, true, out o[k])) {
									return false;
								}
								if (k < h - 1) {
									/*
									 * most elements
									 */
									while (position < content.Length) {
										if (content[position] == ',') {
											position++;
											break;
										} else if (!char.IsWhiteSpace(content, position)) {
											IO.ReportError(fileName, false, "Invalid character encountered while processing an array in template " + template.Name);
											return false;
										} else {
											position++;
										}
									} if (position == content.Length) {
										IO.ReportError(fileName, false, "Array was not continued at the end of the file in template " + template.Name);
										return false;
									}
								} else {
									/*
									 * last element
									 */
									while (position < content.Length) {
										if (content[position] == ';') {
											position++;
											break;
										} else if (!char.IsWhiteSpace(content, position)) {
											IO.ReportError(fileName, false, "Invalid character encountered while processing an array in template " + template.Name);
											return false;
										} else {
											position++;
										}
									} if (position == content.Length) {
										IO.ReportError(fileName, false, "Array was not terminated at the end of the file in template " + template.Name);
										return false;
									}
								}
							} if (k < h) {
								return false;
							}
						}
						Array.Resize<object>(ref structure.Data, structure.Data.Length + 1);
						structure.Data[structure.Data.Length - 1] = o;
					}
					i = position;
				} else {
					/*
					 * inlined template or primitive expected
					 */
					switch (template.Members[m]) {
						case "DWORD":
							while (position < content.Length) {
								if (content[position] == '{' | content[position] == '}' | content[position] == ',' | content[position] == '"') {
									IO.ReportError(fileName, false, "Invalid character encountered while processing a DWORD in template " + template.Name);
									return false;
								} else if (content[position] == ';') {
									string s = content.Substring(i, position - i).Trim();
									int a; if (!int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out a)) {
										IO.ReportError(fileName, false, "DWORD could not be parsed in template " + template.Name);
										return false;
									}
									Array.Resize<object>(ref structure.Data, structure.Data.Length + 1);
									structure.Data[structure.Data.Length - 1] = a;
									position++;
									i = position;
									break;
								} position++;
							} break;
						case "float":
							while (position < content.Length) {
								if (content[position] == '{' | content[position] == '}' | content[position] == ',' | content[position] == '"') {
									IO.ReportError(fileName, false, "Invalid character encountered while processing a DWORD in template " + template.Name);
									return false;
								} else if (content[position] == ';') {
									string s = content.Substring(i, position - i).Trim();
									double a; if (!double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out a)) {
										IO.ReportError(fileName, false, "float could not be parsed in template " + template.Name);
										return false;
									}
									Array.Resize<object>(ref structure.Data, structure.Data.Length + 1);
									structure.Data[structure.Data.Length - 1] = a;
									position++;
									i = position;
									break;
								} position++;
							} break;
						case "string":
							while (position < content.Length) {
								if (content[position] == '"') {
									position++;
									break;
								} else if (!char.IsWhiteSpace(content, position)) {
									IO.ReportError(fileName, false, "Invalid character encountered while processing a string in template " + template.Name);
									return false;
								} else {
									position++;
								}
							} if (position >= content.Length) {
								IO.ReportError(fileName, false, "Unexpected end of file encountered while processing a string in template " + template.Name);
								return false;
							}
							i = position;
							while (position < content.Length) {
								if (content[position] == '"') {
									position++;
									break;
								} else {
									position++;
								}
							} if (position >= content.Length) {
								IO.ReportError(fileName, false, "Unexpected end of file encountered while processing a string in template " + template.Name);
								return false;
							}
							string t = content.Substring(i, position - i - 1);
							while (position < content.Length) {
								if (content[position] == ';') {
									position++;
									break;
								} else if (!char.IsWhiteSpace(content, position)) {
									IO.ReportError(fileName, false, "Invalid character encountered while processing a string in template " + template.Name);
									return false;
								} else {
									position++;
								}
							} if (position >= content.Length) {
								IO.ReportError(fileName, false, "Unexpected end of file encountered while processing a string in template " + template.Name);
								return false;
							}
							Array.Resize<object>(ref structure.Data, structure.Data.Length + 1);
							structure.Data[structure.Data.Length - 1] = t;
							i = position;
							break;
						default:
							{
								Structure o;
								if (!ReadTextualTemplate(fileName, content, ref position, GetTemplate(template.Members[m]), true, out o)) {
									return false;
								}
								while (position < content.Length) {
									if (content[position] == ';') {
										position++;
										break;
									} else if (!char.IsWhiteSpace(content, position)) {
										IO.ReportError(fileName, false, "Invalid character encountered while processing an inlined template in template " + template.Name);
										return false;
									} else {
										position++;
									}
								} if (position >= content.Length) {
									IO.ReportError(fileName, false, "Unexpected end of file encountered while processing an inlined template in template " + template.Name);
									return false;
								}
								Array.Resize<object>(ref structure.Data, structure.Data.Length + 1);
								structure.Data[structure.Data.Length - 1] = o;
								i = position;
							} break;
					}
				}
			}
			if (m >= template.Members.Length) {
				if (inline) {
					return true;
				} else {
					/*
					 * closed non-inline template
					 */
					while (position < content.Length) {
						if (content[position] == '}') {
							position++;
							break;
						} else if (!char.IsWhiteSpace(content, position)) {
							IO.ReportError(fileName, false, "Invalid character encountered in template " + template.Name);
							return false;
						} else {
							position++;
						}
					} if (position >= content.Length) {
						IO.ReportError(fileName, false, "Unexpected end of file encountered in template " + template.Name);
						return false;
					}
					return true;
				}
			} else {
				if (q) {
					IO.ReportError(fileName, false, "Quotation mark not closed at the end of the file in template " + template.Name);
					return false;
				} else if (template.Name.Length != 0) {
					IO.ReportError(fileName, false, "Unexpected end of file encountered in template " + template.Name);
					return false;
				} else {
					return true;
				}
			}
		}
	}
}