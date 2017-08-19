﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace LiteDB.Shell
{
    public class Display
    {
        public List<TextWriter> TextWriters { get; set; }
        public bool Pretty { get; set; }

        public Display()
        {
            this.TextWriters = new List<TextWriter>();
            this.Pretty = false;
        }

        public void WriteWelcome()
        {
            this.WriteInfo("Welcome to LiteDB Shell");
            this.WriteInfo("");
            this.WriteInfo("Getting started with `help`");
            this.WriteInfo("");
        }

        public void WritePrompt(string text)
        {
            this.Write(ConsoleColor.White, text);
        }

        public void WriteInfo(string text)
        {
            this.WriteLine(ConsoleColor.Gray, text);
        }

        public void WriteError(string err)
        {
            this.WriteLine(ConsoleColor.Red, err);
        }

        public void WriteHelp(string line1 = null, string line2 = null)
        {
            if (string.IsNullOrEmpty(line1))
            {
                this.WriteLine("");
            }
            else
            {
                this.WriteLine(ConsoleColor.Cyan, line1);

                if (!string.IsNullOrEmpty(line2))
                {
                    this.WriteLine(ConsoleColor.DarkCyan, "    " + line2);
                    this.WriteLine("");
                }
            }
        }

        public void WriteResult(IEnumerable<BsonValue> results)
        {
            var index = 0;

            foreach(var result in results)
            {
                if (result.IsDocument)
                {
                    var doc = result.AsDocument;

                    this.Write(ConsoleColor.Cyan, string.Format("[{0}]:{1}", ++index, this.Pretty ? Environment.NewLine : " "));
                    this.WriteLine(ConsoleColor.DarkCyan, JsonSerializer.Serialize(doc, this.Pretty, false));
                }
                else if (result.IsString)
                {
                    this.WriteLine(ConsoleColor.DarkCyan, result.AsString);
                }
                else if (result.IsNumber)
                {
                    this.WriteLine(ConsoleColor.DarkCyan, result.RawValue.ToString());
                }
                else
                {
                    this.WriteLine(ConsoleColor.DarkCyan, JsonSerializer.Serialize(result, this.Pretty, false));
                }
            }
        }

        #region Print public methods

        public void Write(string text)
        {
            this.Write(Console.ForegroundColor, text);
        }

        public void WriteLine(string text)
        {
            this.WriteLine(Console.ForegroundColor, text);
        }

        public void WriteLine(ConsoleColor color, string text)
        {
            this.Write(color, text + Environment.NewLine);
        }

        public void Write(ConsoleColor color, string text)
        {
            Console.ForegroundColor = color;

            foreach (var writer in this.TextWriters)
            {
                writer.Write(Regex.Unescape(text));
            }
        }

        #endregion Private methods
    }
}